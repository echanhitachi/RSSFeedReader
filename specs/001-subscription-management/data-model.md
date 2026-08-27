# Data Model: MVP RSS Subscription Management

## Entity: Subscription

Represents a single feed a user has added to their subscription list.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Url` | `string` | Yes | The raw text the user entered. Not validated as a well-formed URL or reachable feed (FR-004). Must be non-empty/non-blank to be accepted (FR-008). |

**Relationships**: None. Subscriptions are independent entries in this MVP —
no user, folder, or feed-item entities exist yet.

**Validation rules**:

- Reject (do not add) a subscription when the submitted `Url` is empty or
  consists only of whitespace (FR-008).
- No format/reachability validation is performed (FR-004).
- Duplicate `Url` values are permitted; each submission creates a new,
  independent entry (FR-007).

**State transitions**: None. A subscription has no lifecycle beyond
"added" — no status field, no removal, no read/unread state in this MVP
(all deferred to later phases per Assumptions in spec.md).

**Persistence**: Held only in an in-process in-memory collection for the
lifetime of the running backend process (FR-006). No database schema.
