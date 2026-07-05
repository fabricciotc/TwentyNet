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
- **JWT Bearer** + **BCrypt** para autenticación custom

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

## Dominio

- **Workspace**: contenedor principal de datos.
- **User**: usuario con email, nombre, apellido, hash de contraseña y estado.
- **UserWorkspaceMembership**: relación muchos-a-muchos entre usuarios y workspaces con rol.
- **RefreshToken**: tokens de refresco rotatorios, revocables y con expiración.
- **Company**: empresa con nombre, dominio y dirección.
- **Person**: contacto con nombre, email, teléfono y relación opcional con una empresa.
- **File**: archivo con nombre, mime type, tamaño, carpeta (`Attachment`, `Avatar`, `EmailAttachment`), storage key, workspace y estado (`Pending`/`Uploaded`). Soporta soft delete y puede asociarse a `Person` o `Company` como attachment.

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
  },
  "Jwt": {
    "Issuer": "TwentyNet",
    "Audience": "TwentyNet",
    "SecretKey": "your-super-secret-key-min-32-chars!",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

> **Importante:** cambia `Jwt:SecretKey` en producción por un valor aleatorio de al menos 32 caracteres. No incluyas secrets en el repositorio. Usa variables de entorno o `appsettings.Local.json` (ignorado por `.gitignore`).
>
> Para almacenamiento local, el directorio por defecto es `./storage` (ignorado por `.gitignore`).

## Migrations

Desde la raíz del repositorio:

```bash
# Crear una nueva migration (ejemplo)
dotnet ef migrations add NombreMigration --project src/TwentyNet.Persistence --startup-project src/TwentyNet.BFF

# Aplicar migrations a la base de datos
dotnet ef database update --project src/TwentyNet.Persistence --startup-project src/TwentyNet.BFF
```

La migración `AddAuth` añade las tablas y columnas necesarias para autenticación (usuarios, memberships, refresh tokens).

## Ejecutar

```bash
dotnet build
dotnet run --project src/TwentyNet.BFF
```

La API expone Swagger en: `https://localhost:7001/swagger` (puerto puede variar).

## Autenticación

La API usa **JWT Bearer**. Los endpoints de `Companies` y `People` requieren autenticación. El token de acceso se obtiene en `/api/auth/login` o `/api/auth/register`.

### Endpoints de autenticación

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/register` | Crear usuario, workspace y membership |
| POST | `/api/auth/login` | Iniciar sesión en un workspace |
| POST | `/api/auth/refresh` | Rotar tokens usando refresh token |
| POST | `/api/auth/logout` | Revocar refresh token |

### Flujo típico

1. Registro:

```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe",
    "workspaceName": "My Workspace"
  }'
```

2. Login (necesitas el `workspaceId` devuelto en el registro):

```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "workspaceId": "WORKSPACE_ID_AQUI"
  }'
```

3. Usar el `accessToken` en endpoints protegidos:

```bash
curl -X GET "https://localhost:7001/api/companies" \
  -H "Authorization: Bearer ACCESS_TOKEN_AQUI"
```

4. Refrescar token:

```bash
curl -X POST https://localhost:7001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{ "refreshToken": "REFRESH_TOKEN_AQUI" }'
```

5. Logout:

```bash
curl -X POST https://localhost:7001/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{ "refreshToken": "REFRESH_TOKEN_AQUI" }'
```

## Endpoints protegidos

### Companies

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies` | Listar empresas del workspace actual |
| GET | `/api/companies/{id}` | Obtener empresa por id |
| POST | `/api/companies` | Crear empresa |
| PUT | `/api/companies/{id}` | Actualizar empresa |
| DELETE | `/api/companies/{id}` | Eliminar empresa |

**Ejemplo POST /api/companies:**

```json
{
  "name": "Twenty CRM",
  "domainName": "twenty.com",
  "address": "123 Main St"
}
```

### People

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/people` | Listar personas del workspace actual |
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
  "companyId": null
}
```

### Files (manejo de archivos)

