# GameStore Ecommerce API

GameStore is an ASP.NET Core minimal API for a small ecommerce-style game store. It supports public catalog browsing, JWT authentication, roles, cart management, checkout, and order history.

## Tech Stack

- .NET 10
- ASP.NET Core minimal APIs
- Entity Framework Core
- SQLite
- JWT bearer authentication
- Swagger / OpenAPI with Swashbuckle
- Data annotations validation

## Project Structure

```text
GameStore/
  Data/
    Migrations/      EF Core migrations
    Seeders/         Default role and genre seeders
    GameStoreContext.cs
    DataExtensions.cs
  Dtos/              Request and response records
  EndPoints/         Minimal API route mappings
  Models/            EF Core entity models
  Services/          Password hashing and JWT token services
  Swagger/           Swagger auth document filter
  Program.cs         Application startup and route registration
  *.http             Local API request samples
```

## Main Features

- User registration, login, token-revoking logout, and current-user profile.
- JWT-protected endpoints with Swagger Authorize support.
- Role table with seeded `Admin` and `Customer` roles.
- Public game and genre browsing.
- Authenticated game, genre, and role management.
- Authenticated cart add/update/remove/clear workflow.
- Authenticated checkout that converts cart items into an order.
- Authenticated order history for the current user.

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

Swagger UI:

```text
http://localhost:5137/swagger
```

## Docker Deployment

The repository includes a Docker setup for local deployment and server deployment. Use this when you want to run the API without installing/running the app directly with `dotnet run`.

Files:

- `Dockerfile` builds and publishes the ASP.NET Core API.
- `docker-compose.yml` runs the API on port `5137`.
- `.env.example` shows the required environment variables.
- `.dockerignore` keeps build output, local DB files, and editor files out of the Docker build context.

### Prerequisites

- Docker Desktop installed.
- Docker Desktop running with the Linux engine enabled.
- Run Docker commands from the repository root:

```powershell
cd D:\.Net\GameStore
```

### First-Time Setup

Create your local environment file from the template:

```powershell
copy .env.example .env
```

Update `.env` with a strong JWT secret. Keep this file private and do not commit it:

```text
JWT_SECRET_KEY=your-very-long-random-secret-key-here
```

The compose file will fail to start if `JWT_SECRET_KEY` is missing.

### Build And Start

```powershell
docker compose up --build -d
```

This command:

- Builds the Docker image from `Dockerfile`.
- Starts the `gamestore-api` container.
- Maps host port `5137` to container port `8080`.
- Stores SQLite data in the `gamestore-data` Docker volume.

### Verify The Container

```text
http://localhost:5137/health
```

```powershell
curl http://localhost:5137/health
```

Expected response:

```json
{
  "status": "Healthy"
}
```

### Swagger In Docker

By default, Docker runs the app with:

```yaml
ASPNETCORE_ENVIRONMENT: Production
```

Swagger is enabled only in `Development`. For local Docker testing, temporarily change this value in `docker-compose.yml`:

```yaml
ASPNETCORE_ENVIRONMENT: Development
```

Then rebuild and start again:

```powershell
docker compose up --build -d
```

Open Swagger:

```text
http://localhost:5137/swagger
```

For server deployment, keep `ASPNETCORE_ENVIRONMENT` as `Production`.

### Logs

```powershell
docker compose logs -f gamestore-api
```

### Stop

```powershell
docker compose down
```

### Rebuild After Code Changes

```powershell
docker compose up --build -d
```

### Reset The Docker Database

The container uses a named Docker volume:

```text
gamestore-data
```

Inside the container, SQLite is stored here:

```text
/app/data/GameStore.db
```

To stop the app and delete this database volume:

```powershell
docker compose down -v
```

The next `docker compose up --build -d` will create a fresh database, apply migrations, and run seeders again.

### Useful Docker Commands

```powershell
docker compose ps
docker compose logs gamestore-api
docker compose restart gamestore-api
docker compose down
```

### Production Notes

- Keep `.env` out of Git.
- Use a long random `JWT_SECRET_KEY`.
- Put the app behind HTTPS using a reverse proxy such as Nginx, Caddy, Azure App Service, or IIS.
- For real production traffic, consider moving from SQLite to SQL Server, PostgreSQL, or another managed database.

### Common Docker Issues

If Docker build fails with a Docker engine error, start Docker Desktop and wait until it says the engine is running.

If the app starts but JWT login fails, check that `.env` has a non-empty `JWT_SECRET_KEY`.

If old data is still showing, reset the volume:

```powershell
docker compose down -v
docker compose up --build -d
```

## CI Pipeline

GitHub Actions workflow:

```text
.github/workflows/ci.yml
```

The workflow runs on pushes to `main`/`master` and on pull requests. It:

- Restores NuGet packages.
- Builds the API in Release mode.
- Builds the Docker image as `gamestore-api:ci`.

This is a validation pipeline only. To deploy to a real host, add a push step for your registry, such as Docker Hub, GitHub Container Registry, Azure Container Registry, or AWS ECR, then deploy that image to your server or cloud service.

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
- `DatabaseSeeder` runs all seeders from one place.

Create a migration from the repository root:

```powershell
dotnet ef migrations add MigrationName --project GameStore --output-dir Data/Migrations
```

Apply migrations from the repository root:

```powershell
dotnet ef database update --project GameStore
```

If your terminal is already inside `GameStore/`, use:

```powershell
dotnet ef database update
```

## Authentication

Login returns a JWT token:

```http
POST http://localhost:5137/users/login
Content-Type: application/json

{
  "email": "admin@gamstore.com",
  "password": "admin@123!"
}
```

In Swagger:

1. Call `/users/login`.
2. Copy only the `token` value.
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

## Request Samples

Register:

```http
POST http://localhost:5137/users/register
Content-Type: application/json

{
  "fullName": "Harsha Yenumula",
  "email": "admin@gamstore.com",
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

More samples are available in:

- `GameStore/users.http`
- `GameStore/games.http`
- `GameStore/orders.http`

## Audit Notes

Completed audit updates:

- README updated to match users, roles, games, genres, cart, checkout, and orders.
- Stale weather request sample replaced with a current games request.
- Hardcoded JWT removed from `users.http`.
- Protected write examples now include `Authorization: Bearer {{token}}`.
- JWT generation now includes the user's role claim, so role-based authorization can work.
- Logout now revokes the current JWT server-side through the `RevokedTokens` table.

Current risks and next improvements:

- Public registration currently accepts `roleId`, so a caller can choose a role. In a production ecommerce app, registration should always create a `Customer`, and only an Admin should manage roles.
- Role, game, and genre write endpoints currently require any authenticated user. A production app should usually restrict these to Admin.
- `Jwt:SecretKey` is a local development value and should move to user secrets or environment variables for real deployments.
- There is no automated test project yet.

## Verification

Current manual verification commands:

```powershell
dotnet build GameStore/GameStore.csproj
dotnet ef database update --project GameStore
```

When a test project is added, run:

```powershell
dotnet test
```
