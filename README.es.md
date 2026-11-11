# TaskPage

> Para ver este documento en inglés, consulta [README.md](README.md).

Prueba técnica: **API REST en .NET (Web API) + Angular + SQL Server**.

Solución full-stack para un sistema interno de gestión de tareas, con backend en
**.NET 10 (ASP.NET Core Web API)** organizado con Clean Architecture, frontend
**Angular 22** (standalone components + SCSS) y base de datos **SQL Server**. La API
queda expuesta y documentada en **Swagger**.

---

## 1. Requisitos previos

| Herramienta        | Versión verificada                                                        |
| ------------------ | ------------------------------------------------------------------------- |
| Node.js            | 22.23.4 (LTS)                                                             |
| npm                | 10.9.9                                                                    |
| .NET SDK           | 10.0.101 (.NET 10 LTS)                                                    |
| Angular CLI        | 22.2.6                                                                    |
| SQL Server         | 2022 o superior (verificado con 2025 Developer)                           |
| sqlcmd             | utilidad de línea de comandos de SQL Server                               |
| Visual Studio Code | con extensiones C# Dev Kit, Angular Language Service y SQL Server (mssql) |

---

## 2. Estructura del repositorio

```text
taskpage/
├── .gitignore
├── README.md
├── README.es.md                # espejo en español de este documento
├── database/
│   └── schema.sql              # único script SQL idempotente
├── backend/                    # solución .NET (Clean Architecture)
│   └── src/
│       ├── TaskPage.Domain/
│       ├── TaskPage.Application/
│       ├── TaskPage.Infrastructure/
│       ├── TaskPage.Api/
│       ├── TaskPage.Application.Tests/
│       └── TaskPage.Api.Tests/
└── frontend/                   # aplicación Angular
```

---

## 3. Instalación y configuración paso a paso

### 3.1 Clonar e instalar dependencias del backend

```bash
git clone https://github.com/gobernalc/taskpage.git taskpage
cd taskpage/backend
dotnet restore
```

### 3.2 Instalar dependencias del frontend

```bash
cd taskpage/frontend
npm install --legacy-peer-deps
```

### 3.3 Crear la base de datos

Ejecutar el script idempotente en una instancia local de SQL Server:

```bash
sqlcmd -S localhost -E -i database\schema.sql
```

- `-S localhost`: instancia por defecto. Si tu instancia es nombrada, usa
  `localhost\NOMBRE_INSTANCIA`.
- `-E`: autenticación de Windows (cuenta actual como `sysadmin`).

El script crea la base de datos `TaskPageDb` (si no existe), las tablas `Users`
y `Tasks` con sus claves primarias/foráneas, `CHECK`, `DEFAULT` y auditoría
(`CreatedAt`/`UpdatedAt`), los índices para búsqueda y orden, y los datos semilla
(2 usuarios y 3 tareas con `AdditionalData` en JSON). Se puede ejecutar
**N veces sin efectos colaterales**: las tablas, restricciones, índices y el seed
solo se crean/insertan la primera vez.

- **`N'...'` (prefijo Unicode):** en SQL Server el prefijo `N` delante de una
  cadena indica un literal `NVARCHAR` (Unicode). Se usa en las columnas de texto
  (`Name`, `Email`, `Title`, `Status`, `AdditionalData`) para que los literales se
  interpreten como Unicode y no se pierdan caracteres (acentos, `ñ`, etc.).
- **Datos semilla opcionales:** el seed (2 usuarios y 3 tareas) es solo contenido
  de demostración. Si no se desea, se puede comentar o eliminar la sección
  "Datos semilla" del script sin afectar el esquema.

Verificación rápida tras ejecutarlo:

```bash
sqlcmd -S localhost -E -d TaskPageDb -Q "SELECT COUNT(*) AS Users FROM dbo.Users; SELECT COUNT(*) AS Tasks FROM dbo.Tasks;"
```

Consulta de ejemplo con JSON (`ISJSON`, `JSON_VALUE`, `JSON_QUERY`, `OPENJSON`
y `JSON_MODIFY`) sobre la columna `Tasks.AdditionalData`:

```sql
-- Validar que el contenido es JSON válido
SELECT Id, ISJSON(AdditionalData) AS IsJson FROM dbo.Tasks;

-- Leer un campo del JSON (prioridad)
SELECT Id, JSON_VALUE(AdditionalData, '$.priority') AS Priority FROM dbo.Tasks;

-- Filtrar por un valor dentro del JSON (prioridad alta)
SELECT Id, Title FROM dbo.Tasks
WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High';

-- Extraer un objeto/array completo del JSON (etiquetas)
SELECT Id, JSON_QUERY(AdditionalData, '$.tags') AS Tags FROM dbo.Tasks;

-- Abrir el array del JSON como filas (una fila por etiqueta)
SELECT t.Id, tag.[value] AS Tag
FROM dbo.Tasks t
CROSS APPLY OPENJSON(t.AdditionalData, '$.tags') AS tag;

-- (Opcional) Actualizar un campo del JSON
UPDATE dbo.Tasks
SET AdditionalData = JSON_MODIFY(AdditionalData, '$.estimatedCompletion', '2026-12-31')
WHERE Id = 1;
```

