-- ==================================================================
-- Script  : database/schema.sql
-- Summary : Single, idempotent schema for the TaskPage database.
--           Creates the database, the Users and Tasks tables (with PK,
--           FK, CHECK, DEFAULT and audit columns), the indexes and the
--           optional seed data. It must run N times without side
--           effects and work on any SQL Server instance.
-- ==================================================================
-- ------------------------------------------------------------------
-- Section 1: database creation.
-- Creates the TaskPageDb database if it does not exist yet.
-- ------------------------------------------------------------------
IF DB_ID(N'TaskPageDb') IS NULL
    BEGIN
        CREATE DATABASE TaskPageDb;
    END


GO
USE TaskPageDb;


GO
-- ------------------------------------------------------------------
-- Section 2: tables.
-- Creates the Users and Tasks tables with their primary keys, foreign
-- keys, CHECK and DEFAULT constraints. Creation is conditional so the
-- script stays idempotent.
-- ------------------------------------------------------------------
-- Users table: system collaborators.
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Users (
            Id        INT            IDENTITY (1, 1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY, -- Clustered PK on Id
            Name      NVARCHAR (100) NOT NULL, -- Collaborator name
            Email     NVARCHAR (200) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE, -- Unique email
            CreatedAt DATETIME2 (0)  CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()) NOT NULL,
            UpdatedAt DATETIME2 (0)  NULL -- Last modification date
        );
    END


GO
-- Tasks table: tasks with additional information stored as JSON.
IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Tasks (
            Id             INT            IDENTITY (1, 1) NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY, -- Clustered PK on Id
            Title          NVARCHAR (200) NOT NULL, -- Required title
            Status         NVARCHAR (20)  CONSTRAINT DF_Tasks_Status DEFAULT (N'Pending') NOT NULL, -- Pending / InProgress / Done
            UserId         INT            NOT NULL, -- Assigned user
            CreatedAt      DATETIME2 (0)  CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()) NOT NULL,
            UpdatedAt      DATETIME2 (0)  NULL, -- Last modification date
            AdditionalData NVARCHAR (MAX) NULL, -- Additional JSON
            CONSTRAINT FK_Tasks_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
            CONSTRAINT CK_Tasks_Status CHECK (Status IN (N'Pending', N'InProgress', N'Done')),
            CONSTRAINT CK_Tasks_AdditionalData_IsJson CHECK (AdditionalData IS NULL
                                                             OR ISJSON(AdditionalData) = 1)
        );
    END


