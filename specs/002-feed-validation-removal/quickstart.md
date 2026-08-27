# Quickstart: Feed URL Validation & Subscription Removal

## Prerequisites

- `001-subscription-management` implemented and working (backend + frontend
  running per its quickstart.md)

## Setup

No new setup beyond `001-subscription-management` — same two projects, same
ports/CORS configuration.

## Validation scenarios

1. **Reject malformed URL** — Submit `xxx` in the add form.
   - **Expected**: Rejected with an "invalid URL format" error message; not
     added to the list (FR-001, FR-002, SC-001).

2. **Reject unreachable/non-feed URL** — Submit a well-formed URL that isn't
   a feed (e.g., `https://example.com/`).
   - **Expected**: Rejected with a "could not verify a feed" error message;
     not added to the list (FR-003, FR-004, SC-002).

3. **Accept a known-good feed** — Submit
   `https://devblogs.microsoft.com/dotnet/feed/`.
   - **Expected**: Accepted, appears in the list within ~10 seconds (SC-003).

4. **Loading feedback** — Submit a valid feed URL and observe the UI while
   the request is in flight.
   - **Expected**: Add button disabled / loading indicator shown until the
     result is known (FR-006).

5. **Remove a subscription** — With 2+ subscriptions in the list, click
   "Remove" on one.
   - **Expected**: Only that entry disappears; others remain; list updates
     without a page reload, within ~2 seconds (FR-008, FR-009, SC-004).

6. **Remove a duplicate URL entry** — Add the same feed URL twice, then
   remove one of the two entries.
   - **Expected**: Exactly one entry is removed; the other remains (User
     Story 2 Scenario 3).

7. **Remove an already-removed entry** — Trigger removal twice for the same
   id (e.g., double-click, or via a direct API call after removing).
   - **Expected**: No error shown to the user (FR-010).

## Automated tests

```powershell
dotnet test backend.Tests/RSSFeedReader.Api.Tests
```

**Expected**: All tests pass, covering `FeedValidationService` (format +
feed checks) and `InMemorySubscriptionService` Add/Remove behavior — see
[data-model.md](./data-model.md) and
[contracts/subscriptions-api.md](./contracts/subscriptions-api.md).
