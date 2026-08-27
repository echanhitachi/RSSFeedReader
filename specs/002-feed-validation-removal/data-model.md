# Data Model: Feed URL Validation & Subscription Removal

## Entity: Subscription (modified from 001-subscription-management)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` | Yes (new) | Stable unique identifier, generated server-side when a subscription is successfully added. Enables removal of a specific entry even when duplicate URLs exist. |
| `Url` | `string` | Yes | The feed URL, now guaranteed to be well-formed and to have returned a parseable feed at the time it was added (FR-001, FR-003). Not re-validated after that. |

**Relationships**: None — still a flat, independent list of entries.

**Validation rules** (new/changed from 001):

- A submission MUST be a well-formed absolute HTTP/HTTPS URL to proceed past
  format validation (FR-001, FR-002).
- A submission that passes format validation MUST also successfully fetch and
  parse as an RSS/Atom feed (at least a feed title extractable) to be added
  (FR-003, FR-004, FR-005).
- Duplicate URLs remain allowed as separate entries, each with its own `Id`
  (unchanged from 001's FR-007).

**State transitions**: A subscription now has two possible terminal states at
submission time — "added" (format + feed validation both passed) or
"rejected" (never stored, per FR-005). Once added, a subscription can
transition to "removed" (FR-008), at which point it no longer appears in
`GetAll()` results. There is no other state (no re-validation, no
edit-in-place).

**Persistence**: Unchanged — in-memory only, for the lifetime of the running
backend process.

## New concept: Feed validation result (not persisted)

Represents the outcome of checking a candidate URL before it becomes a
`Subscription`. Not stored as an entity — used only within the request that
handles `POST /api/subscriptions`.

| Field | Type | Notes |
|-------|------|-------|
| `IsValid` | `bool` | True only if both format and feed checks pass. |
| `ErrorMessage` | `string?` | Present when `IsValid` is false; distinguishes "invalid URL format" from "feed could not be verified" for the error response (FR-002, FR-004). |
