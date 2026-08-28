# Implementation Plan: Extended-MVP Feed Refresh & Item Display

**Branch**: `003-feed-refresh-and-items` | **Date**: 2026-08-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-feed-refresh-and-items/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Add the Extended-MVP capability of manually refreshing an individual
subscription to fetch and display its feed items (title + link minimum),
with per-subscription loading and failure states, and no background
polling. Builds on the existing subscription list (001) and add-time feed
validation (002) without changing their behavior.

## Technical Context

**Language/Version**: C# / .NET 10 (matches 001/002)

**Primary Dependencies**: `System.ServiceModel.Syndication` (already added
in 002-feed-validation-removal) and `IHttpClientFactory` (already
registered) on the backend — reused for refresh, not newly introduced. No
new frontend dependencies.

**Storage**: In-memory only, transient per subscription (FR-011) — refreshed
items live only in the backend's in-memory subscription state for the
current process lifetime; not persisted, not part of the `Subscription`
entity's stored identity.

**Testing**: xUnit for backend refresh/parsing logic (reusing the fake
`HttpMessageHandler` pattern from `FeedValidationServiceTests`); manual
browser verification for per-row loading/error UI states.

**Target Platform**: Same as 001/002 — local ASP.NET Core Web API + Blazor
WebAssembly.

**Project Type**: Web application (extends the existing `backend/RSSFeedReader.Api` and `frontend/RSSFeedReader.UI` projects).

**Performance Goals**: Refresh must complete or fail within a few seconds so
the UI stays responsive (SC-001: items visible within 10 seconds).

**Constraints**: Fixed timeout for refresh fetches (reuse the 5s timeout
pattern from `FeedValidationService`); strictly on-demand — no background
scheduling/polling (FR-008).

**Scale/Scope**: Same single local user, single session scope as 001/002.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|-----------|-------|--------|
| I. Security by Design | Refresh fetch reuses the same bounded-timeout, non-rendering `HttpClient` pattern as add-time validation; item titles/links are displayed as plain text (no HTML rendering), avoiding an injection surface | PASS |
| II. Code Quality & Maintainability | Refresh/parsing logic isolated in a new `IFeedRefreshService`, kept separate from `ISubscriptionService` (storage) and `IFeedValidationService` (add-time check), avoiding duplication by sharing the syndication-parsing approach | PASS |
| III. Incremental MVP-First Delivery | Matches the documented Extended-MVP phase exactly (manual refresh + item display, no polling); no persistence or background features introduced | PASS |
| IV. Test-Backed Changes | New unit tests planned for the refresh/parsing service (success, empty feed, missing fields, failure/timeout) reusing the existing fake-handler test pattern | PASS |
| V. Simplicity & Consistency | Reuses existing `System.ServiceModel.Syndication` + `IHttpClientFactory` dependencies already present from 002; no new libraries; refresh state kept transient/in-memory, consistent with existing storage approach | PASS |

No violations — Complexity Tracking table is not needed.

## Project Structure

### Documentation (this feature)

```text
specs/003-feed-refresh-and-items/
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
│   └── FeedItem.cs                  # NEW: Title, Link
├── Services/
│   ├── IFeedRefreshService.cs       # NEW
│   └── FeedRefreshService.cs        # NEW: fetch + parse items for a given url, reusing SyndicationFeed
└── Program.cs                       # MODIFIED: new POST /api/subscriptions/{id}/refresh endpoint

backend.Tests/RSSFeedReader.Api.Tests/
└── FeedRefreshServiceTests.cs       # NEW

frontend/RSSFeedReader.UI/
├── Services/
│   └── SubscriptionApiClient.cs     # MODIFIED: add RefreshSubscriptionAsync
└── Pages/
    └── Subscriptions.razor          # MODIFIED: per-row Refresh button, loading state, items list, error message
```

**Structure Decision**: Extends the existing Web application structure in
place — no new projects. Refresh/parsing logic is a new backend service
(`FeedRefreshService`) kept separate from `InMemorySubscriptionService`
(storage) and `FeedValidationService` (add-time check) per Principle II,
though it reuses the same syndication-parsing approach. Per-subscription
refresh state (loading/succeeded/failed + items) is tracked only in the
frontend's in-memory UI state, not persisted server-side, consistent with
FR-011.

## Complexity Tracking

*No violations — this section is not applicable.*
