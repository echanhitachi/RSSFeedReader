# Research: Extended-MVP Feed Refresh & Item Display

## Decision: Reuse `System.ServiceModel.Syndication` for item parsing

- **Decision**: `FeedRefreshService` uses `SyndicationFeed.Load(XmlReader)` —
  the same API already used by `FeedValidationService` (002) — but this time
  extracts the full `Items` collection (title + link per item) instead of
  just checking the feed title.
- **Rationale**: Already a project dependency; no new library needed
  (Principle V). Keeps parsing behavior consistent between add-time
  validation and refresh.
- **Alternatives considered**: A separate, richer feed-parsing library —
  rejected; `SyndicationFeed` is sufficient for title/link-only display
  (FR-003), and TechStack.md explicitly names it as the intended Extended-MVP
  parsing tool.

## Decision: Fetch/timeout pattern reused from `FeedValidationService`

- **Decision**: `FeedRefreshService` uses `IHttpClientFactory` with the same
  5-second timeout pattern as `FeedValidationService`.
- **Rationale**: Consistency (Principle V) and because the spec's Assumptions
  explicitly call out reusing the existing timeout convention.
- **Alternatives considered**: A longer timeout for refresh vs. validation —
  rejected as unnecessary differentiation; both are simple GET + parse
  operations of similar cost.

## Decision: Per-subscription refresh state lives in the frontend only

- **Decision**: The backend refresh endpoint is stateless per call — it
  fetches, parses, and returns items or an error; it does not store a
  "last refresh result" against the subscription server-side. The frontend
  (`Subscriptions.razor`) keeps a per-row state (`NotRefreshed` / `Loading` /
  `Succeeded(items)` / `Failed(message)`) in its own component state.
- **Rationale**: FR-011 says items don't need to persist, and keeping this
  transient state out of `ISubscriptionService`/`InMemorySubscriptionService`
  avoids mixing the stored-subscription concern with ephemeral UI-facing
  fetch results (Principle II: separation of concerns). It also avoids
  growing the backend's in-memory footprint indefinitely across refreshes.
- **Alternatives considered**: Storing last-fetched items in
  `InMemorySubscriptionService` alongside the subscription — rejected as
  scope creep toward a caching feature not requested, and it would blur
  "subscription" (identity) with "fetched content" (transient data).

## Decision: New endpoint shape — `POST /api/subscriptions/{id}/refresh`

- **Decision**: A new endpoint, `POST /api/subscriptions/{id}/refresh`,
  looks up the subscription's URL by `id`, fetches/parses it, and returns
  either the item list or an error payload. `404` if the `id` doesn't exist
  (unlike `DELETE`, a refresh of a non-existent subscription is a genuine
  client error, not a no-op, since there's nothing to refresh).
- **Rationale**: Keeps the "what to refresh" question tied to the existing
  subscription identity (reusing `Id` from 002) rather than requiring the
  client to resend the raw URL, which could drift from what's actually
  stored.
- **Alternatives considered**: `GET /api/feed-items?url=...` (stateless,
  URL-driven) — rejected because it would let the frontend refresh arbitrary
  URLs not in the subscription list, which isn't the feature's intent (only
  refreshing existing subscriptions is in scope).

## Decision: Frontend UX for per-row state

- **Decision**: Each subscription row gets its own "Refresh" button; while a
  refresh is in flight, that row shows a loading indicator and the button is
  disabled for that row only (other rows remain interactive) — this
  directly satisfies FR-006 and User Story 1 Scenario 4.
- **Rationale**: Matches FR-005 (refreshing one subscription must not affect
  others) and keeps the existing single-page structure from 001/002.
- **Alternatives considered**: A single global "refreshing" spinner covering
  the whole page — rejected; would violate FR-005's intent of per-subscription
  independence and would block interacting with other rows unnecessarily.

**Output**: All unknowns resolved. Ready for Phase 1 design.