GO
-- ------------------------------------------------------------------
-- Section 3: indexes.
-- Creates the nonclustered indexes required for search, sorting and
-- filters. Conditional creation runs dynamically because CREATE INDEX
-- starts its own batch.
-- ------------------------------------------------------------------
-- Composite index that serves the query for tasks by user, status and
-- creation date in descending order.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Tasks_UserId_Status_CreatedAt'
                      AND object_id = OBJECT_ID(N'dbo.Tasks'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Tasks_UserId_Status_CreatedAt
                 ON dbo.Tasks (UserId, Status, CreatedAt DESC);');
    END


GO
-- Index for the status filter.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Tasks_Status'
                      AND object_id = OBJECT_ID(N'dbo.Tasks'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Tasks_Status
                 ON dbo.Tasks (Status);');
    END


GO
-- Index for full text search on the title.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Tasks_Title'
                      AND object_id = OBJECT_ID(N'dbo.Tasks'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Tasks_Title
                 ON dbo.Tasks (Title);');
    END


GO
-- Index for search and sorting by user name.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Users_Name'
                      AND object_id = OBJECT_ID(N'dbo.Users'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Users_Name
                 ON dbo.Users (Name);');
    END


GO
-- ------------------------------------------------------------------
-- Section 4: seed data (optional).
-- Inserts the sample users and tasks only once. This data is
-- OPTIONAL: if it is not wanted, comment out or delete this whole
-- section; the schema (tables, constraints and indexes) does not
-- depend on it. The conditions on Email and on Title + UserId ensure
-- that repeated executions do not duplicate data.
-- ------------------------------------------------------------------
-- User: Ana Pérez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Users
               WHERE  Email = N'ana.perez@example.com')
    BEGIN
        INSERT  INTO dbo.Users (
            Name,
            Email
        )
        VALUES                 (N'Ana Pérez', N'ana.perez@example.com');
    END


GO
-- User: Luis Gómez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Users
               WHERE  Email = N'luis.gomez@example.com')
    BEGIN
        INSERT  INTO dbo.Users (
            Name,
            Email
        )
        VALUES                 (N'Luis Gómez', N'luis.gomez@example.com');
    END


GO
-- Task 1: Pending status, assigned to Ana Pérez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Tasks
               WHERE  Title = N'Design data model'
                      AND UserId = (SELECT Id
                                    FROM   dbo.Users
                                    WHERE  Email = N'ana.perez@example.com'))
    BEGIN
        INSERT  INTO dbo.Tasks (
            Title,
            Status,
            UserId,
            AdditionalData
        )
        VALUES                 (N'Design data model', N'Pending', (SELECT Id
                                                                   FROM   dbo.Users
                                                                   WHERE  Email = N'ana.perez@example.com'), N'{"priority":"High","estimatedCompletion":"2026-12-15","tags":["backend","urgent"],"notes":"Review API integration"}');
    END


GO
-- Task 2: InProgress status, assigned to Luis Gómez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Tasks
               WHERE  Title = N'Implement task API'
                      AND UserId = (SELECT Id
                                    FROM   dbo.Users
                                    WHERE  Email = N'luis.gomez@example.com'))
    BEGIN
        INSERT  INTO dbo.Tasks (
            Title,
            Status,
            UserId,
            AdditionalData
        )
        VALUES                 (N'Implement task API', N'InProgress', (SELECT Id
                                                                       FROM   dbo.Users
                                                                       WHERE  Email = N'luis.gomez@example.com'), N'{"priority":"Medium","estimatedCompletion":"2026-11-30","tags":["api","dotnet"],"notes":"Expose CRUD in Swagger"}');
    END


GO
-- Task 3: Done status, assigned to Ana Pérez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Tasks
               WHERE  Title = N'Set up development environment'
                      AND UserId = (SELECT Id
                                    FROM   dbo.Users
                                    WHERE  Email = N'ana.perez@example.com'))
    BEGIN
        INSERT  INTO dbo.Tasks (
            Title,
            Status,
            UserId,
            AdditionalData
        )
        VALUES                 (N'Set up development environment', N'Done', (SELECT Id
                                                                             FROM   dbo.Users
                                                                             WHERE  Email = N'ana.perez@example.com'), N'{"priority":"Low","estimatedCompletion":"2026-11-10","tags":["setup"],"notes":"SQL Server + VS Code"}');
    END


-- ------------------------------------------------------------------
-- Section 5: examples of JSON queries in SQL Server.
-- Demonstrates the use of ISJSON, JSON_VALUE, JSON_QUERY, OPENJSON and
-- JSON_MODIFY on the AdditionalData column. The examples are commented
-- out: they are documentation and are not executed (the final UPDATE
-- would modify data).
-- ------------------------------------------------------------------
-- Validate that the content is valid JSON
-- SELECT Id, ISJSON(AdditionalData) AS IsJson FROM dbo.Tasks;
-- Read a field from the JSON (priority)
-- SELECT Id, JSON_VALUE(AdditionalData, '$.priority') AS Priority FROM dbo.Tasks;
-- Filter by a value inside the JSON (high priority)
-- SELECT Id, Title FROM dbo.Tasks
-- WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High';
-- Extract a complete JSON object/array (tags)
-- SELECT Id, JSON_QUERY(AdditionalData, '$.tags') AS Tags FROM dbo.Tasks;
-- Open the JSON array as rows (one row per tag)
-- SELECT t.Id, tag.[value] AS Tag
-- FROM dbo.Tasks t
-- CROSS APPLY OPENJSON(t.AdditionalData, '$.tags') AS tag;
-- (Optional) Update a field in the JSON
-- UPDATE dbo.Tasks
-- SET AdditionalData = JSON_MODIFY(AdditionalData, '$.estimatedCompletion', '2026-12-31')
-- WHERE Id = @TaskId;