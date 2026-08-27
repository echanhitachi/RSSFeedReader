# Implementation Plan: MVP RSS Subscription Management

**Branch**: `001-subscription-management` | **Date**: 2026-08-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-subscription-management/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Deliver the MVP capability for the RSS feed reader: a user can add a feed
subscription by pasting a URL and immediately see it reflected in a displayed
list. No feed fetching, parsing, validation, or persistence is in scope. The
technical approach uses an ASP.NET Core Web API backend holding subscriptions
in an in-memory list, and a Blazor WebAssembly frontend that posts new
subscriptions and displays the current list, per the project's documented
tech stack.

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core Web API + Blazor WebAssembly)
— updated from the originally planned .NET 8 because only the .NET 10 SDK/runtime is installed in the local dev environment.

**Primary Dependencies**: ASP.NET Core Minimal API (backend), Blazor
WebAssembly + `HttpClient` (frontend). No feed-parsing or persistence
libraries — explicitly out of scope for MVP.

**Storage**: In-memory only (a thread-safe in-process list on the backend).
No database; subscriptions are lost on restart per FR-006.

**Testing**: xUnit for backend API/service logic; manual browser
verification for frontend UI per the project's local development checklist.

**Target Platform**: Cross-platform local development (Windows/macOS/Linux),
run via `dotnet run`; browser-hosted Blazor WebAssembly client.

**Project Type**: Web application (separate `backend/` API and `frontend/`
Blazor WebAssembly UI projects, per TechStack.md).

**Performance Goals**: N/A for MVP — local single-user demo; SC-001 requires
new subscriptions to appear in under 5 seconds (trivially met by direct
in-memory operations with no network latency beyond local HTTP calls).

**Constraints**: In-memory storage only (FR-006); no feed URL validation
(FR-004); duplicates allowed (FR-007); CORS must allow only the configured
frontend origin per the constitution's least-privilege requirement.

**Scale/Scope**: Single local user, single session; SC-003 requires
supporting at least 20 subscriptions without display failure.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|-----------|-------|--------|
| I. Security by Design | CORS policy restricted to the frontend's actual configured origin; no secrets/connection strings needed for in-memory MVP; input is stored as opaque text (no rendering of feed content, so no injection surface yet) | PASS |
| II. Code Quality & Maintainability | Backend/frontend concerns separated (API vs. UI); no template demo pages will be left in place; small single-purpose endpoint/service/component | PASS |
| III. Incremental MVP-First Delivery | Scope strictly limited to add + list; no fetching, persistence, removal, or polling introduced | PASS |
| IV. Test-Backed Changes | Add/list is near-trivial pass-through logic; a minimal xUnit test suite will still cover the subscription service (add, list, empty-input rejection) to guard FR-003/FR-008 behavior | PASS |
| V. Simplicity & Consistency | No dependencies beyond ASP.NET Core + Blazor; API base URL and CORS origin centrally configured, not hardcoded in multiple places | PASS |

No violations — Complexity Tracking table is not needed.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
backend/
└── RSSFeedReader.Api/
    ├── Models/
    │   └── Subscription.cs
    ├── Services/
    │   ├── ISubscriptionService.cs
    │   └── InMemorySubscriptionService.cs
    ├── Program.cs                 # Minimal API endpoints + CORS config
    └── appsettings.json

backend.Tests/
└── RSSFeedReader.Api.Tests/
    └── SubscriptionServiceTests.cs

frontend/
└── RSSFeedReader.UI/
    ├── Pages/
    │   └── Subscriptions.razor    # @page "/" — add form + subscription list
    ├── Services/
    │   └── SubscriptionApiClient.cs
    ├── Layout/
    │   └── NavMenu.razor          # updated to remove template demo links
    ├── Program.cs                 # reads ApiBaseUrl from configuration
    └── wwwroot/appsettings.json   # ApiBaseUrl setting
```

**Structure Decision**: Web application structure (Option 2) — separate
`backend/RSSFeedReader.Api` (ASP.NET Core Web API) and
`frontend/RSSFeedReader.UI` (Blazor WebAssembly) projects, matching
TechStack.md. A single `Subscriptions.razor` page at the root route (`/`)
covers both User Story 1 (add) and User Story 2 (view), since they share one
screen in the MVP. Template demo pages (`Home.razor`, `Counter.razor`,
`Weather.razor`) are removed as part of setup, per the constitution's
Technology & Architecture Constraints.

## Complexity Tracking

*No violations — this section is not applicable.*
