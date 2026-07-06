# TwentyNet

Reescritura progresiva del backend de [Twenty CRM](https://github.com/twentyhq/twenty) en ASP.NET Core 8 con EF Core, MediatR, SignalR y arquitectura limpia.

## Stack

- **.NET 8** / **C# 12**
- **ASP.NET Core Web API** con controllers (no Minimal APIs)
- **Entity Framework Core 8** con **PostgreSQL** (Npgsql)
- **MediatR 12** para commands y queries
- **FluentValidation** para validación de inputs
- **AutoMapper** para mapeo entre entidades, DTOs y contracts
- **SignalR** para notificaciones en tiempo real
- **Hot Chocolate 13** para adaptador GraphQL que emula el schema de Twenty
- **xUnit + NSubstitute + EF Core InMemory** para tests
- **JWT Bearer** + **BCrypt** para autenticación custom
- **S3 / Local** storage de archivos mediante `IStorageDriver`

## Estructura

```
TwentyNet.slnx
├── src/
│   ├── TwentyNet.Domain/          # Entidades, value objects, eventos, interfaces de repositorio
│   ├── TwentyNet.Contracts/       # Request/response contracts compartidos
│   ├── TwentyNet.Application/     # Comandos, queries, handlers, DTOs, validadores, perfiles AutoMapper
│   ├── TwentyNet.Persistence/     # EF Core DbContext, configuraciones, repositorios, migrations
│   └── TwentyNet.BFF/             # ASP.NET Core Web API, controllers, DI, Swagger, SignalR, SPA
│       └── Frontend/              # Aplicación SPA estática servida por el BFF
└── tests/
    └── TwentyNet.Application.Tests/  # Tests de handlers y validadores
```

## Dominio

- **Workspace**: contenedor principal de datos.
- **User**: usuario con email, nombre, apellido, hash de contraseña y estado.
- **UserWorkspaceMembership**: relación muchos-a-muchos entre usuarios y workspaces con rol (`Member` o `Admin`).
- **WorkspaceInvite**: invitación para unirse a un workspace con rol asignado, token único y expiración.
- **RefreshToken**: tokens de refresco rotatorios, revocables y con expiración.
- **Company**: empresa con nombre, dominio, dirección y campos personalizados.
- **Person**: contacto con nombre, email, teléfono, relación opcional con una empresa y campos personalizados.
- **File**: archivo con nombre, mime type, tamaño, carpeta (`Attachment`, `Avatar`, `EmailAttachment`), storage key, workspace y estado (`Pending`/`Uploaded`). Soporta soft delete y puede asociarse a `Person` o `Company` como attachment.
- **Webhook**: URL externa, secret, eventos suscritos y estado activo. Pertenece a un workspace.
- **ConnectedAccount**: cuenta de proveedor externo (`Google`, `Microsoft`, `Imap`) vinculada a un usuario y workspace. Almacena tokens cifrados.
- **MessageChannel** / **CalendarChannel**: canales de sincronización asociados a un `ConnectedAccount` para email y calendario.
- **View**: vista guardada por workspace y objeto (`Company` o `Person`) con filtros y ordenamientos.
- **ViewFilter**: filtro dinámico perteneciente a una `View` (`Field`, `Operator`, `Value`).
- **ViewSort**: ordenamiento dinámico perteneciente a una `View` (`Field`, `Direction`).
- **Note**: nota de texto asociada a un `Company` o `Person`, con título, contenido y autor.
- **TaskItem**: tarea con estado (`Todo`, `InProgress`, `Done`), asignación opcional a un usuario y fecha de vencimiento opcional, asociada a un `Company` o `Person`.
- **TimelineActivity**: actividad de timeline generada a partir de eventos de dominio (`RecordCreated`, `RecordUpdated`, `NoteCreated`, `TaskCreated`, `TaskCompleted`, `FileUploaded`) vinculada a un `Company` o `Person`.
- **CustomFieldDefinition**: definición de campo personalizado por workspace y objeto (`Company`, `Person`).
- **RecordRelation**: relación entre dos registros de dominio (`Company`↔`Company`, `Company`↔`Person`, etc.).
- **ApiKey**: clave de API por workspace con nombre, rol, scopes, expiración y revocación.
- **Workflow**: flujo de automatización con trigger (`RecordCreated`, `RecordUpdated`, `FieldChanged`) y acciones (`SendWebhook`, `CreateTask`, `SendNotification`).
- **SsoProvider**: proveedor de autenticación externo (`Saml`, `OAuth2`) por workspace para SSO/SAML.
- **ChatSession** / **ChatMessage**: sesiones y mensajes del chatbot de IA del workspace.
- **SubscriptionPlan** / **WorkspaceSubscription** / **Invoice**: planes, suscripción activa y facturación por workspace.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) 14+ (el `docker-compose.yml` incluye uno listo para usar)
- `dotnet-ef` CLI (opcional, para migrations):

