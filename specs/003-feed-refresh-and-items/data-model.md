# Data Model: Extended-MVP Feed Refresh & Item Display

## Entity: FeedItem (new)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Title` | `string` | No | Extracted from the feed item; blank/placeholder if the source feed omits it (FR-009). |
| `Link` | `string` | No | Extracted from the feed item; blank/placeholder if the source feed omits it (FR-009). |

**Relationships**: Produced transiently by refreshing a `Subscription`; not
stored against it server-side (see research.md's decision on state
location). A refresh response is a list of `FeedItem` (possibly empty,
FR-010).

**Validation rules**: None beyond best-effort extraction — a missing title
or link does not invalidate the item or the refresh (FR-009).

**Persistence**: Not persisted. Exists only for the duration of a single
refresh request/response and whatever the frontend chooses to hold in its
own transient UI state for the current page view (FR-011).

## Entity: Subscription (unchanged storage shape from 002)

No changes to the stored `Subscription` (`Id`, `Url`). Refresh does not
modify stored subscription data — it only reads `Url` by `Id` to know what
to fetch.

## Concept: RefreshResult (not persisted, response-shape only)

| Field | Type | Notes |
|-------|------|-------|
| `Success` | `bool` | True if fetch + parse succeeded. |
| `Items` | `FeedItem[]?` | Present when `Success` is true (may be empty per FR-010). |
| `ErrorMessage` | `string?` | Present when `Success` is false (FR-007). |

## Concept: Frontend per-row refresh state (UI-only, not an API entity)

| State | Meaning |
|-------|---------|
| `NotRefreshed` | Initial state; no refresh attempted yet this session. |
| `Loading` | A refresh request is in flight for this subscription (FR-006). |
| `Succeeded(items)` | Last refresh succeeded; `items` replaces any prior list (FR-004). |
| `Failed(message)` | Last refresh failed; `message` is shown, no stale items are displayed as if current (FR-007). |
