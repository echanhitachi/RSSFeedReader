# Quickstart: MVP RSS Subscription Management

## Prerequisites

- .NET 8 SDK installed
- Both `backend/RSSFeedReader.Api` and `frontend/RSSFeedReader.UI` projects
  scaffolded per [plan.md](./plan.md) project structure
- Template demo pages removed from the frontend (see TechStack.md cleanup
  checklist) and routing verified with no ambiguous routes

## Setup

1. Confirm backend and frontend ports are coordinated (see TechStack.md
   "Port configuration"):
   - Backend: `http://localhost:5151`
   - Frontend: `http://localhost:5213`
   - Frontend `wwwroot/appsettings.json` → `ApiBaseUrl` points to the backend
   - Backend CORS policy allows the frontend origin

2. Start the backend:

   ```powershell
   dotnet run --project backend/RSSFeedReader.Api
   ```

3. Start the frontend:

   ```powershell
   dotnet run --project frontend/RSSFeedReader.UI
   ```

4. Open the frontend URL in a browser.

## Validation scenarios

Run these to confirm the feature works end-to-end (maps to spec.md
Acceptance Scenarios and Success Criteria):

1. **Empty state** — Load the app with no subscriptions added yet.
   - **Expected**: The subscription list area renders empty, no console
     errors in browser DevTools (SC-004, User Story 2 Scenario 1).

2. **Add a subscription** — Enter
   `https://devblogs.microsoft.com/dotnet/feed/` in the input field and
   submit.
   - **Expected**: The URL appears in the displayed list immediately, without
     a page reload (User Story 1 Scenario 1, FR-003, SC-001).

3. **Add a second subscription** — Enter a different URL and submit.
   - **Expected**: Both URLs are visible in the list; the first entry is
     unaffected (User Story 1 Scenario 2).

4. **Reject blank input** — Submit the form with an empty input field.
   - **Expected**: No new entry appears in the list (User Story 1
     Scenario 3, FR-008).

5. **Duplicate URL allowed** — Submit the same URL used in step 2 again.
   - **Expected**: The URL now appears twice in the list (FR-007, Edge
     Cases).

6. **Volume check** — Add at least 20 subscriptions in sequence.
   - **Expected**: All 20+ entries remain visible in the list (SC-003).

## Automated tests

Run the backend unit tests covering the subscription service:

```powershell
dotnet test backend.Tests/RSSFeedReader.Api.Tests
```

**Expected**: All tests pass, covering add/list behavior and blank-input
rejection (see [contracts/subscriptions-api.md](./contracts/subscriptions-api.md)
and [data-model.md](./data-model.md) for the behavior under test).
