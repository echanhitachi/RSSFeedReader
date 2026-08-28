# API Contract: Feed Refresh

Base path: `{ApiBaseUrl}` (e.g., `http://localhost:5115/`)

This adds a new endpoint alongside the existing
`002-feed-validation-removal/contracts/subscriptions-api.md` endpoints
(`GET`/`POST`/`DELETE /api/subscriptions`), which are unchanged.

## `POST /api/subscriptions/{id}/refresh`

Fetches and parses the current feed content for an existing subscription.

**Response 200 OK** — fetch and parse succeeded (FR-003; may be an empty
array per FR-010):

```json
{
  "items": [
    { "title": "Some Post Title", "link": "https://example.com/post-1" },
    { "title": "", "link": "https://example.com/post-2" }
  ]
}
```

**Response 400 Bad Request** — the subscription exists but its feed could
not be fetched/parsed (unreachable, timeout, malformed) (FR-007):

```json
{ "error": "failed to load feed" }
```

**Response 404 Not Found** — no subscription exists with the given `id`:

```json
{ "error": "subscription not found" }
```

**Notes**:

- This endpoint is idempotent in effect (always re-fetches fresh; no
  caching) but each call may return different results if the underlying
  feed changed (FR-002).
- Does not modify the stored subscription (`Id`, `Url` unchanged).
- No automatic/background invocation of this endpoint occurs — it is only
  ever called in direct response to a user action (FR-008).