```bash
dotnet tool install --global dotnet-ef
```

## Configuración local

Edita `src/TwentyNet.BFF/appsettings.json` (o `appsettings.Development.json`) con tus valores:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=twentynet;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Issuer": "TwentyNet",
    "Audience": "TwentyNet",
    "SecretKey": "your-super-secret-key-min-32-chars!",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Storage": {
    "Provider": "Local",
    "LocalPath": "./storage"
  },
  "EnrichmentService": {
    "BaseAddress": "https://api.example.com",
    "TimeoutSeconds": 30
  },
  "WebhookService": {
    "TimeoutSeconds": 30,
    "SsrfBlockPrivateNetworks": true
  },
  "AiChatbot": {
    "Provider": "Stub",
    "ApiKey": "",
    "Model": "gpt-4o-mini",
    "BaseAddress": "https://api.openai.com/v1/",
    "TimeoutSeconds": 60
  }
}
```

> **Importante:** cambia `Jwt:SecretKey` en producción por un valor aleatorio de al menos 32 caracteres. No incluyas secrets en el repositorio. Usa variables de entorno o `appsettings.Local.json` (ignorado por `.gitignore`).
>
> Para almacenamiento local, el directorio por defecto es `./storage` (ignorado por `.gitignore`).
>
> Para S3 / MinIO, configura `Storage:Provider` = `S3` y completa `S3Bucket`, `S3Region`, `S3AccessKey`, `S3SecretKey`, `S3Endpoint` y `S3ForcePathStyle`.
>
> Cada servicio externo tiene su propia sección de `IOptions<T>` (`EnrichmentService`, `WebhookService`, `AiChatbot`, etc.). No agrupes múltiples servicios bajo una sola sección `HttpClient`.

## Docker Compose

El repositorio incluye `docker-compose.yml` con PostgreSQL, Redis y el BFF:

```bash
docker compose up --build -d
```

- BFF: `http://localhost:5000`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

El BFF sirve automáticamente el SPA ubicado en `src/TwentyNet.BFF/Frontend/index.html`.

## Migrations

Desde la raíz del repositorio:

```bash
# Crear una nueva migration (ejemplo)
dotnet ef migrations add NombreMigration --project src/TwentyNet.Persistence --startup-project src/TwentyNet.BFF

# Aplicar migrations a la base de datos
dotnet ef database update --project src/TwentyNet.Persistence --startup-project src/TwentyNet.BFF
```

## Ejecutar

```bash
dotnet build
dotnet run --project src/TwentyNet.BFF
```

La API expone Swagger en: `https://localhost:7001/swagger` (puerto puede variar).  
El endpoint GraphQL está en `/graphql` con Banana Cake Pop en `/graphql/ui`.  
El SPA se sirve en la raíz del mismo origen gracias a `MapFallbackToFile("index.html")`.

## GraphQL

El BFF expone un adaptador GraphQL con **Hot Chocolate** que emula el schema público de Twenty CRM. Los resolvers delegan en los mismos commands/queries de MediatR que usa la API REST, por lo que la lógica de negocio no se duplica.

### Endpoint

- URL: `https://localhost:7001/graphql`
- IDE/Banana Cake Pop: `https://localhost:7001/graphql/ui`

### Queries principales

| Query | Descripción |
|-------|-------------|
| `companies` | Listar empresas del workspace actual |
| `company(id: UUID!)` | Obtener empresa por id |
| `people` | Listar personas del workspace actual |
| `person(id: UUID!)` | Obtener persona por id |
| `views(objectName: String!)` | Listar vistas de un objeto (`Company` o `Person`) |

### Mutations principales

| Mutation | Descripción |
|----------|-------------|
| `login(input: LoginInput!)` | Autenticar usuario y obtener JWT |
| `createCompany(input: CreateCompanyInput!)` | Crear empresa |
| `updateCompany(id: UUID!, input: UpdateCompanyInput!)` | Actualizar empresa |
| `deleteCompany(id: UUID!)` | Eliminar empresa |
| `createPerson(input: CreatePersonInput!)` | Crear persona |
| `updatePerson(id: UUID!, input: UpdatePersonInput!)` | Actualizar persona |
| `deletePerson(id: UUID!)` | Eliminar persona |

