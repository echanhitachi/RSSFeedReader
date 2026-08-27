# API Contract: Subscriptions (v2 — validation & removal)

Base path: `{ApiBaseUrl}` (e.g., `http://localhost:5115/`)

This supersedes the `POST /api/subscriptions` behavior from
`001-subscription-management/contracts/subscriptions-api.md` and adds
removal. `GET /api/subscriptions` is unchanged in shape except each item now
includes an `id`.

## `GET /api/subscriptions`

**Response 200 OK**

```json
[
  { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "url": "https://devblogs.microsoft.com/dotnet/feed/" }
]
```

- Returns `[]` when there are no subscriptions.

## `POST /api/subscriptions`

Validates and adds a new subscription.

**Request body**

```json
{ "url": "https://devblogs.microsoft.com/dotnet/feed/" }
```

**Response 201 Created** — format and feed validation both succeeded:

```json
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "url": "https://devblogs.microsoft.com/dotnet/feed/" }
```

**Response 400 Bad Request** — malformed URL (FR-001/FR-002):

```json
{ "error": "url is not a valid http/https URL" }
```

**Response 400 Bad Request** — well-formed URL, but feed could not be
verified (unreachable, timeout, not a parseable feed) (FR-003/FR-004):

```json
{ "error": "could not verify a feed at this url" }
```

**Notes**:

- Duplicate URLs are still allowed as separate entries, each with a new `id`.
- No subscription is stored unless both checks pass (FR-005).

## `DELETE /api/subscriptions/{id}`

Removes a subscription by its `id` (FR-008, FR-009).

**Response 204 No Content** — removed, or the id did not exist (no error is
raised for an already-removed entry per FR-010).

**Notes**:

- Idempotent: calling this twice with the same `id` returns `204` both
  times.
