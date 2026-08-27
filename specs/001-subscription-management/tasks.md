---

description: "Task list template for feature implementation"
---

# Tasks: MVP RSS Subscription Management

**Input**: Design documents from `/specs/001-subscription-management/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/subscriptions-api.md](./contracts/subscriptions-api.md), [quickstart.md](./quickstart.md)

**Tests**: Included — plan.md commits to xUnit tests for `InMemorySubscriptionService` per constitution Principle IV (Test-Backed Changes).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Paths follow the Web application structure from plan.md: `backend/RSSFeedReader.Api/`, `backend.Tests/RSSFeedReader.Api.Tests/`, `frontend/RSSFeedReader.UI/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create backend Web API project at `backend/RSSFeedReader.Api/` (ASP.NET Core Minimal API, .NET 8)
- [ ] T002 Create frontend Blazor WebAssembly project at `frontend/RSSFeedReader.UI/`
- [ ] T003 Create backend test project at `backend.Tests/RSSFeedReader.Api.Tests/` referencing xUnit and the API project
- [ ] T004 Remove template demo pages `Home.razor`, `Counter.razor`, `Weather.razor` from `frontend/RSSFeedReader.UI/Pages/`
- [ ] T005 Remove links to deleted demo pages and update root nav label in `frontend/RSSFeedReader.UI/Layout/NavMenu.razor`
- [ ] T006 Configure coordinated ports and API base URL: `backend/RSSFeedReader.Api/Properties/launchSettings.json`, `frontend/RSSFeedReader.UI/Properties/launchSettings.json`, and `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` (`ApiBaseUrl`)

**Checkpoint**: Both projects build and run; frontend shows an empty shell with no ambiguous-route errors.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core model and service required by both user stories — MUST complete before any user story work

- [ ] T007 [P] Create `Subscription` model in `backend/RSSFeedReader.Api/Models/Subscription.cs` (per data-model.md: single `Url` string field)
- [ ] T008 [P] Create `ISubscriptionService` interface in `backend/RSSFeedReader.Api/Services/ISubscriptionService.cs` with `Add(string url)` and `GetAll()` members
- [ ] T009 Implement `InMemorySubscriptionService` in `backend/RSSFeedReader.Api/Services/InMemorySubscriptionService.cs`: thread-safe in-memory store, `Add` rejects empty/whitespace input (FR-008), allows duplicates (FR-007), `GetAll` returns insertion order
- [ ] T010 Register `InMemorySubscriptionService` as a singleton and configure a named CORS policy allowing only the frontend origin(s) from T006 in `backend/RSSFeedReader.Api/Program.cs`
- [ ] T011 [P] Create `SubscriptionApiClient` skeleton in `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` reading `ApiBaseUrl` from configuration (per TechStack.md configuration pattern)

**Checkpoint**: Backend has a working, tested-in-isolation service; frontend has an HTTP client shell ready to call endpoints. No endpoints exist yet.

## Phase 3: User Story 1 - Add a feed subscription by URL (Priority: P1) 🎯 MVP

**Goal**: A user pastes a feed URL and it is added to the subscription list.

**Independent Test**: Enter a feed URL and submit it, then confirm the app accepts the input and reports it was added.

- [ ] T012 [P] [US1] Write unit tests for `InMemorySubscriptionService.Add` in `backend.Tests/RSSFeedReader.Api.Tests/SubscriptionServiceTests.cs` (accepts non-empty URL, rejects blank/whitespace-only input per FR-008, allows duplicate URLs per FR-007)
- [ ] T013 [US1] Implement `POST /api/subscriptions` endpoint in `backend/RSSFeedReader.Api/Program.cs` per contracts/subscriptions-api.md (201 on success, 400 on empty `url`)
- [ ] T014 [US1] Implement `AddSubscriptionAsync` in `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` to call the POST endpoint
- [ ] T015 [US1] Create `Subscriptions.razor` page at `@page "/"` in `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor` with a URL input field and add button
- [ ] T016 [US1] Wire the add button in `Subscriptions.razor` to `AddSubscriptionAsync`, ignore submission when input is blank (client-side guard for FR-008), and update the displayed list immediately on success (FR-003)

**Checkpoint**: A user can add subscriptions and see them appear immediately. User Story 1 is independently functional.

## Phase 4: User Story 2 - View the list of subscriptions (Priority: P2)

**Goal**: A user can see every subscription added so far, including on initial load.

**Independent Test**: Load the app and confirm the subscription list area renders correctly whether empty or populated.

- [ ] T017 [P] [US2] Write unit tests for `InMemorySubscriptionService.GetAll` in `backend.Tests/RSSFeedReader.Api.Tests/SubscriptionServiceTests.cs` (returns empty collection initially, returns entries in insertion order after adds)
- [ ] T018 [US2] Implement `GET /api/subscriptions` endpoint in `backend/RSSFeedReader.Api/Program.cs` per contracts/subscriptions-api.md
- [ ] T019 [US2] Implement `GetSubscriptionsAsync` in `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` to call the GET endpoint
- [ ] T020 [US2] Load and render the subscription list in `Subscriptions.razor` on `OnInitializedAsync`, showing an empty state with no errors when there are no subscriptions

**Checkpoint**: The list reliably reflects current state on load and after adds. User Stories 1 and 2 together deliver the full MVP.

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across both user stories

- [ ] T021 [P] Run `dotnet test backend.Tests/RSSFeedReader.Api.Tests` and confirm all tests pass
- [ ] T022 [P] Verify no ambiguous routes: `dotnet clean` + `dotnet build` on `frontend/RSSFeedReader.UI` and check console output
- [ ] T023 Execute all validation scenarios in [quickstart.md](./quickstart.md) manually in the browser, including the 20-subscription volume check (SC-003) and duplicate-URL check (FR-007)

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start here.
- **Foundational (Phase 2)**: Depends on Setup completion. Blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational completion. No dependency on US2.
- **User Story 2 (Phase 4)**: Depends on Foundational completion. No dependency on US1 (can be built/tested in parallel with US1 by a second developer, though US1 delivers the MVP value first).
- **Polish (Phase 5)**: Depends on both user stories being complete.

### Parallel Opportunities

- Within Phase 1: T001 and T002 can run in parallel (different projects); T004/T005/T006 depend on T002 completing.
- Within Phase 2: T007, T008, T011 can run in parallel; T009 depends on T007+T008; T010 depends on T009.
- Within Phase 3: T012 can run in parallel with T014/T015 (different files); T013 depends on Phase 2 (T009, T010).
- Within Phase 4: T017 can run in parallel with T019/T020 (different files); T018 depends on Phase 2 (T009, T010).
- Phase 3 and Phase 4 can be worked in parallel by different developers once Phase 2 is complete.
- Within Phase 5: T021 and T022 can run in parallel.

## Implementation Strategy

**MVP first**: Complete Phase 1 → Phase 2 → Phase 3 (User Story 1) and stop there for a demonstrable MVP — a user can add subscriptions, even before list-viewing polish (T015 already renders a basic list to show the input worked, satisfying User Story 1's acceptance scenarios standalone).

**Incremental delivery**: Add Phase 4 (User Story 2) next to fully satisfy list-viewing on load/empty-state, then Phase 5 to validate and harden. Each phase leaves the app in a working, demonstrable state.
