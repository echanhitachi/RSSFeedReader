# Research: Feed URL Validation & Subscription Removal

## Decision: Feed parsing library — `System.ServiceModel.Syndication`

- **Decision**: Use `System.ServiceModel.Syndication.SyndicationFeed.Load(XmlReader)` to parse
  the HTTP response body and confirm it is a valid RSS/Atom feed with a title.
- **Rationale**: Already anticipated as the Extended-MVP parsing choice in
  TechStack.md ("Add `System.ServiceModel.Syndication` for basic RSS/Atom
  parsing"). Reusing the planned dependency avoids introducing a new one
  (constitution Principle V: Simplicity).
- **Alternatives considered**: `CodeHollow.FeedReader` (third-party NuGet) —
  rejected to avoid an extra external dependency when the built-in library is
  sufficient for "does this parse as a feed with a title" checks.

## Decision: URL format validation

- **Decision**: Use `Uri.TryCreate(url, UriKind.Absolute, out var uri)` combined
  with a scheme check (`http`/`https` only) as the format check (FR-001).
- **Rationale**: Built into .NET, no dependency needed, sufficient to reject
  clearly malformed input like `xxx` while accepting realistic feed URLs.
- **Alternatives considered**: Regex-based URL validation — rejected as
  `Uri.TryCreate` is the idiomatic, more robust .NET approach.

## Decision: Feed reachability check — direct `HttpClient` fetch with timeout

- **Decision**: A dedicated `HttpClient` (via `IHttpClientFactory`) fetches the
  candidate URL with a fixed timeout (5 seconds) before attempting to parse
  the response as a syndication feed.
- **Rationale**: Satisfies FR-003/FR-004 without needing configuration
  surface for timeout (per spec Assumptions: a fixed timeout is acceptable).
  `IHttpClientFactory` is the standard ASP.NET Core pattern for managing
  outbound `HttpClient` instances safely (connection pooling, DNS
  refresh) and is already used implicitly by the framework.
- **Alternatives considered**: Configurable per-request timeout — rejected as
  unnecessary complexity/config surface beyond what the spec requires.

## Decision: Identifying subscriptions for removal

- **Decision**: Add a `Guid Id` to the `Subscription` model, generated when
  the subscription is added. The frontend receives and tracks this `Id` per
  row and sends it back on removal.
- **Rationale**: FR-007 requires subscriptions to be individually
  identifiable even when the same URL is added more than once (duplicates
  remain allowed per the original MVP's FR-007). A `Url`-keyed removal would
  ambiguously match multiple entries.
- **Alternatives considered**: Positional/index-based removal — rejected as
  fragile (indexes shift on concurrent add/remove; not a stable identifier
  across requests).

## Decision: API surface changes

- **Decision**: `POST /api/subscriptions` now performs format + feed
  validation before storing, returning distinct error responses for
  "invalid format" vs. "feed could not be verified" (still both `400 Bad
  Request`, differentiated by an `error` message, since neither exposes a
  new HTTP status vocabulary need). A new `DELETE /api/subscriptions/{id}`
  endpoint is added for removal, returning `204 No Content` whether or not
  the id existed (per FR-010 — removal of a non-existent entry is a no-op,
  not an error).
- **Rationale**: Keeps the API surface minimal and consistent with the
  existing MVP contract style (see `contracts/subscriptions-api.md` from
  001-subscription-management).
- **Alternatives considered**: `404 Not Found` for removing a non-existent
  id — rejected because FR-010 explicitly requires no user-facing error for
  this case, and a client-visible 404 would complicate the frontend's
  "already removed" handling for no benefit.

## Decision: Frontend UX for validation feedback

- **Decision**: Disable the Add button and show a small loading indicator
  while the POST request is in flight; show the server's error message
  inline on failure; add a "Remove" button per list row.
- **Rationale**: Satisfies FR-006 (visible feedback during validation) and
  keeps the single-page structure from 001-subscription-management rather
  than introducing new pages/navigation (Principle V: Simplicity).
- **Alternatives considered**: A separate "pending validation" list — rejected
  as unnecessary UI complexity for a single-user local MVP extension.

**Output**: All unknowns resolved. Ready for Phase 1 design.
