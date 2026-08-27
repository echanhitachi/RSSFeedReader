<!--
Sync Impact Report
- Version change: [TEMPLATE] → 1.0.0 (initial ratification)
- Modified principles: N/A (first adoption; all principles newly defined)
- Added sections:
  - Core Principles: I. Security by Design, II. Code Quality & Maintainability,
    III. Incremental MVP-First Delivery, IV. Test-Backed Changes, V. Simplicity & Consistency
  - Technology & Architecture Constraints
  - Development Workflow & Quality Gates
  - Governance
- Removed sections: none (template placeholders replaced)
- Deferred TODOs: none
-->

# RSSFeedReader Constitution

## Core Principles

### I. Security by Design
Security MUST be treated as a first-class, non-negotiable requirement, not an
afterthought added post-implementation. All code MUST:

- Validate and sanitize all external input (feed URLs, user-supplied data) at
  system boundaries before use, even when current MVP scope defers strict
  validation logic — the validation *seams* MUST exist so rules can be added
  without redesign.
- Never trust content fetched from external feeds; any future HTML/content
  rendering MUST sanitize output before display to prevent injection attacks.
- Keep secrets, connection strings, and API keys out of source control; use
  configuration providers (e.g., `appsettings.json` overrides, environment
  variables, user secrets) instead of hardcoded values.
- Apply the principle of least privilege to CORS policies — allow only the
  specific known frontend origin(s), never wildcard origins in combination
  with credentials.
- Use HTTPS endpoints for all inter-service communication where the runtime
  environment supports it.

**Rationale**: This is a learning/demo project, but insecure patterns learned
here (open CORS, unsanitized rendering, hardcoded secrets) are exactly the
habits that cause real breaches later. Building security in from the MVP
avoids costly rework and reinforces correct habits.

### II. Code Quality & Maintainability
Code MUST be written for the next developer, not just to pass a demo. This
means:

- Follow standard .NET/C# naming conventions and idiomatic ASP.NET Core /
  Blazor patterns; no cleverness that sacrifices readability.
- Keep methods and components small and single-purpose; separate concerns
  between API controllers/endpoints, services, and UI components.
- Avoid duplication of logic between frontend and backend — shared models or
  contracts MUST live in a shared location when both sides need them.
- Every public method or component that isn't self-explanatory from its name
  and signature MUST have a concise comment explaining *why*, not *what*.
- No dead code, commented-out blocks, or leftover template boilerplate (e.g.,
  default Blazor demo pages) MUST remain in the codebase past the phase in
  which it is identified as obsolete.

**Rationale**: The stated project goal is an architecture that supports
future production-ready enhancements without a rewrite. That is only possible
if the MVP code stays clean and consistent as complexity is layered on.

### III. Incremental MVP-First Delivery
Features MUST be delivered in the smallest functional increment that
satisfies the current phase's definition of done before any later-phase
feature is started:

- Build MVP scope (subscription add + list) completely before touching
  Extended-MVP scope (feed fetching, item display).
- Do not introduce persistence, background polling, or advanced features
  until the phase that calls for them is reached, even if "easy to add now."
- Each phase MUST have a clear, testable definition of "done" (as described
  in the project's stakeholder documents) before moving to the next phase.

**Rationale**: The project goals explicitly define phased scope (MVP →
Extended-MVP → post-MVP). Scope creep within a phase undermines the rapid,
demonstrable delivery this project is designed to prove out.

### IV. Test-Backed Changes
Behavior that can be verified automatically MUST be verified automatically:

- New backend logic beyond trivial pass-through (e.g., feed parsing, error
  handling, future persistence logic) MUST include unit or integration tests
  before being considered complete.
- Manual verification steps (e.g., browser checks, port/CORS configuration
  checks) called out in project documentation MUST be performed and confirmed
  before a phase is marked complete, even when automated tests are not yet
  in place for that phase.
- Bug fixes MUST include a test that reproduces the bug where feasible, to
  prevent regression.

**Rationale**: The MVP phases intentionally start without heavy test
infrastructure, but as feed fetching, parsing, and persistence are added, the
project takes on real failure modes (malformed XML, network errors) that
must be guarded against with tests, not assumptions.

### V. Simplicity & Consistency
Prefer the simplest solution that satisfies the current phase's requirements:

- No speculative abstractions, frameworks, or libraries beyond what the
  current phase needs (YAGNI). Justify any added dependency against the
  phase's stated requirements.
- Keep configuration (ports, API base URL, CORS origins) consistent and
  centrally defined across backend and frontend as documented in the tech
  stack, rather than hardcoded in multiple places.
- Architectural changes that deviate from the ASP.NET Core Web API + Blazor
  WebAssembly structure MUST be justified in writing (e.g., in a plan.md)
  before implementation.

**Rationale**: Simplicity keeps the demonstration fast to build and easy to
reason about, while consistency prevents configuration drift (e.g.,
mismatched ports) that has already been identified as a common failure mode
in this project.

## Technology & Architecture Constraints

- Backend: ASP.NET Core Web API. Frontend: Blazor WebAssembly. This stack
  MUST NOT be changed without a constitution amendment.
- Frontend MUST read backend connection details (e.g., API base URL) from
  configuration, never hardcoded in source.
- Backend CORS policy MUST explicitly list allowed frontend origins matching
  the frontend's actual configured ports.
- Storage starts in-memory for MVP; any move to persistent storage (e.g., EF
  Core + SQLite) MUST preserve the existing service/API contracts so the UI
  layer requires minimal changes.
- Template-generated demonstration code (default Blazor sample pages/routes)
  MUST be removed before feature implementation begins, and routing MUST be
  verified to have no ambiguous routes prior to starting UI feature work.

## Development Workflow & Quality Gates

- Every change MUST be evaluated against the current phase's scope (MVP,
  Extended-MVP, or post-MVP) before implementation; out-of-phase work MUST be
  deferred and tracked, not silently included.
- Before a phase is marked complete, the relevant local development checklist
  from the project's stakeholder documentation (backend/frontend run
  correctly, correct ports, CORS configured, no console errors) MUST be
  verified.
- Code review (self-review when working solo) MUST confirm: no leftover
  template code, no hardcoded secrets/URLs, and tests exist for any new
  non-trivial logic.
- Complexity or dependencies added beyond the current phase's stated needs
  MUST be called out and justified before merging.

## Governance

This constitution supersedes ad hoc practices for this project. All plans,
specs, and task breakdowns MUST be checked for compliance with these
principles before implementation begins.

- **Amendments**: Any change to this constitution MUST be proposed with a
  clear rationale, update the affected principles/sections, and bump the
  version according to semantic versioning rules below.
- **Versioning policy**:
  - MAJOR: Backward-incompatible removal or redefinition of a principle.
  - MINOR: A new principle or materially expanded section is added.
  - PATCH: Wording clarifications or non-semantic refinements.
- **Compliance review**: Any plan or task list generated for this project
  MUST include an explicit check against these principles; unresolved
  violations MUST be justified in the plan's complexity-tracking section or
  resolved before implementation proceeds.

**Version**: 1.0.0 | **Ratified**: 2026-08-27 | **Last Amended**: 2026-08-27
