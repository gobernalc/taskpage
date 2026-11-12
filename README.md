# TaskPage

> For the Spanish version of this document, see [README.es.md](README.es.md).

Technical test: **REST API in .NET (Web API) + Angular + SQL Server**.

Full-stack solution for an internal task management system, with a backend in
**.NET 10 (ASP.NET Core Web API)** organized with Clean Architecture, a frontend in
**Angular 22** (standalone components + SCSS) and a **SQL Server** database. The API
is exposed and documented in **Swagger**.

---

## 1. Prerequisites

| Tool               | Verified version                                                            |
| ------------------ | --------------------------------------------------------------------------- |
| Node.js            | 22.23.4 (LTS)                                                               |
| npm                | 10.9.9                                                                      |
| .NET SDK           | 10.0.101 (.NET 10 LTS)                                                      |
| Angular CLI        | 22.2.6                                                                      |
| SQL Server         | 2022 or later (verified with 2025 Developer)                                |
| sqlcmd             | SQL Server command-line utility                                             |
| Visual Studio Code | with C# Dev Kit, Angular Language Service and SQL Server (mssql) extensions |

---

## 2. Repository structure

```text
taskpage/
├── .github/
│   └── workflows/
│       └── ci.yml              # backend and frontend continuous integration
├── database/
│   └── schema.sql              # single idempotent SQL script
├── backend/                    # .NET solution (Clean Architecture)
│   └── src/
│       ├── TaskPage.Domain/
│       ├── TaskPage.Application/
│       ├── TaskPage.Infrastructure/
│       ├── TaskPage.Api/
│       ├── TaskPage.Application.Tests/
│       └── TaskPage.Api.Tests/
├── frontend/                   # Angular application
│   ├── public/
│   │   └── logo.svg            # brand mark used by the icon and the sidebar
│   └── src/
│       ├── assets/i18n/        # es.json (default) and en.json
│       └── app/
│           ├── core/           # models, HTTP, i18n and error handling
│           ├── features/       # users and tasks (lazy loaded)
│           └── shared/         # reusable components and the confirm dialog
├── README.md
└── README.es.md                # Spanish mirror of this document
```

---

## 3. Step-by-step installation and configuration

### 3.1 Clone the repository and install backend dependencies

```bash
git clone https://github.com/gobernalc/taskpage.git taskpage
cd taskpage/backend
dotnet restore
```

### 3.2 Install frontend dependencies

```bash
cd taskpage/frontend
npm install --legacy-peer-deps
```

### 3.3 Create the database

Run the idempotent script on a local SQL Server instance:

```bash
sqlcmd -S localhost -E -i database\schema.sql
```

- `-S localhost`: default instance. If your instance is named, use
  `localhost\INSTANCE_NAME`.
- `-E`: Windows authentication (current account as `sysadmin`).
- **`-f 65001` (UTF-8):** `schema.sql` is stored as UTF-8. The classic ODBC
  `sqlcmd` reads files with the system ANSI code page by default, so add
  `-f 65001` (`sqlcmd -S localhost -E -f 65001 -i database\schema.sql`) or run
  the script from the VS Code mssql extension. The modern Go-based `sqlcmd`
  (v1) reads UTF-8 by default.

The script creates the `TaskPageDb` database (if it does not exist), the `Users`
and `Tasks` tables with their primary/foreign keys, `CHECK`, `DEFAULT` and audit
columns (`CreatedAt`/`UpdatedAt`), the indexes for search and sorting, and the seed
data (2 users and 3 tasks with `AdditionalData` in JSON). It can run **N times
without side effects**: tables, constraints, indexes and seed data are only
created/inserted the first time.

- **`N'...'` (Unicode prefix):** in SQL Server the `N` prefix before a string
  denotes an `NVARCHAR` (Unicode) literal. It is used on text columns (`Name`,
  `Email`, `Title`, `Status`, `AdditionalData`) so the literals are interpreted as
  Unicode and no characters are lost (accents, `ñ`, etc.).
- **Optional seed data:** the seed (2 users and 3 tasks) is only demo content. If
  it is not wanted, the "Seed data" section of the script can be commented out or
  deleted without affecting the schema.

Quick verification after running it:

