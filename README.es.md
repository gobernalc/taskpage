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
├── .github/
│   └── workflows/
│       └── ci.yml              # integración continua de backend y frontend
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
├── frontend/                   # aplicación Angular
│   ├── public/
│   │   └── logo.svg            # marca usada por el icono y la barra lateral
│   └── src/
│       ├── assets/i18n/        # es.json (por defecto) y en.json
│       └── app/
│           ├── core/           # modelos, HTTP, i18n y manejo de errores
│           ├── features/       # usuarios y tareas (carga diferida)
│           └── shared/         # componentes reutilizables y diálogo de confirmación
├── README.md
└── README.es.md                # espejo en español de este documento
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
- **`-f 65001` (UTF-8):** `schema.sql` está guardado en UTF-8. El `sqlcmd` clásico
  (ODBC) lee los archivos con la página de códigos ANSI del sistema, así que agrega
  `-f 65001` (`sqlcmd -S localhost -E -f 65001 -i database\schema.sql`) o ejecuta
  el script desde la extensión mssql de VS Code. El `sqlcmd` moderno basado en Go
  (v1) lee UTF-8 por defecto.

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

Abrir Swagger en: <http://localhost:5017/swagger> (o el puerto que indique la consola).

### 4.2 Frontend

```bash
cd frontend
npm start
```

Abrir la aplicación en <http://localhost:4200>.

Los dos servidores se ejecutan a la vez, cada uno en su propia terminal: el frontend
llama a la API en `http://localhost:5017/api`. En desarrollo la API solo acepta el
origen del dev server de Angular mediante CORS, configurado en `Cors:AllowedOrigins`
dentro de `appsettings.Development.json`. La redirección a HTTPS se habilita fuera
de desarrollo para que el puerto local HTTP siga funcionando.

---

## 5. Pruebas

### 5.1 Pruebas del backend