### Autenticación en GraphQL

Envía el token JWT en el header `Authorization: Bearer ACCESS_TOKEN_AQUI`. El middleware de Hot Chocolate valida el token y expone `IAuthContext` a los resolvers. La mutation `login` es pública.

### Ejemplo

```graphql
query {
  companies {
    id
    name
    domain
    createdAt
  }
}
```

```graphql
mutation {
  login(input: { email: "user@example.com", password: "Password123!", workspaceId: "WORKSPACE_ID" }) {
    accessToken
    refreshToken
    user { id email }
    workspace { id name }
  }
}
```

> El schema GraphQL es un subconjunto compatible del schema de Twenty CRM. Se irá extendiendo a medida que se migren más funciones.

## Autenticación

La API usa **JWT Bearer**. Los endpoints protegidos requieren autenticación. El token de acceso se obtiene en `/api/auth/login` o `/api/auth/register`.

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

## Workspaces, members e invitaciones

Las operaciones administrativas (invitar, cambiar rol, remover) requieren el claim `role` == `Admin` en el JWT.

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

### Invites

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| POST | `/api/workspaces/{id}/invites` | Invitar a un usuario por email con un rol | `RequireAdmin` |
| POST | `/api/invites/{token}/accept` | Aceptar invitación | `RequireMember` |
| POST | `/api/invites/{token}/reject` | Rechazar invitación | `RequireMember` |

> Nota: en esta fase MVP no se envía el email real. El handler logea que se enviaría el correo y devuelve el token de invitación.

## Endpoints protegidos

### Views

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/views?objectName=Company` | Listar vistas del workspace actual |
| GET | `/api/views/{id}` | Obtener vista por id |
| POST | `/api/views` | Crear vista |
| PUT | `/api/views/{id}` | Actualizar vista |
| DELETE | `/api/views/{id}` | Eliminar vista |

Operadores soportados: `Equals`, `Contains`, `GreaterThan`, `LessThan`, `IsEmpty`, `IsNotEmpty`.

### Companies

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies?viewId=&search=&skip=0&take=50` | Listar empresas del workspace actual |
| POST | `/api/companies/search` | Buscar empresas con filtros y ordenamientos explícitos |
| GET | `/api/companies/{id}` | Obtener empresa por id |
| POST | `/api/companies` | Crear empresa |
| PUT | `/api/companies/{id}` | Actualizar empresa |
| DELETE | `/api/companies/{id}` | Eliminar empresa |
| POST | `/api/companies/{id}/attachments` | Asociar un archivo a una empresa |
| GET | `/api/companies/{id}/attachments` | Listar archivos adjuntos de una empresa |
| PUT | `/api/companies/{id}/custom-fields` | Actualizar campos personalizados de una empresa |
| POST | `/api/companies/import` | Importar empresas desde CSV |
| GET | `/api/companies/export` | Exportar empresas a CSV |

### People

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/people?viewId=&search=&skip=0&take=50` | Listar personas del workspace actual |
| POST | `/api/people/search` | Buscar personas con filtros y ordenamientos explícitos |
| GET | `/api/people/{id}` | Obtener persona por id |
| POST | `/api/people` | Crear persona |
| PUT | `/api/people/{id}` | Actualizar persona |
| DELETE | `/api/people/{id}` | Eliminar persona |
| POST | `/api/people/{id}/attachments` | Asociar un archivo a una persona |
| GET | `/api/people/{id}/attachments` | Listar archivos adjuntos de una persona |
| PUT | `/api/people/{id}/custom-fields` | Actualizar campos personalizados de una persona |
| POST | `/api/people/import` | Importar personas desde CSV |
| GET | `/api/people/export` | Exportar personas a CSV |

### Notes

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies/{id}/notes` | Listar notas de una empresa |
| POST | `/api/companies/{id}/notes` | Crear nota en una empresa |
| GET | `/api/people/{id}/notes` | Listar notas de una persona |
| POST | `/api/people/{id}/notes` | Crear nota en una persona |
| GET | `/api/notes/{id}` | Obtener nota por id |
| PUT | `/api/notes/{id}` | Actualizar nota |
| DELETE | `/api/notes/{id}` | Eliminar nota |

### Tasks

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