```bash
sqlcmd -S localhost -E -d TaskPageDb -Q "SELECT COUNT(*) AS Users FROM dbo.Users; SELECT COUNT(*) AS Tasks FROM dbo.Tasks;"
```

Sample JSON query (`ISJSON`, `JSON_VALUE`, `JSON_QUERY`, `OPENJSON` and
`JSON_MODIFY`) on the `Tasks.AdditionalData` column:

```sql
-- Validate that the content is valid JSON
SELECT Id, ISJSON(AdditionalData) AS IsJson FROM dbo.Tasks;

-- Read a field from the JSON (priority)
SELECT Id, JSON_VALUE(AdditionalData, '$.priority') AS Priority FROM dbo.Tasks;

-- Filter by a value inside the JSON (high priority)
SELECT Id, Title FROM dbo.Tasks
WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High';

-- Extract a complete JSON object/array (tags)
SELECT Id, JSON_QUERY(AdditionalData, '$.tags') AS Tags FROM dbo.Tasks;

-- Open the JSON array as rows (one row per tag)
SELECT t.Id, tag.[value] AS Tag
FROM dbo.Tasks t
CROSS APPLY OPENJSON(t.AdditionalData, '$.tags') AS tag;

-- (Optional) Update a field in the JSON
UPDATE dbo.Tasks
SET AdditionalData = JSON_MODIFY(AdditionalData, '$.estimatedCompletion', '2026-12-31')
WHERE Id = 1;
```

### 3.4 Configure the connection string

In `backend/src/TaskPage.Api/appsettings.Development.json`:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=TaskPageDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
}
```

It can also be configured with the `ConnectionStrings__DefaultConnection`
environment variable without touching the file.

---

## 4. Running the application

### 4.1 Backend (API + Swagger)

```bash
cd backend/src/TaskPage.Api
dotnet run
```

Open Swagger at: <http://localhost:5017/swagger> (or the port shown in the console).

### 4.2 Frontend

```bash
cd frontend
npm start
```

Open the application at <http://localhost:4200>.

Both servers run at the same time, each one in its own terminal: the frontend calls
the API at `http://localhost:5017/api`. In development the API only accepts the
Angular dev server origin through CORS, configured in `Cors:AllowedOrigins` inside
`appsettings.Development.json`. HTTPS redirection is enabled outside development so
the local plain-HTTP port keeps working.

---

## 5. Tests

### 5.1 Backend tests

