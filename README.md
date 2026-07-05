# TwentyNet

Reescritura progresiva del backend de Twenty CRM en ASP.NET Core 8 con EF Core, MediatR y arquitectura limpia.

## Stack

- **.NET 8** / **C# 12**
- **ASP.NET Core Web API** con controllers (no Minimal APIs)
- **Entity Framework Core 8** con **PostgreSQL** (Npgsql)
- **MediatR 12** para commands y queries
- **FluentValidation** para validación de inputs
- **AutoMapper** para mapeo entre entidades, DTOs y contracts
- **xUnit + NSubstitute + EF Core InMemory** para tests

## Estructura

```
TwentyNet.sln
├── src/
│   ├── TwentyNet.Domain/          # Entidades, value objects, eventos, interfaces de repositorio
│   ├── TwentyNet.Contracts/       # Request/response contracts compartidos
│   ├── TwentyNet.Application/     # Comandos, queries, handlers, DTOs, validadores, perfiles AutoMapper
│   ├── TwentyNet.Persistence/     # EF Core DbContext, configuraciones, repositorios, migrations
│   └── TwentyNet.BFF/             # ASP.NET Core Web API, controllers, DI, Swagger, HttpClient
└── tests/
    └── TwentyNet.Application.Tests/  # Tests de handlers y validadores
```

## Dominio inicial

- **Workspace**: contenedor principal de datos.
- **User**: usuario con email, nombre y apellido.
- **Company**: empresa con nombre, dominio y dirección.
- **Person**: contacto con nombre, email, teléfono y relación opcional con una empresa.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) 14+
- `dotnet-ef` CLI (opcional, para migrations):

```bash
dotnet tool install --global dotnet-ef
```

## Configuración local

Edita `src/TwentyNet.BFF/appsettings.json` (o `appsettings.Development.json`) con tu connection string de PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=twentynet;Username=postgres;Password=tu_password"
  }
}
```

> No incluyas secrets en el repositorio. Usa variables de entorno o `appsettings.Local.json` ( ignorado por `.gitignore`).

## Migrations

Desde la raíz del repositorio:

```bash
# Crear una nueva migration (ejemplo)
dotnet ef migrations add NombreMigration --project src/TwentyNet.Persistence --startup-project src/TwentyNet.BFF

# Aplicar migrations a la base de datos
dotnet ef database update --project src/TwentyNet.Persistence --startup-project src/TwentyNet.BFF
```

La migración inicial `InitialCreate` ya está generada en `src/TwentyNet.Persistence/Migrations`.

## Ejecutar

```bash
dotnet build
dotnet run --project src/TwentyNet.BFF
```

La API expone Swagger en: `https://localhost:7001/swagger` (puerto puede variar).

## Seed opcional

Puedes insertar un workspace inicial directamente con SQL o mediante un script. La API no incluye seed automático en este MVP.

## Endpoints

### Companies

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies?workspaceId={id}` | Listar empresas de un workspace |
| GET | `/api/companies/{id}` | Obtener empresa por id |
| POST | `/api/companies` | Crear empresa |
| PUT | `/api/companies/{id}` | Actualizar empresa |
| DELETE | `/api/companies/{id}` | Eliminar empresa |

**Ejemplo POST /api/companies:**

```json
{
  "name": "Twenty CRM",
  "domainName": "twenty.com",
  "address": "123 Main St",
  "workspaceId": "00000000-0000-0000-0000-000000000000"
}
```

### People

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/people?workspaceId={id}` | Listar personas de un workspace |
| GET | `/api/people/{id}` | Obtener persona por id |
| POST | `/api/people` | Crear persona |
| PUT | `/api/people/{id}` | Actualizar persona |
| DELETE | `/api/people/{id}` | Eliminar persona |

**Ejemplo POST /api/people:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phone": "+1 555 1234",
  "companyId": null,
  "workspaceId": "00000000-0000-0000-0000-000000000000"
}
```

## Tests

```bash
dotnet test
```

## Decisiones técnicas

- **Repositorio genérico (`EfRepository<T>`)**: cubre CRUD básico y delega queries específicas a los handlers.
- **Unit of work**: `AppDbContext` implementa `IUnitOfWork`; los handlers usan `SaveChangesAsync` del repositorio.
- **Value objects**: `Email` y `PhoneNumber` encapsulan validación básica.
- **Named HttpClient**: `EnrichmentClient` configurado desde `IOptions<HttpClientOptions>` para futuras integraciones de enriquecimiento de datos.

## Limitaciones conocidas

- AutoMapper tiene una advertencia de vulnerabilidad conocida (NU1903 / GHSA-rvv3-g6hj-g44x) en las versiones disponibles para .NET 8; se suprime el warning mientras se espera un patch o se evalúa reemplazo por mapeo manual.
- No hay autenticación ni autorización en este MVP.
- No hay seed automático de datos.
