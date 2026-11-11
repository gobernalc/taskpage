# TaskPage

Prueba técnica: **API REST en .NET (Web API) + Angular + SQL Server**.

Solución full-stack con backend en **.NET 10 (ASP.NET Core Web API)** bajo Clean
Architecture, frontend **Angular 21** (standalone components + SCSS) y base de datos
**SQL Server 2022**. Toda la API queda expuesta y documentada en **Swagger**.

> El catálogo de funcionalidades se refleja en Swagger (`/swagger`) una vez
> implementados los servicios de la prueba.

---

## 1. Requisitos previos

| Herramienta        | Versión verificada                                                        |
| ------------------ | ------------------------------------------------------------------------- |
| Node.js            | 22.17.1 (LTS)                                                             |
| npm                | 10.9.2                                                                    |
| .NET SDK           | 10.0.101 (.NET 10 LTS)                                                    |
| Angular CLI        | 21.2.29 (global)                                                          |
| SQL Server         | 2022 Developer o Express                                                  |
| sqlcmd             | utilidad de línea de comandos de SQL Server                               |
| Visual Studio Code | con extensiones C# Dev Kit, Angular Language Service y SQL Server (mssql) |

---

## 2. Estructura del repositorio

```text
taskpage/
├── .gitignore
├── README.md                   # único documento .md del proyecto
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

### 3.2 Crear la base de datos

Ejecutar el script idempotente en una instancia local de SQL Server:

```bash
sqlcmd -S localhost -E -i ..\database\schema.sql
```

- `-S localhost`: instancia por defecto. Si tu instancia es nombrada, usa
  `localhost\NOMBRE_INSTANCIA`.
- `-E`: autenticación de Windows (cuenta actual como `sysadmin`).

El script se puede ejecutar **N veces sin efectos colaterales** y funciona en
cualquier instancia de SQL Server.

### 3.3 Configurar la cadena de conexión

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
Desde la UI de Swagger se pueden probar todos los servicios de la API.

---

## 5. Pruebas

### 5.1 Pruebas del backend

```bash
cd backend
dotnet test
```

Cada test imprime en consola **✅ si pasa** y **❌ si falla**, con descripción y un flujo legible del resultado de cada prueba.

### 5.2 Pruebas del frontend

```bash
cd frontend
ng test
```

Cada spec imprime en consola **✅ si pasa** y **❌ si falla**.

---

## 6. Notas

- El esquema completo de base de datos vive en un **único script idempotente**
  (`database/schema.sql`) con tablas, claves, CHECK, DEFAULT, índices y datos semilla.
- La cadena de conexión es configurable para que cualquier evaluador pueda montar la
  base de datos en su propia instancia de SQL Server.
