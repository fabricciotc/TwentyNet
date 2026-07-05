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
- **UserWorkspaceMembership**: relación muchos-a-muchos entre usuarios y workspaces con rol (`Member` o `Admin`).
- **WorkspaceInvite**: invitación para unirse a un workspace con rol asignado, token único y expiración.
- **RefreshToken**: tokens de refresco rotatorios, revocables y con expiración.
- **Company**: empresa con nombre, dominio y dirección.
- **Person**: contacto con nombre, email, teléfono y relación opcional con una empresa.
- **File**: archivo con nombre, mime type, tamaño, carpeta (`Attachment`, `Avatar`, `EmailAttachment`), storage key, workspace y estado (`Pending`/`Uploaded`). Soporta soft delete y puede asociarse a `Person` o `Company` como attachment.
- **Webhook**: URL externa, secret, eventos suscritos y estado activo. Pertenece a un workspace.
- **ConnectedAccount**: cuenta de proveedor externo (`Google`, `Microsoft`, `Imap`) vinculada a un usuario y workspace. Almacena tokens cifrados.
- **MessageChannel** / **CalendarChannel**: canales de sincronización asociados a un `ConnectedAccount` (cimientos para futura sincronización de email/calendario).
- **View**: vista guardada por workspace y objeto (`Company` o `Person`) con filtros y ordenamientos.
- **ViewFilter**: filtro dinámico perteneciente a una `View` (`Field`, `Operator`, `Value`).
- **ViewSort**: ordenamiento dinámico perteneciente a una `View` (`Field`, `Direction`).
- **Note**: nota de texto asociada a un `Company` o `Person`, con título, contenido y autor.
- **TaskItem**: tarea con estado (`Todo`, `InProgress`, `Done`), asignación opcional a un usuario y fecha de vencimiento opcional, asociada a un `Company` o `Person`.
- **TimelineActivity**: actividad de timeline generada a partir de eventos de dominio (`RecordCreated`, `RecordUpdated`, `NoteCreated`, `TaskCreated`, `TaskCompleted`, `FileUploaded`) vinculada a un `Company` o `Person`.

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
  "HttpClient": {
    "EnrichmentBaseAddress": "https://api.example.com",
    "EnrichmentTimeoutSeconds": 30,
    "WebhookTimeoutSeconds": 30,
    "SsrfBlockPrivateNetworks": true
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
La migración `AddWorkspaceInvitesAndRoles` añade la entidad `WorkspaceInvite`, convierte el rol de membership a enum mapeado como string y actualiza los tokens JWT para incluir el claim `role`.
La migración `AddViewsFiltersSorts` añade las entidades `View`, `ViewFilter` y `ViewSort` para vistas, filtros dinámicos y ordenamiento.
La migración `AddNotesTasksTimeline` añade las entidades `Note`, `TaskItem` y `TimelineActivity`, junto con sus configuraciones, índices y la relación con `Company`, `Person`, `User` y `Workspace`.

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
| POST | `/api/auth/register` | Crear usuario, workspace y membership (rol `Admin`) |
| POST | `/api/auth/login` | Iniciar sesión en un workspace |
| POST | `/api/auth/refresh` | Rotar tokens usando refresh token |
| POST | `/api/auth/switch-workspace` | Cambiar al contexto de otro workspace del usuario |
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

5. Switch workspace (cambiar de workspace manteniendo la sesión):

```bash
curl -X POST https://localhost:7001/api/auth/switch-workspace \
  -H "Authorization: Bearer ACCESS_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{ "workspaceId": "OTRO_WORKSPACE_ID_AQUI" }'
```

6. Logout:

```bash
curl -X POST https://localhost:7001/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{ "refreshToken": "REFRESH_TOKEN_AQUI" }'
```

## Workspaces, members e invitaciones

Los endpoints de workspace requieren autenticación. Las operaciones administrativas (invitar, cambiar rol, remover) requieren el claim `role` == `Admin` en el JWT.

### Workspaces

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| GET | `/api/workspaces` | Listar workspaces a los que pertenece el usuario autenticado | `RequireMember` |

### Members

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| GET | `/api/workspaces/{id}/members` | Listar miembros del workspace | `RequireMember` |
| PUT | `/api/workspaces/{id}/members/{userId}/role` | Cambiar rol de un miembro | `RequireAdmin` |
| DELETE | `/api/workspaces/{id}/members/{userId}` | Remover miembro del workspace | `RequireAdmin` |

**Ejemplo PUT `/api/workspaces/{id}/members/{userId}/role`:**

```json
{
  "role": "Admin"
}
```

### Invites

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| POST | `/api/workspaces/{id}/invites` | Invitar a un usuario por email con un rol | `RequireAdmin` |
| POST | `/api/invites/{token}/accept` | Aceptar invitación | `RequireMember` |
| POST | `/api/invites/{token}/reject` | Rechazar invitación | `RequireMember` |

**Ejemplo POST `/api/workspaces/{id}/invites`:**

```json
{
  "email": "newuser@example.com",
  "role": "Member"
}
```

> Nota: en esta fase MVP no se envía el email real. El handler logea que se enviaría el correo y devuelve el token de invitación.

## Endpoints protegidos

### Views

Las vistas permiten guardar filtros y ordenamientos reutilizables para `Company` y `Person`.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/views?objectName=Company` | Listar vistas del workspace actual |
| GET | `/api/views/{id}` | Obtener vista por id |
| POST | `/api/views` | Crear vista |
| PUT | `/api/views/{id}` | Actualizar vista |
| DELETE | `/api/views/{id}` | Eliminar vista |

**Ejemplo POST /api/views:**

```json
{
  "objectName": "Company",
  "name": "Empresas Acme",
  "isDefault": false,
  "filters": [
    { "field": "Name", "operator": "Contains", "value": "Acme" },
    { "field": "DomainName", "operator": "IsNotEmpty" }
  ],
  "sorts": [
    { "field": "Name", "direction": "Asc" }
  ]
}
```

Operadores soportados: `Equals`, `Contains`, `GreaterThan`, `LessThan`, `IsEmpty`, `IsNotEmpty`.
Direcciones soportadas: `Asc`, `Desc`.

### Companies

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies?viewId=&search=&skip=0&take=50` | Listar empresas del workspace actual (paginado, búsqueda global y vista opcional) |
| POST | `/api/companies/search` | Buscar empresas con filtros y ordenamientos explícitos |
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

**Ejemplo POST /api/companies/search:**

```json
{
  "search": "twenty",
  "filters": [
    { "field": "DomainName", "operator": "IsNotEmpty" }
  ],
  "sorts": [
    { "field": "Name", "direction": "Asc" }
  ],
  "skip": 0,
  "take": 20
}
```

Campos de filtro/ordenamiento soportados para `Company`: `Name`, `DomainName`, `Address`.

### People

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/people?viewId=&search=&skip=0&take=50` | Listar personas del workspace actual (paginado, búsqueda global y vista opcional) |
| POST | `/api/people/search` | Buscar personas con filtros y ordenamientos explícitos |
| GET | `/api/people/{id}` | Obtener persona por id |
| POST | `/api/people` | Crear persona |
| PUT | `/api/people/{id}` | Actualizar persona |
| DELETE | `/api/people/{id}` | Eliminar persona |

Campos de filtro/ordenamiento soportados para `Person`: `FirstName`, `LastName`, `Email`, `Phone`.

### Notes

Las notas se crean siempre asociadas a un `Company` o a un `Person` del workspace actual.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies/{id}/notes` | Listar notas de una empresa |
| POST | `/api/companies/{id}/notes` | Crear nota en una empresa |
| GET | `/api/people/{id}/notes` | Listar notas de una persona |
| POST | `/api/people/{id}/notes` | Crear nota en una persona |
| GET | `/api/notes/{id}` | Obtener nota por id |
| PUT | `/api/notes/{id}` | Actualizar nota |
| DELETE | `/api/notes/{id}` | Eliminar nota |

**Ejemplo POST /api/companies/{id}/notes:**

```json
{
  "title": "Meeting notes",
  "content": "Discussed pricing and next steps"
}
```

### Tasks

Las tareas se crean siempre asociadas a un `Company` o a un `Person` del workspace actual. El estado puede ser `Todo`, `InProgress` o `Done`.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies/{id}/tasks` | Listar tareas de una empresa |
| POST | `/api/companies/{id}/tasks` | Crear tarea en una empresa |
| GET | `/api/people/{id}/tasks` | Listar tareas de una persona |
| POST | `/api/people/{id}/tasks` | Crear tarea en una persona |
| GET | `/api/tasks/{id}` | Obtener tarea por id |
| POST | `/api/tasks/{id}/complete` | Marcar tarea como completada |
| PUT | `/api/tasks/{id}` | Actualizar tarea |
| DELETE | `/api/tasks/{id}` | Eliminar tarea |

**Ejemplo POST /api/people/{id}/tasks:**

```json
{
  "title": "Send follow-up email",
  "assignedToUserId": "USER_ID_AQUI",
  "dueDate": "2026-07-10T17:00:00Z"
}
```

**Ejemplo PUT /api/tasks/{id}:**

```json
{
  "title": "Send follow-up email",
  "status": "InProgress",
  "assignedToUserId": "USER_ID_AQUI",
  "dueDate": "2026-07-10T17:00:00Z"
}
```

### Timeline

El timeline muestra las actividades registradas para un `Company` o `Person`, ordenadas de más reciente a más antigua. Las actividades se generan automáticamente a partir de eventos de dominio.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies/{id}/timeline` | Obtener timeline de una empresa |
| GET | `/api/people/{id}/timeline` | Obtener timeline de una persona |

**Tipos de actividad posibles:**

- `RecordCreated`
- `RecordUpdated`
- `NoteCreated`
- `TaskCreated`
- `TaskCompleted`
- `FileUploaded`

## Realtime (SignalR)

El BFF expone un hub de SignalR en:

```
/hubs/workspace
```

### Autenticación

SignalR usa el mismo JWT Bearer que la API HTTP. El token debe enviarse en el query string como `access_token` al conectar:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7001/hubs/workspace?access_token=ACCESS_TOKEN_AQUI")
    .build();

await connection.start();
```

> El token también se lee automáticamente del header `Authorization` si el transporte lo permite.

### Eventos del servidor

Una vez conectado, el cliente recibe eventos del workspace al que pertenece el token (claim `workspace_id`):

| Evento | Payload | Descripción |
|--------|---------|-------------|
| `ObjectRecordChanged` | `{ objectName: "Company" \| "Person", recordId: Guid, changeType: "created" \| "updated" \| "deleted" }` | Notifica cambios en registros de Company o Person del workspace actual |

**Ejemplo de manejo en JavaScript:**

```javascript
connection.on("ObjectRecordChanged", (objectName, recordId, changeType) => {
    console.log(`${objectName} ${recordId} was ${changeType}`);
});
```

### Implementación

- El backend emite eventos de dominio (`ObjectRecordCreatedEvent`, `ObjectRecordUpdatedEvent`, `ObjectRecordDeletedEvent`) desde los handlers de Company y Person, y los eventos `NoteCreatedEvent`, `TaskCreatedEvent` y `TaskCompletedEvent` desde los handlers de Notes y Tasks.
- `SignalRRealTimeNotifier` traduce esos eventos a llamadas de grupo SignalR (`workspace:{workspaceId}`).
- Los clientes se unen automáticamente al grupo de su workspace en `OnConnectedAsync` y se remueven en `OnDisconnectedAsync`.

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

### Webhooks

Los webhooks salientes notifican a URLs externas cuando ocurren cambios en registros de `Company` y `Person` dentro del workspace actual.

#### Eventos soportados

| Evento | Descripción |
|--------|-------------|
| `company.created` | Nueva empresa creada |
| `company.updated` | Empresa actualizada |
| `company.deleted` | Empresa eliminada |
| `person.created` | Nueva persona creada |
| `person.updated` | Persona actualizada |
| `person.deleted` | Persona eliminada |

#### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/webhooks` | Listar webhooks del workspace |
| GET | `/api/webhooks/{id}` | Obtener webhook por id |
| POST | `/api/webhooks` | Crear webhook |
| PUT | `/api/webhooks/{id}` | Actualizar webhook |
| DELETE | `/api/webhooks/{id}` | Eliminar webhook |

**Ejemplo POST /api/webhooks:**

```json
{
  "targetUrl": "https://example.com/webhook",
  "secret": "mi-webhook-secret",
  "events": ["company.created", "company.updated", "person.created"]
}
```

#### Formato de entrega

Cada entrega es un POST con `Content-Type: application/json` y los siguientes headers:

| Header | Valor |
|--------|-------|
| `X-Twenty-Webhook-Event` | Nombre del evento, ej. `company.created` |
| `X-Twenty-Webhook-Signature` | `sha256=<hex>` con HMAC-SHA256 del cuerpo JSON usando el `secret` del webhook |

**Ejemplo de cuerpo:**

```json
{
  "event": "company.created",
  "workspaceId": "WORKSPACE_ID",
  "objectName": "Company",
  "recordId": "RECORD_ID",
  "timestamp": "2026-07-05T17:46:00Z",
  "data": {
    "recordId": "RECORD_ID"
  }
}
```

#### Seguridad SSRF

El envío de webhooks usa `SecureHttpClient`, que resuelve el host de `targetUrl` a direcciones IP y bloquea rangos privados cuando `HttpClient:SsrfBlockPrivateNetworks` es `true` (por defecto). Se bloquean `localhost`, `127.0.0.0/8`, `10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`, `169.254.0.0/16`, `::1` y `fc00::/7`.

### Connected Accounts (cimientos de conectores)

Los connected accounts almacenan credenciales de proveedores externos (`Google`, `Microsoft`, `Imap`) para futura sincronización de mensajería y calendarios. Los tokens (`AccessToken` y `RefreshToken`) se cifran en reposo usando `IDataProtectionProvider` con el propósito `"ConnectedAccountTokens"`.

#### Proveedores soportados

| Valor | Descripción |
|-------|-------------|
| `Google` | Google (Gmail / Google Calendar) |
| `Microsoft` | Microsoft (Outlook / Outlook Calendar) |
| `Imap` | IMAP genérico |

#### Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/connected-accounts` | Listar cuentas del workspace |
| GET | `/api/connected-accounts/{id}` | Obtener cuenta por id |
| POST | `/api/connected-accounts` | Crear cuenta conectada |
| DELETE | `/api/connected-accounts/{id}` | Eliminar cuenta conectada |

**Ejemplo POST /api/connected-accounts:**

```json
{
  "provider": "Google",
  "email": "user@example.com",
  "accessToken": "ACCESS_TOKEN",
  "refreshToken": "REFRESH_TOKEN",
  "expiresAt": "2026-07-05T18:00:00Z"
}
```

#### Entidades relacionadas (no expuestas aún por API)

- `MessageChannel`: canal de sincronización de mensajes (`Gmail`, `Outlook`, `Imap`).
- `CalendarChannel`: canal de sincronización de calendarios (`GoogleCalendar`, `OutlookCalendar`, `CalDav`).

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