```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

Cada test imprime en consola **✅ si pasa** y **❌ si falla**, con descripción
y un flujo legible del resultado de cada prueba.

La suite del backend cubre:

- `UserService`: creación válida, correo duplicado, normalización del correo (trim y
  minúsculas), actualización, eliminación con y sin tareas asociadas, consulta por id,
  búsqueda, ordenamiento y paginación.
- `TaskService`: creación, usuario inexistente, JSON válido e inválido, actualización,
  eliminación, transiciones de estado (rechazando la prohibida `Pending -> Done` y
  permitiendo las demás), cambios de estado idempotentes, filtros, búsqueda,
  ordenamiento y paginación.
- Validadores: campos obligatorios, longitudes máximas, formato de correo, estado
  inválido y documentos JSON inválidos.
- Controladores: códigos `200`/`201`/`204`/`400`/`404`/`409`, forma de las respuestas,
  cabeceras `Location` y validación de entrada.
- `GlobalExceptionMiddleware`: mapeo a `ProblemDetails`, cabecera `X-Request-Id` y
  correlación con `traceId`.
- `HealthController`: `200` con la base de datos disponible y `503` cuando no responde.

### 5.2 Pruebas del frontend

```bash
cd frontend
ng test --watch=false
```

Cada spec imprime en consola **✅ si pasa** y **❌ si falla** mediante un reporter
personalizado (`vitest.emoji-reporter.ts`).

La suite del frontend cubre:

- `UsersService`/`TasksService`: peticiones CRUD, parámetros de consulta, mapeo de
  respuestas paginadas y respuestas de error de red.
- `TranslateService`/`TranslatePipe`: carga de `es.json`/`en.json`, cambio y
  persistencia del idioma, claves faltantes, interpolación, conservación de acentos
  y conservación del diccionario anterior cuando falla una petición.
- `TaskList`: renderizado, enlaces al detalle, filtro de estado, búsqueda de título
  con debounce, paginación, estado vacío, eliminación con confirmación y errores de
  carga.
- `TaskDetail`: renderizado, metadatos, datos JSON adicionales, ids inválidos,
  confirmación de eliminación, cancelación y errores de carga.
- `TaskCreate`/`TaskEdit`: carga de usuarios, validación reactiva, payloads de
  envío, validación de JSON y errores de la API.
- `TaskStatus`: la transición prohibida `Pending -> Done`, las transiciones
  permitidas, el manejo de errores de la API y la restauración del valor del selector.
- `UserList`/`UserForm`: renderizado, búsqueda, eliminación con confirmación,
  validación reactiva y flujos de creación y edición.
- `ConfirmDialog`: contenido accesible del diálogo, salidas de confirmación y
  cancelación, cierre con Escape y con el fondo, estado ocupado y foco inicial.
- `Pagination` y `Badge`: navegación en los límites y enganches de tono de estado.
- Shell de la aplicación: renderizado de la barra lateral, persistencia del tema,
  cambio de idioma con banderas, ignorar el idioma activo, etiquetas accesibles y
  recuperación ante una petición fallida del diccionario.

### 5.3 Integración continua

El workflow de GitHub Actions (`.github/workflows/ci.yml`) se ejecuta en cada push a
`main` y en cada pull request contra `main`. Tiene dos jobs independientes:

- `backend`: `dotnet restore`, `dotnet build --no-restore` y
  `dotnet test --no-build --logger "console;verbosity=detailed"` sobre .NET 10.
- `frontend`: `npm ci`, `npm run build` y `ng test --watch=false` sobre Node 22.

Ambos jobs son checks requeridos para fusionar en `main`. Las cachés de NuGet y npm
mantienen el pipeline en el orden de minutos y se conserva la salida ✅/❌.

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

### 6.4 Validación (FluentValidation)

Los DTOs de entrada se validan con **FluentValidation** antes de llegar a los
servicios: campos obligatorios, longitudes máximas, formato de correo, identificadores
positivos, valores de enum, transiciones de estado válidas y documentos JSON válidos.
La validación está centralizada, la API devuelve mensajes legibles en inglés y las
entidades nunca se exponen.

### 6.5 Documentación de la API (Swagger)

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

### 6.6 Logging y observabilidad

- Un [`GlobalExceptionMiddleware`](backend/src/TaskPage.Api/Middleware/GlobalExceptionMiddleware.cs:1)
  global mapea las fallas de dominio y de validación a `ProblemDetails`
  (400/404/409/500) sin exponer detalles internos.
- Las respuestas de error incluyen la cabecera `X-Request-Id` y el identificador
  de correlación en `ProblemDetails.Extensions`, bajo la clave `traceId`.
- **Serilog** escribe logs estructurados en consola y en `logs/taskpage-.log`,
  correlacionando cada evento con el `TraceIdentifier` de la petición. Los correos se
  enmascaran y `AdditionalData` nunca se vuelca completo.

### 6.7 Seguridad

- Consultas **parametrizadas** mediante EF Core (sin concatenación de SQL).
- **Validación de entrada** en los DTOs con FluentValidation y límites de longitud.
- **Sin detalles internos** expuestos al cliente: solo `ProblemDetails`.
- **Sin datos sensibles** en los logs: correos enmascarados y sin payloads de
  `AdditionalData`.
- **HTTPS** habilitado fuera de desarrollo; `TrustServerCertificate=True` solo en
  desarrollo.
- **CORS restringido**: solo el origen configurado del dev server de Angular en
  desarrollo.
- **Pin de dependencia** para el paquete transitivo `Microsoft.Data.SqlClient`
  afectado por avisos conocidos.
- Control de respuestas con `[ProducesResponseType]` y códigos HTTP consistentes.

### 6.8 Estrategia de pruebas

- Backend: **xUnit** con pruebas de servicios, validadores, controladores,
  middleware y health check.
- Frontend: **Vitest 4.1.12** con un reporter personalizado que imprime ✅/❌ por spec.
- Regla de oro: **corregir la app, no la prueba**. Todo cambio debe dejar ambas
  suites en verde antes de entregarse.
- Escenarios cubiertos: positivos, negativos, datos incompletos, nulos, valores
  incorrectos, caracteres especiales, límites, transiciones de estado, idempotencia
  y recuperación ante fallos.

### 6.9 Base de datos

- Todo el esquema vive en **un único script idempotente** (`database/schema.sql`)
  que se puede ejecutar N veces sin efectos colaterales.
- Incluye claves primarias/foráneas, restricciones `CHECK`, valores `DEFAULT`,
  columnas de auditoría, índices clustered/nonclustered/unique y datos semilla
  idempotentes.
- El JSON se valida con `ISJSON`, se lee y filtra con `JSON_VALUE`, se extrae con
  `JSON_QUERY`, se expande con `OPENJSON` y se actualiza con `JSON_MODIFY`.

### 6.10 Diseño visual

El frontend aplica una paleta personalizada azul marino/azul profundo con una
identidad visual propia.

- Los design tokens viven en [`frontend/src/styles.scss`](frontend/src/styles.scss:1)
  como variables CSS: colores, degradados de 2 paradas, espaciados, radios,
  sombras, movimiento y tipografía. Los componentes consumen las variables, de
  modo que no se codifica ningún color, espaciado, radio o degradado.
- Un tema claro/oscuro conmutable sobrescribe los tokens mediante el selector
  `[data-theme="dark"]`, respeta `prefers-color-scheme` por defecto y persiste la
  elección en `localStorage` (clave no sensible).
- El shell de la aplicación ([`app.html`](frontend/src/app/app.html:1),
  [`app.scss`](frontend/src/app/app.scss:1)) es una **barra lateral colapsable** con
  marca, navegación con iconos, controles de tema e idioma, zona principal y pie. El
  estado colapsado se persiste, y el layout aporta degradados, sombras, transiciones
  sutiles y estados de foco accesibles.
- El **diálogo de confirmación** ([`confirm-dialog.ts`](frontend/src/app/shared/components/confirm-dialog/confirm-dialog.ts:1))
  es un modal centrado con línea de acento, fondo atenuado, trampa de foco, foco
  inicial en la acción segura y cierre con Escape y con el fondo.

### 6.11 Internacionalización

Todo texto visible de la UI se centraliza en `frontend/src/assets/i18n/es.json`
(idioma por defecto, español) y `frontend/src/assets/i18n/en.json`, y se referencia
por clave desde las plantillas — está prohibido codificar textos:

- Un [`TranslateService`](frontend/src/app/core/i18n/translate.service.ts:1)
  personalizado carga el diccionario activo, lo expone como signal, persiste el
  idioma en `localStorage` (clave no sensible), mantiene sincronizado
  `document.documentElement.lang` y conserva el diccionario anterior cuando falla una
  petición.
- Un [`TranslatePipe`](frontend/src/app/core/i18n/translate.pipe.ts:1)
  independiente renderiza las claves en las plantillas con interpolación simple
  `{{name}}`.
- El conmutador de idioma usa botones de bandera accesibles para español e inglés.
- Los estados (`Pending`, `InProgress`, `Done`), botones, títulos, placeholders y
  textos de accesibilidad viven en los archivos de idioma; la API sigue devolviendo
  mensajes en inglés y el frontend traduce solo su propia interfaz.

### 6.12 Funcionalidades de la interfaz

El frontend funcional gestiona tareas y usuarios de extremo a extremo:

- **Lista de tareas** ([`task-list.ts`](frontend/src/app/features/tasks/task-list/task-list.ts:1))
  lista tareas con filtro de estado, búsqueda de título con debounce y paginación,
  muestra la insignia de estado y el usuario asignado, enlaza cada título con su
  detalle y permite eliminar una tarea o cambiar su estado en línea (rechazando la
  transición prohibida `Pending -> Done`).
- **Detalle de tarea** ([`task-detail.ts`](frontend/src/app/features/tasks/task-detail/task-detail.ts:1))
  muestra la tarea completa, el usuario asignado, las fechas de auditoría localizadas
  y los datos JSON adicionales, con accesos directos para editar o eliminar.
- **Creación y edición de tareas** ([`task-create.ts`](frontend/src/app/features/tasks/task-create/task-create.ts:1),
  [`task-edit.ts`](frontend/src/app/features/tasks/task-edit/task-edit.ts:1)) son
  formularios reactivos con título, selector de usuario asignado y datos adicionales
  JSON opcionales, validados en el cliente antes de enviar.
- **Lista de usuarios** ([`user-list.ts`](frontend/src/app/features/users/user-list/user-list.ts:1))
  lista usuarios con búsqueda y paginación y permite eliminarlos.
- **Formulario de usuario** ([`user-form.ts`](frontend/src/app/features/users/user-form/user-form.ts:1))
  crea y edita usuarios con validación de nombre y correo.
- **Acciones destructivas** pasan siempre por el diálogo de confirmación accesible y
  nunca por un prompt nativo.
- **Componentes compartidos** (`badge`, `button`, `card`, `select`, `pagination` y
  `confirm-dialog`) se reutilizan en cada funcionalidad en lugar de duplicar marcado.
- **Gestión de errores** lee el `ProblemDetails` devuelto por la API y muestra un
  mensaje legible sin exponer detalles internos.

Rutas: `/tasks` (lista), `/tasks/new` (crear), `/tasks/:id` (detalle),
`/tasks/:id/edit` (editar), `/users` (lista), `/users/new` (crear) y
`/users/:id/edit` (editar).

### 6.13 Propuestas adicionales implementadas

Las siguientes funcionalidades van más allá de la especificación original y están
completamente implementadas:

- **CRUD ampliado**: actualización y eliminación de usuarios y tareas, además del
  cambio de estado de tareas.
- **Paginación, búsqueda y ordenamiento** en ambos listados, con un modelo de
  resultado paginado.
- **Columnas de auditoría** (`CreatedAt`, `UpdatedAt`) en la base de datos y en las
  respuestas de la API.
- **Normalización y unicidad del correo**, comparado sin distinguir mayúsculas.
- **JSON en SQL Server** para datos adicionales de las tareas, con validación y
  funciones nativas.
- **Persistencia con EF Core 10** con mapeos tipados y consultas parametrizadas.
- **FluentValidation** para validación de entrada centralizada con mensajes en inglés.
- **Serilog** con logging estructurado, correlación por `TraceIdentifier` y
  enmascaramiento de datos personales.
- **Cabecera `X-Request-Id`** y `traceId` en `ProblemDetails` para correlacionar
  peticiones.
- **Health check de base de datos** en `GET /api/health`.
- **Tema claro/oscuro** con design tokens, soporte de preferencia del sistema y
  persistencia.
- **Internacionalización español/inglés** con botones de bandera, idioma persistido y
  sincronización del idioma del documento.
- **Barra lateral colapsable** con riel de iconos, marca, estado persistido y
  controles accesibles.
- **Diálogo de confirmación** para acciones destructivas con gestión de foco, Escape y
  cierre con el fondo.
- **Página de detalle de tarea** con fechas de auditoría y datos JSON adicionales.
- **Pruebas automatizadas** con xUnit y Vitest, incluido el reporter de emojis ✅/❌.
- **Integración continua** con GitHub Actions (jobs de backend y frontend).
- **Configuración de CORS de desarrollo** gobernada por variables de configuración.
- **Endurecimiento de seguridad**: CORS restringido, sin detalles internos en errores,
  datos personales enmascarados y dependencia parcheada fijada.
- **Accesibilidad**: landmarks semánticos, etiquetas, estados de foco, trampa de foco
  y enganches de contraste de estados.
- **Diseño responsivo** con barra lateral adaptable y espaciados basados en tokens.
- **README bilingüe** (`README.md` y `README.es.md`) mantenido sincronizado.

### 6.14 Limitaciones conocidas y siguientes pasos

Cada punto es una funcionalidad futura con su beneficio; nada de esto queda
parcialmente habilitado:

- **Autenticación y autorización con JWT/OIDC** (Microsoft.Identity.Web + Entra ID,
  políticas por rol y `[Authorize]` por controlador): controla quién crea, edita y
  elimina, y deja el sistema listo para auditoría.
- **Concurrencia optimista con `rowversion`/ETag**: evita que dos usuarios sobrescriban
  la misma tarea (lost update) y devuelve 409/412 cuando ocurre.
- **Migraciones versionadas ejecutadas por el pipeline** (EF Core Migrations, DbUp o
  Flyway): despliegues repetibles y trazables sin pasos manuales.
- **Observabilidad ampliada**: health checks `live`/`ready`, sink central de logs y
  trazas distribuidas (OpenTelemetry/Application Insights).
- **Versionado de la API con `Asp.Versioning`**: evolución sin romper clientes.
- **Pruebas end-to-end con Playwright y accesibilidad automatizada con `axe-core`**:
  cubren el flujo completo y el contraste de estados.
- **Rate limiting y cabeceras de seguridad** (HSTS, CSP y `X-Content-Type-Options`):
  protegen el Swagger y la API expuestos.
- **Entornos separados con secretos gestionados** (Key Vault) y despliegue
  reproducible en contenedores.
- **Demo pública en una única URL**: la fase opcional de despliegue no se ejecutó; la
  app sigue ejecutándose localmente con `ng serve` + `dotnet run`, y publicar el build
  de Angular desde `wwwroot` queda documentado como siguiente paso.

---

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
- Estado del proyecto: están implementadas las **fases 1 a 8** (base de datos, capa
  de dominio y aplicación, persistencia en SQL Server, API REST con Swagger, pruebas
  del backend, base del frontend, frontend funcional y pruebas del frontend). La fase
  final de integración y documentación está completa, incluyendo las adiciones de
  menú de colaboración, tema, idioma, diálogo de confirmación y detalle de tarea. La
  demo de despliegue opcional (fase 10) queda documentada como siguiente paso con su
  beneficio.
