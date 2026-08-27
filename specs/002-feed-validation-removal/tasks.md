---

description: "Task list template for feature implementation"
---

# Tasks: Feed URL Validation & Subscription Removal

**Input**: Design documents from `/specs/002-feed-validation-removal/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/subscriptions-api.md](./contracts/subscriptions-api.md), [quickstart.md](./quickstart.md); builds on the completed `001-subscription-management` implementation.

**Tests**: Included — plan.md commits to xUnit tests for `FeedValidationService` and the updated `InMemorySubscriptionService`.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)

## Phase 1: Setup

**Purpose**: Add the dependency needed for feed parsing

- [X] T001 Add `System.ServiceModel.Syndication` NuGet package reference to `backend/RSSFeedReader.Api/RSSFeedReader.Api.csproj`

**Checkpoint**: Solution still builds with the new package restored.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Model and service changes required by both user stories

- [X] T002 Add `Id` (Guid) to the `Subscription` record in `backend/RSSFeedReader.Api/Models/Subscription.cs`, generated on construction
- [X] T003 Update `ISubscriptionService` in `backend/RSSFeedReader.Api/Services/ISubscriptionService.cs`: `Add` now accepts a pre-validated `url` (no longer does the blank check itself — validation moves to `IFeedValidationService`), and add `bool Remove(Guid id)`
- [X] T004 Update `InMemorySubscriptionService` in `backend/RSSFeedReader.Api/Services/InMemorySubscriptionService.cs` to assign a new `Id` on `Add` and implement `Remove` (returns `true`/`false`, but callers must treat `false` as a no-op per FR-010, not an error)
- [X] T005 [P] Create `IFeedValidationService` in `backend/RSSFeedReader.Api/Services/IFeedValidationService.cs` with a `Task<FeedValidationResult> ValidateAsync(string url)` member
- [X] T006 Implement `FeedValidationService` in `backend/RSSFeedReader.Api/Services/FeedValidationService.cs`: format check via `Uri.TryCreate` (http/https only), then `HttpClient` fetch (via `IHttpClientFactory`, 5s timeout) and `SyndicationFeed.Load` parse, returning a valid/invalid result with a distinguishing error message
- [X] T007 Register `IHttpClientFactory` (`AddHttpClient`) and `IFeedValidationService` in `backend/RSSFeedReader.Api/Program.cs`

**Checkpoint**: Backend has validation and removal logic ready; no endpoints wired yet.

## Phase 3: User Story 1 - Validate a feed URL before adding it (Priority: P1) 🎯 MVP for this feature

**Goal**: Only well-formed URLs that resolve to a real feed can be added; others are rejected with a clear error.

**Independent Test**: Submit `xxx` and a valid feed URL; confirm the former is rejected and the latter is accepted and listed.

- [X] T008 [P] [US1] Write unit tests for `FeedValidationService` in `backend.Tests/RSSFeedReader.Api.Tests/FeedValidationServiceTests.cs` (rejects malformed URL, rejects non-feed/unreachable URL via a fake `HttpMessageHandler`, accepts a valid feed response)
- [X] T009 [US1] Update `POST /api/subscriptions` in `backend/RSSFeedReader.Api/Program.cs` to call `IFeedValidationService.ValidateAsync` before `ISubscriptionService.Add`, returning `400` with the appropriate error message on failure, `201` with `{id, url}` on success
- [X] T010 [US1] Update `AddSubscriptionAsync` in `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` to return the parsed error message on failure (not just a bool) and the created `{id, url}` on success
- [X] T011 [US1] Update `Subscriptions.razor` in `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor` to: disable the Add button and show a loading indicator while the request is in flight (FR-006), display the returned error message on failure, and store the returned `id` alongside the URL in local state on success

**Checkpoint**: Adding subscriptions now enforces format + feed validation end-to-end.

## Phase 4: User Story 2 - Remove a subscription (Priority: P2)

**Goal**: A user can remove any individual subscription, including one of two duplicate entries.

**Independent Test**: With multiple subscriptions listed, remove one and confirm only that entry disappears.

- [X] T012 [P] [US2] Write unit tests for `InMemorySubscriptionService.Remove` in `backend.Tests/RSSFeedReader.Api.Tests/SubscriptionServiceTests.cs` (removes the matching id only, leaves duplicates with a different id intact, removing a non-existent id does not throw)
- [X] T013 [US2] Implement `DELETE /api/subscriptions/{id}` endpoint in `backend/RSSFeedReader.Api/Program.cs` per contracts/subscriptions-api.md (`204` regardless of whether the id existed)
- [X] T014 [US2] Implement `RemoveSubscriptionAsync(Guid id)` in `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` to call the DELETE endpoint
- [X] T015 [US2] Add a "Remove" button per row in `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor`, wired to `RemoveSubscriptionAsync`, updating the displayed list immediately on success (FR-009)

**Checkpoint**: Both validation (US1) and removal (US2) work together as the complete feature.

## Phase 5: Polish & Cross-Cutting Concerns

- [X] T016 [P] Run `dotnet test backend.Tests/RSSFeedReader.Api.Tests` and confirm all tests pass
- [X] T017 [P] `dotnet build` the full solution to confirm no compile errors from the model/service signature changes
- [X] T018 Execute all validation scenarios in [quickstart.md](./quickstart.md) manually in the browser, including the duplicate-removal and already-removed no-op cases

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start here.
- **Foundational (Phase 2)**: Depends on Setup. Blocks both user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational. Independent of US2.
- **User Story 2 (Phase 4)**: Depends on Foundational (specifically T002–T004 for `Id`/`Remove`). Independent of US1's validation logic, though both touch `Subscriptions.razor` (T011 and T015 should be applied sequentially since they edit the same file).
- **Polish (Phase 5)**: Depends on both user stories being complete.

### Parallel Opportunities

- Within Phase 2: T005 can run in parallel with T002/T003; T006 depends on T005; T007 depends on T006.
- Within Phase 3: T008 can run in parallel with T009 (different files).
- Within Phase 4: T012 can run in parallel with T013 (different files).
- T010/T014 (different methods, same file) and T011/T015 (same file, different sections) should be applied sequentially by whoever implements both stories to avoid merge conflicts, even though the underlying work is conceptually independent.
- Within Phase 5: T016 and T017 can run in parallel.

## Implementation Strategy

**MVP first**: Complete Phase 1 → Phase 2 → Phase 3 (User Story 1) to stop
invalid subscriptions from being added — this alone addresses the most
important half of the user's request.

**Incremental delivery**: Add Phase 4 (User Story 2) to allow removing any
subscription, then Phase 5 to validate and confirm both stories work
together correctly.
