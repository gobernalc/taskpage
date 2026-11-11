-- ==================================================================
-- Script  : database/schema.sql
-- Fecha   : 2026-11-11
-- Autor   : gobernalc
-- Resumen : Esquema único e idempotente de la base de datos TaskPage.
--           Crea la base de datos, las tablas Users y Tasks (con PK,
--           FK, CHECK, DEFAULT y auditoría), los índices y los datos
--           semilla opcionales. Debe poder ejecutarse N veces sin
--           efectos colaterales y funcionar en cualquier instancia de
--           SQL Server.
-- ==================================================================
-- ------------------------------------------------------------------
-- Sección 1: creación de la base de datos.
-- Fecha : 2026-11-11 | Autor : gobernalc
-- Resumen: Crea la base de datos TaskPageDb si todavía no existe.
-- ------------------------------------------------------------------
IF DB_ID(N'TaskPageDb') IS NULL
    BEGIN
        CREATE DATABASE TaskPageDb;
    END


GO
USE TaskPageDb;


GO
-- ------------------------------------------------------------------
-- Sección 2: tablas.
-- Fecha : 2026-11-11 | Autor : gobernalc
-- Resumen: Crea las tablas Users y Tasks con sus claves primarias,
--          foráneas, CHECK y DEFAULT. La creación es condicional para
--          que el script sea idempotente.
-- ------------------------------------------------------------------
-- Tabla Users: colaboradores del sistema.
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Users (
            Id        INT            IDENTITY (1, 1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY, -- PK clustered sobre Id
            Name      NVARCHAR (100) NOT NULL, -- Nombre del colaborador
            Email     NVARCHAR (200) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE, -- Correo único
            CreatedAt DATETIME2 (0)  -- Fecha de creación
            CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()) NOT NULL,
            UpdatedAt DATETIME2 (0)  NULL -- Fecha de última modificación
        );
    END


GO
-- Tabla Tasks: tareas con información adicional en JSON.
IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Tasks (
            Id             INT            IDENTITY (1, 1) NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY, -- PK clustered sobre Id
            Title          NVARCHAR (200) NOT NULL, -- Título obligatorio
            Status         NVARCHAR (20)  -- Pending / InProgress / Done
            CONSTRAINT DF_Tasks_Status DEFAULT (N'Pending') NOT NULL,
            UserId         INT            NOT NULL, -- Usuario asignado
            CreatedAt      DATETIME2 (0)  -- Fecha de creación
            CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()) NOT NULL,
            UpdatedAt      DATETIME2 (0)  NULL, -- Fecha de última modificación
            AdditionalData NVARCHAR (MAX) NULL, -- JSON adicional
            CONSTRAINT FK_Tasks_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
            CONSTRAINT CK_Tasks_Status CHECK (Status IN (N'Pending', N'InProgress', N'Done')),
            CONSTRAINT CK_Tasks_AdditionalData_IsJson CHECK (AdditionalData IS NULL
                                                             OR ISJSON(AdditionalData) = 1)
        );
    END


