# API Contract: Subscriptions

Base path: `{ApiBaseUrl}` (configured per environment, e.g.,
`http://localhost:5151/api/`)

## `GET /api/subscriptions`

Returns the current list of subscriptions.

**Response 200 OK**

```json
[
  "https://devblogs.microsoft.com/dotnet/feed/",
  "https://example.com/another-feed.xml"
]
```

- Returns an empty array `[]` when no subscriptions have been added.
- Order reflects insertion order (oldest first).

## `POST /api/subscriptions`

Adds a new subscription.

**Request body**

```json
{
  "url": "https://devblogs.microsoft.com/dotnet/feed/"
}
```

**Response 201 Created** — subscription accepted:

```json
{
  "url": "https://devblogs.microsoft.com/dotnet/feed/"
}
```

**Response 400 Bad Request** — `url` is missing, empty, or whitespace-only
(FR-008):

```json
{
  "error": "url must not be empty"
}
```

**Notes**:

- No uniqueness constraint — submitting the same `url` twice returns
  `201 Created` both times (FR-007).
- No validation that `url` is a well-formed or reachable URL (FR-004).
- Data is not persisted across backend restarts (FR-006).
