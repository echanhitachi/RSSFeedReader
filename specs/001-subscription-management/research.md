# Research: MVP RSS Subscription Management

All Technical Context fields were resolvable from the stakeholder documents
(`ProjectGoals.md`, `TechStack.md`) — no `NEEDS CLARIFICATION` markers
remained after drafting the plan. This document records the key decisions
and the alternatives considered.

## Decision: Backend framework — ASP.NET Core Minimal API

- **Decision**: Use ASP.NET Core Minimal API endpoints (not MVC controllers)
  for the two operations needed (add subscription, list subscriptions).
- **Rationale**: TechStack.md specifies ASP.NET Core Web API. For two simple
  endpoints, Minimal APIs keep the code small and readable (constitution
  Principle V: Simplicity), avoiding controller boilerplate unwarranted at
  this scope.
- **Alternatives considered**: MVC Controllers — rejected as unnecessary
  ceremony for two endpoints at MVP scope; can be introduced later if the API
  surface grows.

## Decision: In-memory storage via a singleton service

- **Decision**: A single `InMemorySubscriptionService` registered as a
  singleton, backed by a thread-safe collection (e.g., `ConcurrentBag<string>`
  or a `List<string>` guarded by a lock), holds subscriptions for the life of
  the process.
- **Rationale**: FR-006 requires only in-memory persistence for MVP.
  Singleton lifetime ensures the list survives across HTTP requests within a
  running process. Thread-safety is needed because ASP.NET Core can process
  requests concurrently even for a single local user (e.g., overlapping
  browser tabs/requests).
- **Alternatives considered**: Static field — rejected as harder to test and
  less idiomatic than DI-registered singleton; EF Core in-memory provider —
  rejected as unnecessary complexity for a plain string list at this phase.

## Decision: Frontend — single Blazor page for add + list

- **Decision**: One routed page (`Subscriptions.razor` at `/`) contains both
  the add-subscription form and the subscription list display.
- **Rationale**: Both User Story 1 and User Story 2 are visually and
  functionally coupled in the MVP (per AppFeatures.md: "the subscription list
  updates immediately when a subscription is added"). Splitting into
  multiple pages would add navigation complexity with no MVP benefit.
- **Alternatives considered**: Separate `/add` and `/subscriptions` pages —
  rejected as unnecessary indirection for a single-screen MVP.

## Decision: No feed URL validation or duplicate detection

- **Decision**: Accept any non-empty string as a subscription; reject only
  empty/blank submissions (FR-004, FR-007, FR-008).
- **Rationale**: Explicitly deferred per AppFeatures.md ("No validation of
  feed URLs") and the feature spec's Assumptions section. Only client- and
  server-side "non-empty" checks are needed to satisfy FR-008.
- **Alternatives considered**: Regex/URL-format validation — rejected as
  out-of-scope scope creep for this phase (would be introduced alongside
  feed fetching in Extended-MVP).

## Decision: CORS configuration

- **Decision**: Backend `Program.cs` configures a named CORS policy that
  allows only the exact frontend origin(s) from `launchSettings.json`
  (e.g., `http://localhost:5213`, `https://localhost:7025`).
- **Rationale**: Constitution Principle I (Security by Design) requires
  least-privilege CORS — explicit origins, not wildcards.
- **Alternatives considered**: `AllowAnyOrigin()` — rejected; violates the
  constitution's CORS requirement even for a local demo.

## Decision: Testing approach

- **Decision**: xUnit tests cover `InMemorySubscriptionService` directly
  (add returns the new subscription, list reflects additions in order,
  adding blank input is rejected). No UI automation tests for MVP.
- **Rationale**: Constitution Principle IV requires tests for non-trivial
  logic; the service has enough behavior (concurrency-safe add/list,
  blank-input rejection) to warrant unit tests, while full browser UI
  automation is disproportionate for a single static page at this phase.
- **Alternatives considered**: bUnit component tests — deferred; can be
  added if the UI grows more complex in a later phase.

**Output**: All unknowns resolved. Ready for Phase 1 design.