### 3.4 Configurar la cadena de conexión

En `backend/src/TaskPage.Api/appsettings.Development.json`:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=TaskPageDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
}
```

También se puede configurar con la variable de entorno
`ConnectionStrings__DefaultConnection` sin tocar el archivo.

---

## 4. Ejecución

### 4.1 Backend (API + Swagger)

```bash
cd backend/src/TaskPage.Api
dotnet run
```

Abrir Swagger en: <http://localhost:5000/swagger> (o el puerto que indique la consola).

### 4.2 Frontend

```bash
cd frontend
ng serve
```

Abrir la aplicación en <http://localhost:4200>.

---

## 5. Pruebas

### 5.1 Pruebas del backend

```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

Cada test imprime en consola **✅ si pasa** y **❌ si falla**, con descripción
y un flujo legible del resultado de cada prueba.

### 5.2 Pruebas del frontend

```bash
cd frontend
ng test --watch=false
```

Cada spec imprime en consola **✅ si pasa** y **❌ si falla** mediante un reporter
personalizado (`vitest.emoji-reporter.ts`).

---

## 6. Decisiones técnicas

### 6.1 Arquitectura

El backend sigue Clean Architecture, con dependencias en una sola dirección
desde las capas externas hacia el centro:

- `TaskPage.Domain`: entidades, enumeraciones y excepciones de dominio
  (sin dependencias).
- `TaskPage.Application`: casos de uso, DTOs, validación, filtros e interfaces de
  repositorio.
- `TaskPage.Infrastructure`: persistencia con EF Core e implementaciones de
  repositorio.
- `TaskPage.Api`: controladores, middleware y el pipeline HTTP.

### 6.2 Reglas de negocio implementadas

Las capas de dominio y aplicación son dueñas de las reglas de negocio:

- El título de la tarea es obligatorio (1-200 caracteres).
- Toda tarea debe tener un usuario asignado que ya exista.
- Cambiar una tarea directamente de Pending a Done se rechaza: debe pasar primero
  por InProgress.
- Un usuario con tareas asociadas no se puede eliminar (409 Conflict).
- El correo se normaliza (recorta espacios y se pasa a minúsculas) y debe ser
  único, comparado sin distinguir mayúsculas.

### 6.3 Persistencia (EF Core)

La capa de infraestructura persiste los datos con **EF Core 10** y el proveedor
de SQL Server. El [`AppDbContext`](backend/src/TaskPage.Infrastructure/Persistence/AppDbContext.cs:1)
mapea las entidades `User` y `TaskItem` a las tablas `Users` y `Tasks` mediante
[`UserConfiguration`](backend/src/TaskPage.Infrastructure/Persistence/Configurations/UserConfiguration.cs:1)
y [`TaskConfiguration`](backend/src/TaskPage.Infrastructure/Persistence/Configurations/TaskConfiguration.cs:1):
claves, columnas obligatorias, longitudes máximas, la conversión del enum
`TaskState` a texto, relaciones e índices quedan alineados con
[`database/schema.sql`](database/schema.sql:1).

- [`UserRepository`](backend/src/TaskPage.Infrastructure/Repositories/UserRepository.cs:1)
  y [`TaskRepository`](backend/src/TaskPage.Infrastructure/Repositories/TaskRepository.cs:1)
  implementan las interfaces de la aplicación `IUserRepository` e
  `ITaskRepository`.
- El listado de tareas admite **filtros** (estado y usuario asignado),
  **búsqueda** (título), **orden** (título, estado y fecha de creación) y
  **paginación**.
- El filtro de prioridad lee la columna JSON con la función nativa
  **`JSON_VALUE(AdditionalData, '$.priority')`**, traducida por EF Core mediante
  una función de base de datos mapeada para que la consulta siga parametrizada.
- [`DependencyInjection.cs`](backend/src/TaskPage.Infrastructure/DependencyInjection.cs:1)
  expone `AddInfrastructure`, que registra `AppDbContext` con la cadena de conexión
  `DefaultConnection` y los repositorios como servicios scoped.
- El esquema físico sigue perteneciendo a [`database/schema.sql`](database/schema.sql:1)
  (único script idempotente): EF Core se usa para consultas y comandos, no para
  generar el esquema.

### 6.4 Documentación de la API

