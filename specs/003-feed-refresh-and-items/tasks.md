---

description: "Task list template for feature implementation"
---

# Tasks: Extended-MVP Feed Refresh & Item Display

**Input**: Design documents from `/specs/003-feed-refresh-and-items/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/feed-refresh-api.md](./contracts/feed-refresh-api.md), [quickstart.md](./quickstart.md); builds on the completed `001-subscription-management` and `002-feed-validation-removal` implementations.

**Tests**: Included — plan.md commits to xUnit tests for `FeedRefreshService` per constitution Principle IV (Test-Backed Changes).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)

## Phase 1: Setup

**Purpose**: No new dependencies needed — `System.ServiceModel.Syndication` and `IHttpClientFactory` are already present from 002-feed-validation-removal.

- [X] T001 Confirm `backend/RSSFeedReader.Api/RSSFeedReader.Api.csproj` already references `System.ServiceModel.Syndication` (added in 002) and the solution builds cleanly before starting

**Checkpoint**: Solution builds; no new packages required.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Model and service scaffolding required by both user stories

- [X] T002 [P] Create `FeedItem` model in `backend/RSSFeedReader.Api/Models/FeedItem.cs` (per data-model.md: `Title`, `Link`, both optional/blank-tolerant)
- [X] T003 [P] Create `IFeedRefreshService` in `backend/RSSFeedReader.Api/Services/IFeedRefreshService.cs` with a `Task<FeedRefreshResult> RefreshAsync(string url)` member returning a result type with `Success`, `Items`, `ErrorMessage`
- [X] T004 Implement `FeedRefreshService` in `backend/RSSFeedReader.Api/Services/FeedRefreshService.cs`: reuse the `IHttpClientFactory` + 5s timeout pattern from `FeedValidationService`, fetch the url, parse with `SyndicationFeed.Load`, map each `SyndicationItem` to a `FeedItem` (blank `Title`/`Link` when missing, per FR-009), return success with items (including empty list per FR-010) or a failure result on fetch/parse/timeout error
- [X] T005 Register `IFeedRefreshService` (and its own named `HttpClient` via `AddHttpClient`) in `backend/RSSFeedReader.Api/Program.cs`

**Checkpoint**: Backend has a working, tested-in-isolation refresh/parsing service. No endpoint exists yet.

## Phase 3: User Story 1 - Manually refresh a subscription to see its items (Priority: P1) 🎯 MVP for this feature

**Goal**: A user can click Refresh on a subscription and see its items (title + link), with per-row loading feedback, and refreshing again replaces the list.

**Independent Test**: Refresh a known-good feed subscription and confirm items with titles and links appear; refresh again and confirm the list is replaced, not appended.

- [X] T006 [P] [US1] Write unit tests for `FeedRefreshService` in `backend.Tests/RSSFeedReader.Api.Tests/FeedRefreshServiceTests.cs` (successful parse returns items with title/link, empty feed returns an empty item list, item missing title/link is returned with a blank field rather than failing)
- [X] T007 [US1] Implement `POST /api/subscriptions/{id}/refresh` in `backend/RSSFeedReader.Api/Program.cs`: look up the subscription's `Url` via `ISubscriptionService`, return `404` if not found, otherwise call `IFeedRefreshService.RefreshAsync` and return `200` with `{items}` per contracts/feed-refresh-api.md
- [X] T008 [US1] Add `RefreshSubscriptionAsync(Guid id)` to `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs`, returning a result type with success/items/error matching the endpoint contract
- [X] T009 [US1] Add a per-row "Refresh" button and items display in `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor`: track per-subscription state (not-refreshed/loading/succeeded/failed) in component state, disable/show a loading indicator on the row being refreshed only (FR-005, FR-006), and render the returned items' title + link, replacing any prior list for that row (FR-004)

**Checkpoint**: A user can refresh any subscription independently and see its current items. User Story 1 is independently functional.

## Phase 4: User Story 2 - See a clear error when a refresh fails (Priority: P2)

**Goal**: A failed refresh shows a clear per-subscription error message instead of a silent failure, stale data, or crash, and a subsequent refresh attempt is not permanently blocked.

**Independent Test**: Refresh a subscription pointing at an unreachable/invalid feed and confirm a "failed to load feed" message appears for that row only, and clicking refresh again retries cleanly.

- [X] T010 [P] [US2] Write unit tests for `FeedRefreshService` failure paths in `backend.Tests/RSSFeedReader.Api.Tests/FeedRefreshServiceTests.cs` (unreachable host/non-2xx response, non-feed content, timeout all return a failure result with an error message, not an exception)
- [X] T011 [US2] Update `POST /api/subscriptions/{id}/refresh` in `backend/RSSFeedReader.Api/Program.cs` to return `400` with `{error: "failed to load feed"}` when `IFeedRefreshService.RefreshAsync` reports failure (per contracts/feed-refresh-api.md)
- [X] T012 [US2] Update `Subscriptions.razor` in `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor` to show the row's Failed state with the returned error message when refresh fails, without displaying stale/partial items as if it succeeded, and ensure clicking Refresh again on a failed row re-attempts cleanly (resets to Loading, then Succeeded/Failed based on the new outcome)

**Checkpoint**: Both successful and failed refreshes are handled correctly and independently per subscription. User Stories 1 and 2 together deliver the full Extended-MVP feature.

## Phase 5: Polish & Cross-Cutting Concerns

- [X] T013 [P] Run `dotnet test backend.Tests/RSSFeedReader.Api.Tests` and confirm all tests pass
- [X] T014 [P] `dotnet build` the full solution to confirm no compile errors from the new model/service/endpoint additions
- [X] T015 Execute all validation scenarios in [quickstart.md](./quickstart.md) manually in the browser, including the no-background-refresh check via browser DevTools Network tab (FR-008)

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start here.
- **Foundational (Phase 2)**: Depends on Setup. Blocks both user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational. Independent of US2's failure-specific work, though both extend the same endpoint/page.
- **User Story 2 (Phase 4)**: Depends on Foundational (T004's service) and benefits from US1's endpoint/UI scaffolding (T007, T009) already existing, since it adds the failure branch to the same endpoint and component rather than creating new ones.
- **Polish (Phase 5)**: Depends on both user stories being complete.

### Parallel Opportunities

- Within Phase 2: T002 and T003 can run in parallel; T004 depends on both; T005 depends on T004.
- Within Phase 3: T006 can run in parallel with T007/T008 (different files).
- Within Phase 4: T010 can run in parallel with T011 (different files).
- T007/T011 (same endpoint, same file) and T009/T012 (same component, same file) should be applied sequentially by whoever implements both stories, even though the underlying logic (success vs. failure branch) is conceptually independent.
- Within Phase 5: T013 and T014 can run in parallel.

## Implementation Strategy

**MVP first**: Complete Phase 1 → Phase 2 → Phase 3 (User Story 1) to deliver
manual refresh with item display for the happy path — the core Extended-MVP
value.

**Incremental delivery**: Add Phase 4 (User Story 2) to harden failure
handling, then Phase 5 to validate both stories work together and confirm no
background polling was accidentally introduced.