### Timeline

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/companies/{id}/timeline` | Obtener timeline de una empresa |
| GET | `/api/people/{id}/timeline` | Obtener timeline de una persona |

### Custom Fields

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| GET | `/api/custom-fields?objectName=Company` | Listar definiciones de campos personalizados | `RequireMember` |
| POST | `/api/custom-fields` | Crear definición de campo personalizado | `RequireAdmin` |
| PUT | `/api/custom-fields/{id}` | Actualizar definición de campo personalizado | `RequireAdmin` |
| DELETE | `/api/custom-fields/{id}` | Eliminar definición de campo personalizado | `RequireAdmin` |

Tipos soportados: `Text`, `Number`, `Date`, `Boolean`, `Select`, `MultiSelect`.

### Record Relations

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/relations?objectName=Company&recordId=...` | Listar relaciones de un registro |
| POST | `/api/relations` | Crear relación entre registros |
| DELETE | `/api/relations/{id}` | Eliminar relación |

### Files

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/files` | Crear registro de archivo pendiente y obtener URL de upload |
| PUT | `/api/files/{id}/content` | Subir contenido del archivo (usado por el driver Local) |
| PATCH | `/api/files/{id}/complete` | Marcar archivo como `Uploaded` |
| GET | `/api/files/{id}/download` | Descargar archivo (redirecciona a URL presigned S3 o stream local) |
| DELETE | `/api/files/{id}` | Soft delete del archivo y eliminación del storage |

### Webhooks

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/webhooks` | Listar webhooks del workspace |
| GET | `/api/webhooks/{id}` | Obtener webhook por id |
| POST | `/api/webhooks` | Crear webhook |
| PUT | `/api/webhooks/{id}` | Actualizar webhook |
| DELETE | `/api/webhooks/{id}` | Eliminar webhook |

Eventos soportados: `company.created`, `company.updated`, `company.deleted`, `person.created`, `person.updated`, `person.deleted`.

El envío de webhooks usa `SecureHttpClient`, que resuelve el host de `targetUrl` a direcciones IP y bloquea rangos privados cuando `WebhookService:SsrfBlockPrivateNetworks` es `true`.

### Connected Accounts (Email / Calendar sync)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/connected-accounts` | Listar cuentas del workspace |
| GET | `/api/connected-accounts/{id}` | Obtener cuenta por id |
| POST | `/api/connected-accounts` | Crear cuenta conectada |
| DELETE | `/api/connected-accounts/{id}` | Eliminar cuenta conectada |

Proveedores soportados: `Google`, `Microsoft`, `Imap`. Los tokens se cifran en reposo con `IDataProtectionProvider`.

### Realtime (SignalR)

El BFF expone un hub de SignalR en `/hubs/workspace`. Los clientes se unen automáticamente al grupo de su workspace y reciben eventos como `ObjectRecordChanged`.

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7001/hubs/workspace?access_token=ACCESS_TOKEN_AQUI")
    .build();

connection.on("ObjectRecordChanged", (objectName, recordId, changeType) => {
    console.log(`${objectName} ${recordId} was ${changeType}`);
});

await connection.start();
```

### API Keys

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| GET | `/api/api-keys` | Listar API keys del workspace | `RequireAdmin` |
| GET | `/api/api-keys/{id}` | Obtener API key por id | `RequireAdmin` |
| POST | `/api/api-keys` | Generar nueva API key | `RequireAdmin` |
| PUT | `/api/api-keys/{id}/revoke` | Revocar API key | `RequireAdmin` |
| DELETE | `/api/api-keys/{id}` | Eliminar API key | `RequireAdmin` |

### Workflows

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/workflows` | Listar workflows del workspace |
| GET | `/api/workflows/{id}` | Obtener workflow por id |
| POST | `/api/workflows` | Crear workflow |
| PUT | `/api/workflows/{id}` | Actualizar workflow |
| DELETE | `/api/workflows/{id}` | Eliminar workflow |

Triggers soportados: `RecordCreated`, `RecordUpdated`, `FieldChanged`.  
Acciones soportadas: `SendWebhook`, `CreateTask`, `SendNotification`.

### SSO / SAML

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| GET | `/api/sso/providers` | Listar proveedores SSO | `RequireAdmin` |
| GET | `/api/sso/providers/{id}` | Obtener proveedor SSO | `RequireAdmin` |
| POST | `/api/sso/providers` | Crear proveedor SSO (SAML u OAuth2) | `RequireAdmin` |
| PUT | `/api/sso/providers/{id}` | Actualizar proveedor SSO | `RequireAdmin` |
| DELETE | `/api/sso/providers/{id}` | Eliminar proveedor SSO | `RequireAdmin` |
| GET | `/api/sso/saml/{providerId}/login` | Iniciar login SAML (redirige al IdP) | Público |
| POST | `/api/sso/saml/{providerId}/acs` | ACS SAML, provisiona usuario y devuelve JWT | Público |

