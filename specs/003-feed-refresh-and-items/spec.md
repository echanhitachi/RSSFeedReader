# Feature Specification: Extended-MVP Feed Refresh & Item Display

**Feature Branch**: `003-feed-refresh-and-items`

**Created**: 2026-08-27

**Status**: Draft

**Input**: User description: "Extended-MVP features per ProjectGoals.md/AppFeatures.md: users can manually refresh a subscribed feed to fetch its content, and see items (title and link minimum) from the feed; basic error handling shows a failure message; no automatic/background polling."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manually refresh a subscription to see its items (Priority: P1)

A user clicks a "refresh" control on a subscription to fetch that feed's
current content and see its items (at minimum, each item's title and link).

**Why this priority**: This is the core Extended-MVP capability — it turns
the subscription list from a static list of URLs into something that shows
actual feed content, which is the next essential step after subscription
management (001) and validated adds (002).

**Independent Test**: With a subscription to a known-good feed (e.g.,
`https://devblogs.microsoft.com/dotnet/feed/`) in the list, click its refresh
control and confirm a list of items (each with a title and link) appears
for that subscription.

**Acceptance Scenarios**:

1. **Given** a subscription to a known-good feed, **When** the user clicks
   refresh for that subscription, **Then** the system fetches the feed and
   displays its items, each showing at least a title and a link.
2. **Given** a subscription that has already been refreshed once, **When**
   the user clicks refresh again, **Then** the displayed items are replaced
   with the current fetch results (not appended/duplicated).
3. **Given** multiple subscriptions in the list, **When** the user refreshes
   one of them, **Then** only that subscription's items are fetched/updated;
   other subscriptions' displayed items (if any) are unaffected.
4. **Given** a refresh is in progress for a subscription, **When** the user
   is waiting for the result, **Then** the UI indicates that subscription is
   loading (e.g., a loading state on that row) rather than appearing
   unresponsive.

---

### User Story 2 - See a clear error when a refresh fails (Priority: P2)

A user clicks refresh on a subscription whose feed cannot be fetched or
parsed, and sees a clear failure message instead of a silent failure or a
crash.

**Why this priority**: Once refresh exists (User Story 1), failures are
inevitable (feed moved, host down, malformed XML). Users need to understand
that a refresh didn't work, but this depends on refresh existing first, so
it is a secondary, complementary story.

**Independent Test**: Refresh a subscription whose feed is unreachable or
invalid and confirm a "failed to load feed" style message is shown for that
subscription, without affecting other subscriptions or crashing the app.

**Acceptance Scenarios**:

1. **Given** a subscription whose URL no longer resolves to a valid feed,
   **When** the user clicks refresh, **Then** the system shows a clear
   failure message for that subscription (e.g., "Failed to load feed") and
   does not display stale or partial item data as if it succeeded.
2. **Given** a refresh failed for one subscription, **When** the user views
   the rest of the list, **Then** other subscriptions and their previously
   fetched items (if any) remain unaffected.
3. **Given** a subscription's refresh previously failed, **When** the user
   clicks refresh again, **Then** the system attempts the fetch again (no
   permanent "broken" state) and updates the message/items based on the new
   attempt's outcome.

---

### Edge Cases

- What happens when a feed has no items (empty feed)? The refresh succeeds
  and an empty items section is shown for that subscription, not treated as
  an error.
- What happens when a feed item is missing a title or link? Missing fields
  are shown as blank/placeholder rather than causing the whole refresh to
  fail.
- What happens when the user navigates away and back? Items fetched by a
  previous refresh are not required to persist across a page reload — since
  storage remains in-memory only, only the subscription list itself persists
  for the session; refreshed items may need to be re-fetched. (This mirrors
  the "no background polling" constraint — content is fetched on demand
  only.)
- What happens when a refresh takes a long time or times out? The system
  applies a reasonable timeout and treats a timeout as a failed refresh
  (User Story 2), the same as any other unreachable feed.
- What happens if the user clicks refresh again while one is already in
  progress for the same subscription? The system ignores the duplicate
  click (or treats it as restarting the same request) rather than issuing
  overlapping fetches whose results could arrive out of order.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST be able to trigger a manual refresh for an
  individual subscription.
- **FR-002**: When a refresh is triggered, the system MUST fetch the
  subscription's feed URL and attempt to parse its items.
- **FR-003**: On a successful refresh, the system MUST display the fetched
  items for that subscription, each showing at minimum a title and a link.
- **FR-004**: On a successful refresh, previously displayed items for that
  subscription (from an earlier refresh) MUST be replaced by the new
  results, not appended to.
- **FR-005**: Refreshing one subscription MUST NOT alter the displayed items
  of any other subscription.
- **FR-006**: The system MUST show a distinct loading indicator for a
  subscription while its refresh is in progress.
- **FR-007**: On a failed refresh (unreachable host, non-feed response,
  malformed feed, or timeout), the system MUST display a clear failure
  message for that subscription (e.g., "Failed to load feed") and MUST NOT
  display partial or stale item data as if the refresh succeeded.
- **FR-008**: The system MUST NOT automatically or periodically refresh any
  subscription in the background — fetching only happens in direct response
  to a user-triggered refresh (per the Extended-MVP's "manual refresh only"
  constraint).
- **FR-009**: A feed item with a missing title or link MUST be displayed
  with a placeholder for the missing field rather than causing the entire
  refresh to fail.
- **FR-010**: An empty feed (zero items) MUST be treated as a successful
  refresh showing zero items, not as a failure.
- **FR-011**: Items fetched via refresh are not required to be persisted;
  they MAY be held only in memory for the current session/page view, aligned
  with the project's existing in-memory-only storage approach.

### Key Entities

- **FeedItem**: Represents a single entry read from a feed during a refresh.
  Attributes: `Title` (string, may be blank if missing from the source feed),
  `Link` (string, may be blank if missing from the source feed). Belongs to
  the subscription that was refreshed to produce it; not persisted beyond
  the current in-memory session (FR-011).
- **Subscription** (extended from 001/002): Gains an associated, transient
  refresh state for UI purposes: not-yet-refreshed / loading / succeeded (with
  its `FeedItem` list) / failed (with an error message). This state exists
  only for the current session and is not part of the subscription's stored
  identity.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can refresh a known-good feed subscription and see its
  items (title + link) displayed within 10 seconds.
- **SC-002**: 100% of refresh attempts against unreachable or invalid feeds
  result in a clear failure message rather than a silent failure, crash, or
  misleading success indication.
- **SC-003**: Refreshing one subscription never changes the displayed items
  of any other subscription in the same session (0% cross-subscription
  interference across repeated testing).
- **SC-004**: A user can tell, without external instructions, whether a
  refresh succeeded, is in progress, or failed, based solely on on-screen
  feedback for that subscription.

## Assumptions

- "Item metadata (minimum)" means title and link only, per AppFeatures.md;
  richer content (summaries, full HTML body, images) remains out of scope
  for this feature and is deferred to later phases (e.g., "Better item
  display" in AppFeatures.md's post-MVP list).
- A reasonable fixed timeout (a few seconds, consistent with the timeout
  already used for add-time feed validation in 002-feed-validation-removal)
  is acceptable for refresh fetches and does not need to be user-configurable.
- No background/scheduled polling is in scope — refresh is strictly
  on-demand per AppFeatures.md ("No automatic polling").
- Items are not required to persist across a page reload or app restart,
  consistent with the project's in-memory-only storage approach; only the
  subscription list itself (from 001/002) persists for the running session.
- This feature builds on the completed `001-subscription-management` and
  `002-feed-validation-removal` features and does not change their existing
  add/remove/list behavior.