```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

Each test prints **✅ when it passes** and **❌ when it fails** to the console, with
a description and a readable flow of each test result.

The backend suite covers:

- `UserService`: valid creation, duplicate email, email normalization (trim and
  lowercase), update, deletion with and without associated tasks, lookup by id,
  search, sorting and pagination.
- `TaskService`: creation, nonexistent user, valid and invalid JSON, update, deletion,
  status transitions (rejecting the forbidden `Pending -> Done` and allowing the rest),
  idempotent status changes, filters, search, sorting and pagination.
- Validators: required fields, maximum lengths, email format, invalid status and
  invalid JSON documents.
- Controllers: `200`/`201`/`204`/`400`/`404`/`409` status codes, response shapes,
  `Location` headers and input validation.
- `GlobalExceptionMiddleware`: `ProblemDetails` mapping, `X-Request-Id` header and
  `traceId` correlation.
- `HealthController`: `200` with the database available and `503` when it is unavailable.

### 5.2 Frontend tests

```bash
cd frontend
ng test --watch=false
```

Each spec prints **✅ when it passes** and **❌ when it fails** to the console
through a custom reporter (`vitest.emoji-reporter.ts`).

The frontend suite covers:

- `UsersService`/`TasksService`: CRUD requests, query params, paged response
  mapping and network error responses.
- `TranslateService`/`TranslatePipe`: loading `es.json`/`en.json`, language
  change and persistence, missing keys, interpolation, accent preservation and
  keeping the previous dictionary when a request fails.
- `TaskList`: rendering, detail links, status filter, debounced title search,
  pagination, empty state, deletion with confirmation and load errors.
- `TaskDetail`: rendering, metadata, additional JSON data, invalid ids, deletion
  confirmation, cancellation and load errors.
- `TaskCreate`/`TaskEdit`: user loading, reactive validation, submit payloads,
  JSON validation and API errors.
- `TaskStatus`: the forbidden `Pending -> Done` transition, the allowed
  transitions, API error handling and selector value restoration.
- `UserList`/`UserForm`: rendering, search, deletion with confirmation and
  reactive validation, creation and edition flows.
- `ConfirmDialog`: accessible alert dialog content, confirm and cancel outputs,
  Escape and backdrop dismissal, busy state and initial focus.
- `Pagination` and `Badge`: boundary navigation and status tone hooks.
- Application shell: sidebar rendering, theme toggle persistence, language
  switching with flags, ignored active language, accessible labels and recovery
  from a failed dictionary request.

### 5.3 Continuous integration

The GitHub Actions workflow (`.github/workflows/ci.yml`) runs on every push to
`main` and on every pull request targeting `main`. It has two independent jobs:

- `backend`: `dotnet restore`, `dotnet build --no-restore` and
  `dotnet test --no-build --logger "console;verbosity=detailed"` on .NET 10.
- `frontend`: `npm ci`, `npm run build` and `ng test --watch=false` on Node 22.

Both jobs are required status checks for the merge into `main`. NuGet and npm
caches keep the pipeline in the minutes range, and the ✅/❌ output is preserved.

---

## 6. Technical decisions

### 6.1 Architecture

The backend follows Clean Architecture, with one-way dependencies from the outer
layers toward the center:

- `TaskPage.Domain`: entities, enums and domain exceptions (no dependencies).
- `TaskPage.Application`: use cases, DTOs, validation, filters and repository
  interfaces.
- `TaskPage.Infrastructure`: persistence with EF Core and repository
  implementations.
- `TaskPage.Api`: controllers, middleware and the HTTP pipeline.

### 6.2 Implemented business rules

The domain and application layers own the business rules:

- The task title is required (1-200 characters).
- Every task must have an assigned user that already exists.
- Changing a task directly from Pending to Done is rejected: the task must pass
  through InProgress first.
- A user with associated tasks cannot be deleted (409 Conflict).
- The email is normalized (trimmed and lowercased) and must be unique, compared
  case-insensitively.

### 6.3 Persistence (EF Core)

The infrastructure layer persists data with **EF Core 10** and the SQL Server
provider. The [`AppDbContext`](backend/src/TaskPage.Infrastructure/Persistence/AppDbContext.cs:1)
maps the `User` and `TaskItem` entities to the `Users` and `Tasks` tables through
[`UserConfiguration`](backend/src/TaskPage.Infrastructure/Persistence/Configurations/UserConfiguration.cs:1)
and [`TaskConfiguration`](backend/src/TaskPage.Infrastructure/Persistence/Configurations/TaskConfiguration.cs:1):
keys, required columns, maximum lengths, the `TaskState` string conversion,
relationships and indexes stay aligned with [`database/schema.sql`](database/schema.sql:1).

- [`UserRepository`](backend/src/TaskPage.Infrastructure/Repositories/UserRepository.cs:1)
  and [`TaskRepository`](backend/src/TaskPage.Infrastructure/Repositories/TaskRepository.cs:1)
  implement the application interfaces `IUserRepository` and `ITaskRepository`.
- The task listing supports **filtering** (status and assigned user), **search**
  (title), **sorting** (title, status and creation date) and **pagination**.
- The priority filter reads the JSON column with the native function
  **`JSON_VALUE(AdditionalData, '$.priority')`**, translated by EF Core through a
  mapped database function so the query stays parameterized.
- [`DependencyInjection.cs`](backend/src/TaskPage.Infrastructure/DependencyInjection.cs:1)
  exposes `AddInfrastructure`, which registers `AppDbContext` with the
  `DefaultConnection` connection string and the repositories as scoped services.
- The physical schema remains owned by [`database/schema.sql`](database/schema.sql:1)
  (single idempotent script): EF Core is used for queries and commands, not to
  generate the schema.

### 6.4 Validation (FluentValidation)

Input DTOs are validated with **FluentValidation** before reaching the services:
required fields, maximum lengths, email format, positive identifiers, enum values,
valid status transitions and valid JSON documents. Validation is centralized, the
API returns readable messages in English and entities are never exposed.

### 6.5 API documentation (Swagger)

The REST API is exposed and documented in **Swagger** (UI at `/swagger`, JSON at
`/swagger/v1/swagger.json`). Every endpoint is testable from the Swagger UI and
documents its request models, query parameters and response codes.

| Method   | Route                    | Description                                                             |
| -------- | ------------------------ | ----------------------------------------------------------------------- |
| `POST`   | `/api/users`             | Creates a user (201 + `Location`, 400 validation, 409 duplicate email). |
| `GET`    | `/api/users`             | Lists users with search, sorting and pagination (200).                  |
| `GET`    | `/api/users/{id}`        | Gets a user by id (200, 404).                                           |
| `PUT`    | `/api/users/{id}`        | Replaces the name and email of a user (200, 400, 404, 409).             |
| `DELETE` | `/api/users/{id}`        | Deletes a user with no associated tasks (204, 404, 409).                |
| `POST`   | `/api/tasks`             | Creates a task assigned to a user (201 + `Location`, 400, 404).         |
| `GET`    | `/api/tasks`             | Lists tasks with filters, search, sorting and pagination (200).         |
| `GET`    | `/api/tasks/{id}`        | Gets a task by id (200, 404).                                           |
| `PUT`    | `/api/tasks/{id}`        | Replaces the editable values of a task (200, 400, 404).                 |
| `DELETE` | `/api/tasks/{id}`        | Deletes a task (204, 404).                                              |
| `PUT`    | `/api/tasks/{id}/status` | Changes a task status respecting the transition rule (200, 400, 404).   |
| `GET`    | `/api/health`            | Reports the service and database health (200, 503).                     |

### 6.6 Logging and observability

- A global [`GlobalExceptionMiddleware`](backend/src/TaskPage.Api/Middleware/GlobalExceptionMiddleware.cs:1)
  maps domain and validation failures to `ProblemDetails` (400/404/409/500)
  without exposing internal details.
- Error responses include the `X-Request-Id` header and the correlation id in
  `ProblemDetails.Extensions` under the `traceId` key.
- **Serilog** writes structured logs to the console and `logs/taskpage-.log`,
  correlating every event with the request `TraceIdentifier`. Emails are masked and
  `AdditionalData` is never fully dumped.

### 6.7 Security

- **Parameterized** queries through EF Core (no SQL concatenation).
- **Input validation** in DTOs with FluentValidation and length limits.
- **No internal details** exposed to the client: only `ProblemDetails`.
- **No sensitive data** in logs: masked emails and no `AdditionalData` payloads.
- **HTTPS** enabled outside development; `TrustServerCertificate=True` only in
  development.
- **Restricted CORS**: only the configured Angular dev origin in development.
- **Dependency pin** for the transitive `Microsoft.Data.SqlClient` package affected
  by known advisories.
- Response control with `[ProducesResponseType]` and consistent HTTP codes.

### 6.8 Testing strategy

- Backend: **xUnit** with service, validator, controller, middleware and health
  check tests.
- Frontend: **Vitest 4.1.12** with a custom reporter that prints ✅/❌ per spec.
- Golden rule: **fix the app, not the test**. Every change must keep both suites
  green before delivery.
- Scenarios covered: positive, negative, incomplete data, nulls, wrong values,
  special characters, limits, state transitions, idempotency and failure recovery.

### 6.9 Database

- The whole schema lives in **one idempotent script** (`database/schema.sql`) that
  can run N times without side effects.
- Primary/foreign keys, `CHECK` constraints, `DEFAULT` values, audit columns,
  clustered/nonclustered/unique indexes and idempotent seed data are included.
- JSON is validated with `ISJSON`, read and filtered with `JSON_VALUE`, extracted
  with `JSON_QUERY`, expanded with `OPENJSON` and updated with `JSON_MODIFY`.

### 6.10 Visual design

The frontend applies a custom navy/deep blue palette with a strong visual identity.

- Design tokens live in [`frontend/src/styles.scss`](frontend/src/styles.scss:1)
  as CSS variables: colors, 2-stop gradients, spacing, radii, shadows, motion and
  typography. Components consume the variables, so no color, spacing, radius or
  gradient is hardcoded.
- A switchable light/dark theme overrides the tokens through the
  `[data-theme="dark"]` selector, honors `prefers-color-scheme` by default and
  persists the user choice in `localStorage` (non-sensitive key).
- The application shell ([`app.html`](frontend/src/app/app.html:1),
  [`app.scss`](frontend/src/app/app.scss:1)) is a **collapsible sidebar** with a
  brand mark, icon navigation, theme and language controls, a main outlet and a
  footer. The collapsed state is persisted, and the layout adds gradients, shadows,
  subtle transitions and accessible focus states.
- The **confirmation dialog** ([`confirm-dialog.ts`](frontend/src/app/shared/components/confirm-dialog/confirm-dialog.ts:1))
  is a centered modal with an accent line, dimmed backdrop, focus trap, initial
  focus on the safe action, Escape and backdrop dismissal.

### 6.11 Internationalization

Every visible UI text is centralized in `frontend/src/assets/i18n/es.json`
(default language, Spanish) and `frontend/src/assets/i18n/en.json`, and referenced
by key from the templates — hardcoding texts is forbidden:

- A custom [`TranslateService`](frontend/src/app/core/i18n/translate.service.ts:1)
  loads the active dictionary, exposes it as a signal, persists the language in
  `localStorage` (non-sensitive key), keeps `document.documentElement.lang` in sync
  and preserves the previous dictionary when a request fails.
- A standalone [`TranslatePipe`](frontend/src/app/core/i18n/translate.pipe.ts:1)
  renders keys in templates with simple `{{name}}` interpolation.
- The language switcher uses accessible flag buttons for Spanish and English.
- States (`Pending`, `InProgress`, `Done`), buttons, titles, placeholders and
  accessibility texts live in the language files; the API keeps returning English
  messages and the frontend translates only its own interface.

### 6.12 Interface features

The functional frontend manages tasks and users end to end:

- **Task list** ([`task-list.ts`](frontend/src/app/features/tasks/task-list/task-list.ts:1))
  lists tasks with a status filter, debounced title search and pagination, shows
  the status badge and the assigned user, links each title to its detail view and
  allows deleting a task or changing its status inline (rejecting the forbidden
  `Pending -> Done` transition).
- **Task detail** ([`task-detail.ts`](frontend/src/app/features/tasks/task-detail/task-detail.ts:1))
  shows the full task, assigned user, localized audit timestamps and additional JSON
  data, with shortcuts to edit or delete it.
- **Task creation and edition** ([`task-create.ts`](frontend/src/app/features/tasks/task-create/task-create.ts:1),
  [`task-edit.ts`](frontend/src/app/features/tasks/task-edit/task-edit.ts:1)) are
  reactive forms with title, an assigned-user selector and optional JSON additional
  data, validated on the client before submission.
- **User list** ([`user-list.ts`](frontend/src/app/features/users/user-list/user-list.ts:1))
  lists users with search and pagination and allows deleting them.
- **User form** ([`user-form.ts`](frontend/src/app/features/users/user-form/user-form.ts:1))
  creates and edits users with name and email validation.
- **Destructive actions** always go through the accessible confirmation dialog and
  never through a native prompt.
- **Shared components** (`badge`, `button`, `card`, `select`, `pagination` and
  `confirm-dialog`) are reused by every feature instead of duplicating markup.
- **Error handling** reads the `ProblemDetails` payload returned by the API and
  shows a readable message without exposing internal details.

Routes: `/tasks` (list), `/tasks/new` (create), `/tasks/:id` (detail),
`/tasks/:id/edit` (edit), `/users` (list), `/users/new` (create) and
`/users/:id/edit` (edit).

### 6.13 Additional proposals implemented

The following features go beyond the original specification and are fully
implemented:

- **Expanded CRUD**: update and delete for users and tasks, plus task status changes.
- **Pagination, search and sorting** for both listings, with a paged result model.
- **Audit columns** (`CreatedAt`, `UpdatedAt`) in the database and in the API responses.
- **Email normalization and uniqueness**, compared case-insensitively.
- **JSON in SQL Server** for additional task data, with validation and native functions.
- **EF Core 10** persistence with typed mappings and parameterized queries.
- **FluentValidation** for centralized input validation with English messages.
- **Serilog** structured logging with `TraceIdentifier` correlation and PII masking.
- **`X-Request-Id` header** and `traceId` in `ProblemDetails` for request correlation.
- **Database health check** at `GET /api/health`.
- **Light/dark theme** with design tokens, system preference support and persistence.
- **Spanish/English internationalization** with flag buttons, persisted language and
  document language synchronization.
- **Collapsible sidebar** with icon rail, brand mark, persisted state and accessible
  controls.
- **Confirmation dialog** for destructive actions with focus management, Escape and
  backdrop dismissal.
- **Task detail page** with audit timestamps and additional JSON data.
- **Automated tests** with xUnit and Vitest, including the ✅/❌ emoji reporter.
- **Continuous integration** with GitHub Actions (backend and frontend jobs).
- **Development CORS configuration** driven by environment settings.
- **Security hardening**: restricted CORS, no internal error details, masked PII and a
  pinned patched dependency.
- **Accessibility**: semantic landmarks, labels, focus states, focus trap and status
  contrast hooks.
- **Responsive layout** with a mobile-friendly sidebar and token-based spacing.
- **Bilingual README** (`README.md` and `README.es.md`) kept in sync.

### 6.14 Known limitations and next steps

Each point is a future feature with its benefit; nothing here is partially enabled:

- **Authentication and authorization with JWT/OIDC** (Microsoft.Identity.Web + Entra
  ID, role policies and `[Authorize]` per controller): it controls who creates, edits
  and deletes, and leaves the system ready for auditing.
- **Optimistic concurrency with `rowversion`/ETag**: it prevents two users from
  overwriting the same task (lost update) and returns 409/412 when it happens.
- **Versioned migrations executed by the pipeline** (EF Core Migrations, DbUp or
  Flyway): repeatable and traceable deployments without manual steps.
- **Expanded observability**: `live`/`ready` health checks, a central log sink and
  distributed traces (OpenTelemetry/Application Insights).
- **API versioning with `Asp.Versioning`**: evolution without breaking clients.
- **End-to-end tests with Playwright and automated accessibility with `axe-core`**:
  they cover the full flow and status contrast.
- **Rate limiting and security headers** (HSTS, CSP and `X-Content-Type-Options`):
  they protect the exposed Swagger and API.
- **Separate environments with managed secrets** (Key Vault) and reproducible
  container deployment.
- **Public demo at a single URL**: the optional deployment phase was not executed;
  the app keeps running locally with `ng serve` + `dotnet run`, and publishing the
  Angular build from `wwwroot` remains the documented next step.

---

## 7. Branching and delivery

The repository follows **trunk-based development**:

- `main` is the only long-lived branch and it is always green and releasable.
  There is no `dev` or `qa` branch.
- Every change lives in a short-lived branch created from an up-to-date `main`:
  `feat/<slug>`, `fix/<slug>`, `chore/<slug>`, `docs/<slug>` or `test/<slug>`.
  A branch lives two days at most and keeps the diff around 400 lines.
- The branch reaches `main` through a pull request with one approving review from
  the tech lead, a green CI run, a squash merge and immediate deletion of the
  branch. `main` is protected: no direct pushes and no force pushes.
- A release is a semver tag (`vMAJOR.MINOR.PATCH`) created on `main`. Environments
  are deployment targets configured with environment variables, not branches.
- Hotfixes branch from the affected tag as `fix/<slug>`, reach `main` through a
  pull request and produce a patch tag.

Daily flow:

```bash
git switch main && git pull --ff-only
git switch -c feat/task-status-transition
# work, commit and push
git push -u origin feat/task-status-transition
# open the pull request against main, squash merge and delete the branch
git switch main && git pull --ff-only
```

---

## 8. Notes

- The database schema lives in a **single idempotent script**
  (`database/schema.sql`), which can run N times without side effects.
- The connection string is configurable so any evaluator can set up the database
  on their own SQL Server instance.
- The frontend test runner is **Vitest 4.1.12** (configured in `angular.json` with
  the custom reporter `vitest.emoji-reporter.ts`, which prints ✅/❌ per spec).
- Project status: **phases 1 to 8 are implemented** (database, domain and
  application layer, SQL Server persistence, REST API with Swagger, backend tests,
  frontend base, functional frontend and frontend tests). The final integration and
  documentation phase is complete, including the collaboration menu, theme,
  language, confirmation dialog and task detail additions. The optional demo
  deployment (phase 10) remains documented as a next step with its benefit.
