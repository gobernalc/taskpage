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
├── .gitignore
├── README.md
├── README.es.md                # Spanish mirror of this document
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
└── frontend/                   # Angular application
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

Open Swagger at: <http://localhost:5000/swagger> (or the port shown in the console).

### 4.2 Frontend

```bash
cd frontend
ng serve
```

Open the application at <http://localhost:4200>.

---

## 5. Tests

### 5.1 Backend tests

```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

Each test prints **✅ when it passes** and **❌ when it fails** to the console, with
a description and a readable flow of each test result.

### 5.2 Frontend tests

```bash
cd frontend
ng test --watch=false
```

Each spec prints **✅ when it passes** and **❌ when it fails** to the console
through a custom reporter (`vitest.emoji-reporter.ts`).

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

## 8. Notes

- The database schema lives in a **single idempotent script**
  (`database/schema.sql`), which can run N times without side effects.
- The connection string is configurable so any evaluator can set up the database
  on their own SQL Server instance.
- The frontend test runner is **Vitest 4.1.12** (configured in `angular.json` with
  the custom reporter `vitest.emoji-reporter.ts`, which prints ✅/❌ per spec).
- Project status: the environment and project structure are set up, and
  **Phase 1 (database schema)** and **Phase 2 (domain and application layer)** are
  implemented. Backend and frontend layers will keep growing phase by phase
  according to [`plans/design.md`](plans/design.md:1).
