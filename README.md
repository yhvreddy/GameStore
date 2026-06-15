# GameStore Ecommerce API

GameStore is an ASP.NET Core Web API for a small ecommerce-style game store. It supports public catalog browsing, JWT authentication, role data, cart management, checkout, order history, and database-backed application logs.

The project now uses an MVC-style controller structure with repository interfaces and implementations, plus one consistent API response envelope for success, validation, authentication, and application errors.

## Tech Stack

- .NET 10
- ASP.NET Core controllers
- Entity Framework Core
- SQLite
- JWT bearer authentication
- Swagger / OpenAPI with Swashbuckle
- Data annotations validation

## Project Structure

```text
GameStore/
  Controllers/       HTTP endpoints and API responses
  Data/              DbContext, migrations, seeders, database startup
  Dtos/              Request DTOs, response DTOs, ApiResponse<T>
  Extensions/        Shared framework/helper extensions
  Interfaces/        Repository and service contracts
  Mapping/           Entity-to-DTO mapping extensions
  Models/            EF Core entity models
  Repositories/      EF Core data access implementations
  Services/          Password hashing, JWT token creation, logging implementation
  Swagger/           Swagger auth document filter
  Program.cs         Application startup and middleware registration
  *.http             Local API request samples
```

## API Response Format

All application responses use `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": {},
  "errors": [],
  "timestamp": "2026-06-15T00:00:00Z"
}
```

Failure responses use the same shape:

```json
{
  "success": false,
  "message": "Validation failed.",
  "data": null,
  "errors": [
    "The Name field is required."
  ],
  "timestamp": "2026-06-15T00:00:00Z"
}
```

Notes:

- `data` contains the actual DTO for successful requests.
- `errors` contains validation or failure details.
- Delete and logout endpoints return `200 OK` with `data: null`, rather than `204 NoContent`, so the response format stays consistent.
- JWT `401 Unauthorized` and `403 Forbidden` middleware responses are also wrapped.

## Main Features

- User registration, login, token-revoking logout, and current-user profile.
- JWT-protected endpoints with Swagger Authorize support.
- Role table with seeded `Admin` and `Customer` roles.
- Public game, genre, and role browsing.
- Authenticated game, genre, and role management.
- Authenticated cart add/update/remove/clear workflow.
- Authenticated checkout that converts cart items into an order.
- Authenticated order history for the current user.
- Database-backed logging through `ILogService` and `LogService`.
- Public log endpoint for client or application events.
- Write-operation logging for users, games, genres, roles, cart, and checkout.

## Getting Started

From the repository root:

```powershell
dotnet restore GameStore/GameStore.csproj
dotnet build GameStore/GameStore.csproj
dotnet run --project GameStore/GameStore.csproj --launch-profile http
```

Default HTTP URL:

```text
http://localhost:5137
```

Swagger UI in Development:

```text
http://localhost:5137/swagger
```

Health check:

```text
http://localhost:5137/health
```

Expected response:

```json
{
  "success": true,
  "message": "Health check completed.",
  "data": {
    "status": "Healthy"
  },
  "errors": [],
  "timestamp": "2026-06-15T00:00:00Z"
}
```

## Database

The app uses SQLite with this default connection string:

```json
"GameStore": "Data Source=GameStore.db"
```

Pending migrations run automatically at startup through:

```csharp
app.MigrationDb();
```

Seed data is organized under `GameStore/Data/Seeders/`:

- `RoleSeeder` creates `Admin` and `Customer`.
- `GenreSeeder` creates default game genres.
- `GameSeeder` creates default game catalog records.
- `DatabaseSeeder` runs all seeders from one place.

Create a migration from the repository root:

```powershell
dotnet ef migrations add MigrationName --project GameStore --output-dir Data/Migrations
```

Apply migrations from the repository root:

```powershell
dotnet ef database update --project GameStore
```

## Authentication

Login returns a wrapped JWT response:

```http
POST http://localhost:5137/users/login
Content-Type: application/json

{
  "email": "admin@gamstore.com",
  "password": "admin@123!"
}
```

Example shape:

```json
{
  "success": true,
  "message": "User logged in successfully.",
  "data": {
    "token": "jwt-token-value",
    "expiresAt": "2026-06-15T12:00:00Z",
    "user": {
      "id": 1,
      "fullName": "Admin User",
      "email": "admin@gamstore.com",
      "roleId": 1
    }
  },
  "errors": [],
  "timestamp": "2026-06-15T10:00:00Z"
}
```

In Swagger:

1. Call `/users/login`.
2. Copy only `data.token`.
3. Click `Authorize`.
4. Paste the token only, without typing `Bearer`.

The JWT includes user id, name, email, role, and token ID claims. Logout stores the token ID in `RevokedTokens`, so the same token is rejected on future requests.

## Endpoint Summary