GO
-- ------------------------------------------------------------------
-- Sección 3: índices.
-- Fecha : 2026-11-11 | Autor : gobernalc
-- Resumen: Crea los índices nonclustered necesarios para búsqueda,
--          orden y filtros. La creación condicional se ejecuta de forma
--          dinámica porque CREATE INDEX debe iniciar su propio lote.
-- ------------------------------------------------------------------
-- Índice compuesto que sirve a la consulta de tareas por usuario,
-- estado y orden por fecha de creación descendente.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Tasks_UserId_Status_CreatedAt'
                      AND object_id = OBJECT_ID(N'dbo.Tasks'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Tasks_UserId_Status_CreatedAt
                 ON dbo.Tasks (UserId, Status, CreatedAt DESC);');
    END


GO
-- Índice para el filtro por estado.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Tasks_Status'
                      AND object_id = OBJECT_ID(N'dbo.Tasks'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Tasks_Status
                 ON dbo.Tasks (Status);');
    END


GO
-- Índice para la búsqueda por texto en el título.
IF NOT EXISTS (SELECT 1
               FROM   sys.indexes
               WHERE  name = N'IX_Tasks_Title'
                      AND object_id = OBJECT_ID(N'dbo.Tasks'))
    BEGIN
        EXECUTE (N'CREATE NONCLUSTERED INDEX IX_Tasks_Title
                 ON dbo.Tasks (Title);');
    END


GO
-- Índice para la búsqueda y el orden por nombre de usuario.
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
-- Sección 4: datos semilla (opcionales).
-- Fecha : 2026-11-11 | Autor : gobernalc
-- Resumen: Inserta los usuarios y tareas de ejemplo una única vez.
--          Estos datos son OPCIONALES: solo sirven para tener contenido
--          de demostración. Si no se desean, basta con comentar o
--          eliminar esta sección completa; el esquema (tablas,
--          restricciones e índices) no depende de ellos. La condición
--          por Email y por Title + UserId garantiza que las ejecuciones
--          repetidas no dupliquen datos.
-- ------------------------------------------------------------------
-- Usuario: Ana Pérez.
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
-- Usuario: Luis Gómez.
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
-- Tarea 1: estado Pending, asignada a Ana Pérez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Tasks
               WHERE  Title = N'Diseñar modelo de datos'
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
        VALUES                 (N'Diseñar modelo de datos', N'Pending', (SELECT Id
                                                                         FROM   dbo.Users
                                                                         WHERE  Email = N'ana.perez@example.com'), N'{"priority":"High","estimatedCompletion":"2026-12-15","tags":["backend","urgent"],"notes":"Revisar integración con la API"}');
    END


GO
-- Tarea 2: estado InProgress, asignada a Luis Gómez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Tasks
               WHERE  Title = N'Implementar API de tareas'
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
        VALUES                 (N'Implementar API de tareas', N'InProgress', (SELECT Id
                                                                              FROM   dbo.Users
                                                                              WHERE  Email = N'luis.gomez@example.com'), N'{"priority":"Medium","estimatedCompletion":"2026-11-30","tags":["api","dotnet"],"notes":"Exponer CRUD en Swagger"}');
    END


GO
-- Tarea 3: estado Done, asignada a Ana Pérez.
IF NOT EXISTS (SELECT 1
               FROM   dbo.Tasks
               WHERE  Title = N'Configurar entorno de desarrollo'
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
        VALUES                 (N'Configurar entorno de desarrollo', N'Done', (SELECT Id
                                                                               FROM   dbo.Users
                                                                               WHERE  Email = N'ana.perez@example.com'), N'{"priority":"Low","estimatedCompletion":"2026-11-10","tags":["setup"],"notes":"SQL Server + VS Code"}');
    END


-- ------------------------------------------------------------------
-- Sección 5: ejemplos de consultas con JSON en SQL Server.
-- Fecha : 2026-11-11 | Autor : gobernalc
-- Resumen: Demuestra el uso de ISJSON, JSON_VALUE, JSON_QUERY,
--          OPENJSON y JSON_MODIFY sobre la columna AdditionalData.
--          Estos ejemplos quedan comentados: son documentación y no se
--          ejecutan con el script (el UPDATE final modificaría datos).
-- ------------------------------------------------------------------
-- Validar que el contenido es JSON válido
-- SELECT Id, ISJSON(AdditionalData) AS EsJson FROM dbo.Tasks;
-- Leer un campo del JSON (prioridad)
-- SELECT Id, JSON_VALUE(AdditionalData, '$.priority') AS Priority FROM dbo.Tasks;
-- Filtrar por un valor dentro del JSON (prioridad alta)
-- SELECT Id, Title FROM dbo.Tasks
-- WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High';
-- Extraer un objeto/array completo del JSON (etiquetas)
-- SELECT Id, JSON_QUERY(AdditionalData, '$.tags') AS Tags FROM dbo.Tasks;
-- Abrir el array del JSON como filas (una fila por etiqueta)
-- SELECT t.Id, tag.[value] AS Tag
-- FROM dbo.Tasks t
-- CROSS APPLY OPENJSON(t.AdditionalData, '$.tags') AS tag;
-- (Opcional) Actualizar un campo del JSON
-- UPDATE dbo.Tasks
-- SET AdditionalData = JSON_MODIFY(AdditionalData, '$.estimatedCompletion', '2026-12-31')
-- WHERE Id = @TaskId;