La API soporta dos backends de almacenamiento mediante `IStorageDriver`:

- **Local**: guarda archivos en disco bajo `Storage:LocalPath`.
- **S3**: usa `AWSSDK.S3`, compatible con AWS S3 y MinIO (configurando `S3Endpoint` y `S3ForcePathStyle=true`).

#### Configuración

Local (por defecto en `appsettings.json`):

```json
{
  "Storage": {
    "Provider": "Local",
    "LocalPath": "./storage"
  }
}
```

S3 / MinIO:

```json
{
  "Storage": {
    "Provider": "S3",
    "S3Bucket": "twentynet",
    "S3Region": "us-east-1",
    "S3AccessKey": "YOUR_ACCESS_KEY",
    "S3SecretKey": "YOUR_SECRET_KEY",
    "S3Endpoint": "https://minio.example.com",
    "S3ForcePathStyle": true
  }
}
```

> No incluyas credenciales S3 en el repositorio. Usa variables de entorno o `appsettings.Local.json`.

#### Endpoints de archivos

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/files` | Crear registro de archivo pendiente y obtener URL de upload |
| PUT | `/api/files/{id}/content` | Subir contenido del archivo (usado por el driver Local) |
| PATCH | `/api/files/{id}/complete` | Marcar archivo como `Uploaded` |
| GET | `/api/files/{id}/download` | Descargar archivo (redirecciona a URL presigned S3 o stream local) |
| DELETE | `/api/files/{id}` | Soft delete del archivo y eliminación del storage |

#### Adjuntos en People / Companies

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/people/{id}/attachments` | Asociar un archivo a una persona |
| GET | `/api/people/{id}/attachments` | Listar archivos adjuntos de una persona |
| POST | `/api/companies/{id}/attachments` | Asociar un archivo a una empresa |
| GET | `/api/companies/{id}/attachments` | Listar archivos adjuntos de una empresa |

#### Ejemplo de upload con driver Local

1. Crear el registro:

```bash
curl -X POST https://localhost:7001/api/files \
  -H "Authorization: Bearer ACCESS_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "report.pdf",
    "mimeType": "application/pdf",
    "size": 1024,
    "folder": "Attachment"
  }'
```

Respuesta:

```json
{
  "fileId": "FILE_ID_AQUI",
  "uploadUrl": "/api/files/FILE_ID_AQUI/content",
  "storageKey": "WORKSPACE_ID/attachment/FILE_ID-report.pdf"
}
```

2. Subir el contenido:

```bash
curl -X PUT "https://localhost:7001/api/files/FILE_ID_AQUI/content" \
  -H "Authorization: Bearer ACCESS_TOKEN_AQUI" \
  -F "file=@report.pdf"
```

3. Completar el upload:

```bash
curl -X PATCH "https://localhost:7001/api/files/FILE_ID_AQUI/complete" \
  -H "Authorization: Bearer ACCESS_TOKEN_AQUI"
```

4. Asociar a una persona:

```bash
curl -X POST "https://localhost:7001/api/people/PERSON_ID_AQUI/attachments" \
  -H "Authorization: Bearer ACCESS_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{ "fileId": "FILE_ID_AQUI" }'
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
- **Autenticación custom con JWT + BCrypt**: se evaluó ASP.NET Core Identity, pero dado que `User` ya era una entidad de dominio con `Email` como value object y se requería una relación muchos-a-muchos con workspaces, se optó por una implementación custom limpia que respeta la arquitectura de capas. Las abstracciones (`IAuthContext`, `ITokenService`, `IPasswordService`) viven en `Domain` y sus implementaciones en `BFF`.
- **Refresh token rotation**: cada uso de `/api/auth/refresh` revoca el token anterior y emite uno nuevo.

## Limitaciones conocidas

- AutoMapper tiene una advertencia de vulnerabilidad conocida (NU1903 / GHSA-rvv3-g6hj-g44x) en las versiones disponibles para .NET 8; se suprime el warning mientras se espera un patch o se evalúa reemplazo por mapeo manual.
- No hay seed automático de datos.