### AI Chatbot

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/chatbot/sessions` | Listar sesiones del usuario |
| POST | `/api/chatbot/sessions` | Crear sesión de chat |
| GET | `/api/chatbot/sessions/{sessionId}/messages` | Listar mensajes de una sesión |
| POST | `/api/chatbot/sessions/{sessionId}/messages` | Enviar mensaje al chatbot |

Configura `AiChatbot:Provider` como `OpenAi` y provee `AiChatbot:ApiKey` para usar la API de OpenAI. El modo `Stub` devuelve respuestas locales para desarrollo y tests.

### Billing

| Método | Ruta | Descripción | Política |
|--------|------|-------------|----------|
| GET | `/api/billing/plans` | Listar planes de suscripción | Público |
| GET | `/api/billing/subscription` | Obtener suscripción del workspace | `RequireAdmin` |
| POST | `/api/billing/subscribe` | Suscribir workspace a un plan | `RequireAdmin` |
| POST | `/api/billing/subscription/{id}/cancel` | Cancelar suscripción | `RequireAdmin` |
| GET | `/api/billing/invoices` | Listar facturas del workspace | `RequireAdmin` |
| POST | `/api/billing/invoices` | Generar factura manual (testing) | `RequireAdmin` |

> Billing usa un stub por defecto. Para integrar con un proveedor real (Stripe, etc.), implementa `IBillingService` y registra tu proveedor en DI.

## Tests

```bash
dotnet test
```

## Decisiones técnicas

- **Arquitectura de capas**: Domain → Contracts → Application → Persistence → BFF. El BFF es la única capa que conoce ASP.NET Core y la infraestructura externa.
- **Repositorio genérico (`EfRepository<T>`)**: cubre CRUD básico y delega queries específicas a los handlers.
- **Unit of work**: `AppDbContext` implementa `IUnitOfWork`; los handlers usan `SaveChangesAsync` del repositorio.
- **Value objects**: `Email` y `PhoneNumber` encapsulan validación básica.
- **Patrón IOptions por servicio externo**: cada integración externa (`EnrichmentService`, `WebhookService`, `AiChatbot`, etc.) tiene su propia sección de configuración tipada y su `HttpClient` nombrado cuando aplica.
- **HttpClient nombrado**: `EnrichmentClient` y `WebhookClient` se configuran desde sus respectivas opciones (`IOptions<EnrichmentServiceOptions>`, `IOptions<WebhookServiceOptions>`).
- **Autenticación custom con JWT + BCrypt**: se optó por una implementación limpia que respeta la arquitectura de capas. Las abstracciones (`IAuthContext`, `ITokenService`, `IPasswordService`) viven en `Domain` y sus implementaciones en `BFF`.
- **Refresh token rotation**: cada uso de `/api/auth/refresh` revoca el token anterior y emite uno nuevo.
- **SignalR**: notificaciones en tiempo real por workspace mediante grupos (`workspace:{id}`).
- **SPA servido por el BFF**: los archivos estáticos de `src/TwentyNet.BFF/Frontend` se sirven con `StaticFileOptions` apuntando a esa carpeta y el fallback resuelve `index.html`.
- **Adaptador GraphQL con Hot Chocolate**: el schema expuesto por el BFF emula el de Twenty CRM y delega a MediatR, manteniendo la lógica de negocio en `Application` y conviviendo con los controllers REST existentes.

## Limitaciones conocidas

- AutoMapper tiene una advertencia de vulnerabilidad conocida (NU1903 / GHSA-rvv3-g6hj-g44x) en las versiones disponibles para .NET 8; se suprime el warning mientras se espera un patch o se evalúa reemplazo por mapeo manual.
- No hay seed automático de datos.
- Billing y AI chatbot usan implementaciones stub por defecto; se proveen abstracciones listas para integrar proveedores reales.
- El build del frontend real de Twenty CRM (`twenty-front`) no pudo completarse en este entorno local (proceso de `twenty-ui:build` se atascó sin actividad de CPU). El BFF continúa sirviendo el SPA estático de desarrollo ubicado en `src/TwentyNet.BFF/Frontend/`. Para usar el UI oficial de Twenty hay que compilarlo externamente y copiar los artefactos a esa carpeta.