La API REST queda expuesta y documentada en **Swagger** (UI en `/swagger`, JSON en
`/swagger/v1/swagger.json`). Todos los endpoints se pueden probar desde la UI de
Swagger y documentan sus modelos de entrada, parámetros de consulta y códigos de
respuesta.

| Método   | Ruta                     | Descripción                                                                      |
| -------- | ------------------------ | -------------------------------------------------------------------------------- |
| `POST`   | `/api/users`             | Crea un usuario (201 + `Location`, 400 validación, 409 correo duplicado).        |
| `GET`    | `/api/users`             | Lista usuarios con búsqueda, orden y paginación (200).                           |
| `GET`    | `/api/users/{id}`        | Obtiene un usuario por id (200, 404).                                            |
| `PUT`    | `/api/users/{id}`        | Reemplaza el nombre y el correo de un usuario (200, 400, 404, 409).              |
| `DELETE` | `/api/users/{id}`        | Elimina un usuario sin tareas asociadas (204, 404, 409).                         |
| `POST`   | `/api/tasks`             | Crea una tarea asignada a un usuario (201 + `Location`, 400, 404).               |
| `GET`    | `/api/tasks`             | Lista tareas con filtros, búsqueda, orden y paginación (200).                    |
| `GET`    | `/api/tasks/{id}`        | Obtiene una tarea por id (200, 404).                                             |
| `PUT`    | `/api/tasks/{id}`        | Reemplaza los valores editables de una tarea (200, 400, 404).                    |
| `DELETE` | `/api/tasks/{id}`        | Elimina una tarea (204, 404).                                                    |
| `PUT`    | `/api/tasks/{id}/status` | Cambia el estado de una tarea respetando la regla de transición (200, 400, 404). |
| `GET`    | `/api/health`            | Informa el estado del servicio y de la base de datos (200, 503).                 |

Comportamiento transversal:

- Un [`GlobalExceptionMiddleware`](backend/src/TaskPage.Api/Middleware/GlobalExceptionMiddleware.cs:1)
  global mapea las fallas de dominio y de validación a `ProblemDetails`
  (400/404/409/500) sin exponer detalles internos.
- Las respuestas de error incluyen la cabecera `X-Request-Id` y el identificador
  de correlación en `ProblemDetails.Extensions`, bajo la clave `traceId`.
- **Serilog** escribe logs estructurados en consola y en `logs/taskpage-.log`,
  correlacionando cada evento con el `TraceIdentifier` de la petición y enmascarando
  los datos sensibles.

## 7. Ramas y entrega

El repositorio sigue **trunk-based development**:

- `main` es la única rama de larga vida y siempre está verde y desplegable. No
  existen ramas `dev` ni `qa`.
- Cada cambio vive en una rama corta creada desde `main` actualizado:
  `feat/<slug>`, `fix/<slug>`, `chore/<slug>`, `docs/<slug>` o `test/<slug>`.
  Una rama vive como máximo dos días y mantiene el diff alrededor de 400 líneas.
- La rama llega a `main` mediante un pull request con una aprobación del tech lead,
  el CI en verde, squash merge y borrado inmediato de la rama. `main` está
  protegida: sin pushes directos y sin force pushes.
- Una liberación es un tag semver (`vMAJOR.MINOR.PATCH`) creado sobre `main`. Los
  entornos son destinos de despliegue configurados con variables de entorno, no
  ramas.
- Los hotfix salen del tag afectado como `fix/<slug>`, entran a `main` por pull
  request y generan un tag de parche.

Flujo diario:

```bash
git switch main && git pull --ff-only
git switch -c feat/task-status-transition
# trabajar, hacer commit y push
git push -u origin feat/task-status-transition
# abrir el pull request contra main, squash merge y borrar la rama
git switch main && git pull --ff-only
```

## 8. Notas

- El esquema de base de datos vive en un **único script idempotente**
  (`database/schema.sql`), que se puede ejecutar N veces sin efectos colaterales.
- La cadena de conexión es configurable para que cualquier evaluador pueda montar la
  base de datos en su propia instancia de SQL Server.
- El runner de pruebas del frontend es **Vitest 4.1.12** (configurado en `angular.json`
  con el reporter personalizado `vitest.emoji-reporter.ts`, que imprime ✅/❌ por spec).
- Estado del proyecto: la configuración del entorno y la estructura del proyecto
  están completas, y están implementadas la **Fase 1 (esquema de base de datos)**,
  la **Fase 2 (capa de dominio y aplicación)**, la
  **Fase 3 (persistencia en SQL Server)** y la
  **Fase 4 (API REST con Swagger)**. Las capas de backend y frontend
  seguirán creciendo fase a fase según [`plans/design.md`](plans/design.md:1).