Users:

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/users` | Admin | List users |
| POST | `/users/register` | Public | Register user |
| POST | `/users/login` | Public | Login and receive token |
| GET | `/users/me` | User | Get current user |
| POST | `/users/logout` | User | Revoke current JWT |

Roles:

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/roles` | Public | List roles |
| GET | `/roles/{id}` | Public | Get role by ID |
| POST | `/roles` | User | Create role |
| PUT | `/roles/{id}` | User | Update role |
| DELETE | `/roles/{id}` | User | Delete role |

Games:

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/games` | Public | List games |
| GET | `/games/{id}` | Public | Get game by ID |
| POST | `/games` | User | Create game |
| PUT | `/games/{id}` | User | Update game |
| DELETE | `/games/{id}` | User | Delete game |

Genres:

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/genres` | Public | List genres |
| GET | `/genres/{id}` | Public | Get genre by ID |
| POST | `/genres` | User | Create genre |
| PUT | `/genres/{id}` | User | Update genre |
| DELETE | `/genres/{id}` | User | Delete genre |

Cart and orders:

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/cart` | User | View current user's cart |
| POST | `/cart/items` | User | Add a game to cart |
| PUT | `/cart/items/{gameId}` | User | Update cart item quantity |
| DELETE | `/cart/items/{gameId}` | User | Remove one cart item |
| DELETE | `/cart` | User | Clear cart |
| POST | `/checkout` | User | Create order from cart |
| GET | `/orders/my` | User | List current user's orders |
| GET | `/orders/my/{id}` | User | Get current user's order |

Logs:

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/logs` | Public | List application log entries |
| POST | `/logs` | Public | Create an application log entry |

## Request Samples

Register:

```http
POST http://localhost:5137/users/register
Content-Type: application/json

{
  "fullName": "Harsha Yenumula",
  "email": "customer@example.com",
  "password": "admin@123!",
  "roleId": 2
}
```

Add to cart:

```http
POST http://localhost:5137/cart/items
Authorization: Bearer your-token-here
Content-Type: application/json

{
  "gameId": 1,
  "quantity": 2
}
```

Checkout:

```http
POST http://localhost:5137/checkout
Authorization: Bearer your-token-here
```

Create log:

```http
POST http://localhost:5137/logs
Content-Type: application/json

{
  "level": "Information",
  "message": "Checkout page opened",
  "source": "Frontend",
  "userId": null,
  "exception": null
}
```

More samples are available in:

- `GameStore/users.http`
- `GameStore/games.http`
- `GameStore/orders.http`

## Docker Deployment

The repository includes a Docker setup for local deployment and server deployment.

Files:

- `Dockerfile` builds and publishes the ASP.NET Core API.
- `docker-compose.yml` runs the API on port `5137`.
- `.env.example` shows required environment variables.
- `.dockerignore` keeps build output, local DB files, and editor files out of the Docker build context.

First-time setup:

```powershell
copy .env.example .env
```

Update `.env` with a strong JWT secret:

```text
JWT_SECRET_KEY=your-very-long-random-secret-key-here
```

Build and start:

```powershell
docker compose up --build -d
```

Verify:

```powershell
curl http://localhost:5137/health
```

View logs:

```powershell
docker compose logs -f gamestore-api
```

Stop:

```powershell
docker compose down
```

Reset Docker database volume:

```powershell
docker compose down -v
docker compose up --build -d
```

Swagger is enabled only in `Development`. For local Docker testing, set this in `docker-compose.yml`:

```yaml
ASPNETCORE_ENVIRONMENT: Development
```

## CI Pipeline

GitHub Actions workflow:

```text
.github/workflows/ci.yml
```

The workflow restores NuGet packages, builds the API in Release mode, and builds the Docker image as `gamestore-api:ci`.

## Audit Notes

Completed audit updates:

- Replaced minimal API endpoint files with MVC-style controllers.
- Added repository interfaces in `Interfaces/` and EF implementations in `Repositories/`.
- Moved all service/repository contracts, including `ILogService`, into `Interfaces/`.
- Added `ApiResponse<T>` and wrapped controller, validation, authentication, authorization, root, and health responses.
- Updated delete/logout actions to return a consistent response body.
- Updated README to match the current architecture and response contract.

Current risks and next improvements:

- Public registration currently accepts `roleId`, so callers can choose their role. A production app should normally create public registrations as `Customer` only.
- Role, game, and genre write endpoints currently require any authenticated user. Production management endpoints should usually be restricted to Admin.
- `Jwt:SecretKey` in committed settings is for local development only. Use environment variables, user secrets, or a secret manager outside local development.
- There is no automated test project yet.

## Verification

Because a running local API process can lock `bin/Debug`, a temp output build is useful during development:

```powershell
dotnet build GameStore/GameStore.csproj -o C:\tmp\gamestore-build
```

Latest verification:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

When a test project is added, run:

```powershell
dotnet test
```
