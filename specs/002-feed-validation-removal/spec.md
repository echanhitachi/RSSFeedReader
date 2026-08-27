# Feature Specification: Feed URL Validation & Subscription Removal

**Feature Branch**: `002-feed-validation-removal`

**Created**: 2026-08-27

**Status**: Draft

**Input**: User description: "1. Validate that the feed url is valid in format and actually returns feed metadata. 2. Provide ability to remove an invalid feed subscription."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validate a feed URL before adding it (Priority: P1)

A user pastes a URL to subscribe to. Before the subscription is added, the
system checks that the URL is well-formed and that it actually points to a
reachable RSS/Atom feed (by fetching it and confirming feed metadata, such as
a title, can be parsed from the response).

**Why this priority**: Without validation, users can add subscriptions that
will never produce content, undermining trust in the list. This directly
replaces the MVP's "no validation" behavior, which was only ever a temporary
simplification.

**Independent Test**: Submit a malformed string (e.g., `xxx`) and confirm it
is rejected with an error message; submit a valid feed URL (e.g.,
`https://devblogs.microsoft.com/dotnet/feed/`) and confirm it is accepted and
added to the list.

**Acceptance Scenarios**:

1. **Given** the add-subscription form, **When** the user submits text that
   is not a well-formed URL (e.g., `xxx`), **Then** the subscription is
   rejected and an error message is shown explaining the URL is invalid.
2. **Given** the add-subscription form, **When** the user submits a
   well-formed URL that does not return parseable feed content (e.g., a page
   that isn't RSS/Atom, or an unreachable host), **Then** the subscription is
   rejected and an error message is shown explaining the feed could not be
   verified.
3. **Given** the add-subscription form, **When** the user submits a
   well-formed URL that returns a valid RSS/Atom feed, **Then** the
   subscription is added and appears in the list.
4. **Given** a feed check is in progress, **When** the user is waiting for
   the result, **Then** the UI indicates validation is happening (e.g., a
   loading/disabled state) rather than appearing unresponsive.

---

### User Story 2 - Remove a subscription (Priority: P2)

A user removes a subscription from their list, primarily to clear out one
that turned out to be invalid or is no longer wanted.

**Why this priority**: Once validation exists, users still need a way to
correct mistakes (e.g., a feed that passed validation but is no longer
desired, or was added before this feature existed) or clean up their list.
Depends on subscriptions being individually identifiable, which User Story 1's
underlying changes help establish, but removal is independently valuable and
testable on its own.

**Independent Test**: With one or more subscriptions in the list, trigger a
remove action on one entry and confirm only that entry disappears from the
list while others remain.

**Acceptance Scenarios**:

1. **Given** a list with multiple subscriptions, **When** the user removes
   one specific subscription, **Then** that subscription no longer appears in
   the list and all other subscriptions remain unchanged.
2. **Given** a list with only one subscription, **When** the user removes it,
   **Then** the list becomes empty with no errors.
3. **Given** the same feed URL was added twice (as two separate entries),
   **When** the user removes one of the two entries, **Then** exactly one
   entry is removed and the other remains.

---

### Edge Cases

- What happens when the feed host is slow or unresponsive during validation?
  The system MUST apply a reasonable timeout and treat a timeout the same as
  a failed validation (reject with an error message).
- What happens when the feed URL redirects to another URL? The system
  follows standard HTTP redirects and validates the final response.
- What happens when a user tries to remove a subscription that was already
  removed (e.g., a stale UI state from a second tab)? The removal request is
  treated as a no-op if the entry no longer exists; no error is shown to the
  user for this case.
- What happens to a subscription that passed validation at add-time but whose
  feed later becomes unreachable? Out of scope for this feature — there is no
  periodic re-validation; the user can manually remove it if desired.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST validate that a submitted subscription value is
  a well-formed URL before attempting to add it.
- **FR-002**: The system MUST reject a submission that is not a well-formed
  URL and MUST display an error message explaining why.
- **FR-003**: The system MUST attempt to fetch the submitted URL and confirm
  the response can be parsed as an RSS or Atom feed (i.e., feed metadata such
  as a title is extractable) before accepting it as a subscription.
- **FR-004**: The system MUST reject a submission whose URL is well-formed
  but does not yield a parseable feed (unreachable, wrong content type,
  malformed feed XML, or timeout), and MUST display an error message
  explaining that the feed could not be verified.
- **FR-005**: The system MUST NOT add a subscription to the list unless both
  format validation (FR-001) and feed verification (FR-003) succeed.
- **FR-006**: The system MUST give the user visible feedback while a feed is
  being validated (e.g., a loading indicator), since verification involves a
  network call and is not instantaneous.
- **FR-007**: Each subscription MUST have a stable, unique identifier so it
  can be individually removed, even when the same URL is subscribed to more
  than once.
- **FR-008**: Users MUST be able to remove any individual subscription from
  the list.
- **FR-009**: The system MUST update the displayed list immediately after a
  subscription is removed, without requiring a page reload.
- **FR-010**: Removing a subscription that no longer exists (already removed)
  MUST NOT produce an error shown to the user.

### Key Entities

- **Subscription**: Represents a single feed the user has added. Adds a
  unique identifier (FR-007) to the existing `Url` attribute (from the MVP)
  so individual entries can be targeted for removal, including when
  duplicate URLs exist as separate entries.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of malformed URL submissions are rejected with a
  clear error message and are never added to the list.
- **SC-002**: 100% of well-formed URLs that do not resolve to a valid feed
  are rejected with a clear error message and are never added to the list.
- **SC-003**: A user can successfully add a known-good feed URL and see it
  appear in the list within 10 seconds (accounting for the network
  verification call).
- **SC-004**: A user can remove any single subscription and see the list
  update within 2 seconds, with all other subscriptions unaffected.
- **SC-005**: A new user can distinguish a rejected (invalid) submission from
  a successful one without needing external instructions, based solely on
  on-screen feedback.

## Assumptions

- "Feed metadata" means the response can be parsed as a standard RSS or Atom
  document well enough to extract at least a feed title; deep validation of
  every optional field is not required.
- A single validation attempt at add-time is sufficient; there is no
  requirement to periodically re-check previously added subscriptions.
- A reasonable fixed timeout (e.g., a few seconds) for the feed-fetch check
  is acceptable and does not need to be user-configurable.
- Subscriptions continue to be stored in memory only (per the original MVP
  scope) — this feature does not introduce persistence.
- "Invalid feed subscription" in the user's request is addressed by: (a)
  preventing invalid ones from being added at all (User Story 1), and (b)
  allowing removal of any subscription so users can clean up entries they no
  longer want (User Story 2), since once validation exists at add-time there
  should be few if any invalid entries to specifically target for removal.
