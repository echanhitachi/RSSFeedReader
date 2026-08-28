# Quickstart: Extended-MVP Feed Refresh & Item Display

## Prerequisites

- `001-subscription-management` and `002-feed-validation-removal` implemented
  and working

## Setup

No new setup beyond the existing projects/ports/CORS configuration.

## Validation scenarios

1. **Refresh a known-good feed** — Add
   `https://devblogs.microsoft.com/dotnet/feed/` as a subscription, then
   click its Refresh button.
   - **Expected**: Items (title + link) appear for that subscription within
     ~10 seconds (SC-001, User Story 1 Scenario 1).

2. **Refresh again replaces items** — Click Refresh a second time on the
   same subscription.
   - **Expected**: The item list is replaced, not appended to (FR-004, User
     Story 1 Scenario 2).

3. **Refreshing one subscription doesn't affect others** — With two
   subscriptions in the list, refresh only one.
   - **Expected**: Only the refreshed subscription's items change; the other
     subscription's display is untouched (FR-005, SC-003).

4. **Loading state** — Click Refresh and observe the row while the request
   is in flight.
   - **Expected**: That row shows a loading indicator; other rows remain
     interactive (FR-006, User Story 1 Scenario 4).

5. **Failed refresh** — Refresh a subscription whose URL no longer resolves
   to a valid feed (e.g., manually stop the backend's reachability to that
   host, or use a feed that returns non-feed content).
   - **Expected**: A clear "Failed to load feed" message appears for that
     subscription; no items are shown as if it succeeded (FR-007, SC-002,
     User Story 2 Scenario 1).

6. **Retry after failure** — Click Refresh again on a subscription that
   previously failed.
   - **Expected**: The system attempts the fetch again and updates based on
     the new outcome (User Story 2 Scenario 3).

7. **No background refresh** — Leave the app open without clicking Refresh.
   - **Expected**: No network requests to any feed occur automatically
     (verify via browser DevTools Network tab) — confirms FR-008.

## Automated tests

```powershell
dotnet test backend.Tests/RSSFeedReader.Api.Tests
```

**Expected**: All tests pass, covering `FeedRefreshService` (successful
parse with items, empty feed, missing title/link fields, and
failure/timeout cases) — see [data-model.md](./data-model.md) and
[contracts/feed-refresh-api.md](./contracts/feed-refresh-api.md).
