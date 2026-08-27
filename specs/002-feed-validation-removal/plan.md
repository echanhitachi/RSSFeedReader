# Implementation Plan: Feed URL Validation & Subscription Removal

**Branch**: `002-feed-validation-removal` | **Date**: 2026-08-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-feed-validation-removal/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Extend the existing subscription-management MVP so that adding a subscription
now validates the URL's format and fetches the URL to confirm it returns a
parseable RSS/Atom feed before accepting it, and so that any subscription can
be individually removed from the list (requiring subscriptions to carry a
stable unique identifier, since duplicate URLs are allowed).

## Technical Context

**Language/Version**: C# / .NET 10 (matches the existing `001-subscription-management` implementation)

**Primary Dependencies**: `System.ServiceModel.Syndication` (feed parsing) and `HttpClient` (fetching candidate feed URLs) on the backend, per TechStack.md's planned Extended-MVP additions. No new frontend dependencies.

**Storage**: In-memory only (unchanged from MVP) — `Subscription` gains an `Id` (Guid) field to support removal.

**Testing**: xUnit for backend validation logic and service Add/Remove behavior (mocking/faking the feed fetch where practical).

**Target Platform**: Same as 001 — local cross-platform dev, ASP.NET Core Web API + Blazor WebAssembly.

**Project Type**: Web application (extends the existing `backend/RSSFeedReader.Api` and `frontend/RSSFeedReader.UI` projects — no new projects).

**Performance Goals**: Feed validation must complete (or time out) within a few seconds so add-subscription stays responsive (SC-003: under 10 seconds end-to-end).

**Constraints**: A fixed, reasonable HTTP timeout (e.g., 5 seconds) for the feed-fetch check (per spec Assumptions); no periodic re-validation of existing subscriptions.

**Scale/Scope**: Same single local user, single session scope as the MVP.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|-----------|-------|--------|
| I. Security by Design | Feed-fetch uses a bounded timeout and does not render fetched content (only extracts a title for validation), avoiding SSRF-amplification/hang risk; no new secrets introduced | PASS |
| II. Code Quality & Maintainability | Validation logic isolated in a dedicated `IFeedValidationService`, kept separate from `ISubscriptionService` storage concerns | PASS |
| III. Incremental MVP-First Delivery | This is an explicit, user-requested extension of MVP scope (was previously deferred); no unrelated features added alongside it | PASS |
| IV. Test-Backed Changes | New unit tests planned for URL-format validation, feed-fetch validation (with a fake/mocked HTTP handler), and service Add/Remove behavior | PASS |
| V. Simplicity & Consistency | Reuses `System.ServiceModel.Syndication` as already anticipated in TechStack.md rather than introducing a new parsing library; no new frontend dependencies | PASS |

No violations — Complexity Tracking table is not needed.

## Project Structure

### Documentation (this feature)

```text
specs/002-feed-validation-removal/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
backend/RSSFeedReader.Api/
├── Models/
│   └── Subscription.cs              # MODIFIED: add Id (Guid)
├── Services/
│   ├── ISubscriptionService.cs      # MODIFIED: Add now takes an already-validated url; add Remove(Guid id)
│   ├── InMemorySubscriptionService.cs
│   ├── IFeedValidationService.cs    # NEW
│   └── FeedValidationService.cs     # NEW: URL format check + HTTP fetch + syndication parse
└── Program.cs                       # MODIFIED: orchestrate validation before Add; new DELETE endpoint

backend.Tests/RSSFeedReader.Api.Tests/
├── SubscriptionServiceTests.cs      # MODIFIED: add Remove tests, update Add tests for Id
└── FeedValidationServiceTests.cs    # NEW

frontend/RSSFeedReader.UI/
├── Services/
│   └── SubscriptionApiClient.cs     # MODIFIED: surface validation errors; add RemoveSubscriptionAsync
└── Pages/
    └── Subscriptions.razor          # MODIFIED: show validation error messages, loading state, remove button per row
```

**Structure Decision**: Extends the existing Web application structure from
`001-subscription-management` in place — no new projects. Feed validation
logic is added as a new backend service (`FeedValidationService`) kept
separate from storage (`InMemorySubscriptionService`) per Principle II
(separation of concerns). The `Subscription` model gains an `Id` to make
individual entries (including duplicate URLs) independently removable.

## Complexity Tracking

*No violations — this section is not applicable.*
