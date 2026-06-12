# GameStore

GameStore is a small ASP.NET Core minimal API for managing a video game catalog. It uses Entity Framework Core with SQLite for persistence and exposes CRUD endpoints for games and genres.

## Tech Stack

- .NET 10
- ASP.NET Core minimal APIs
- Entity Framework Core
- SQLite
- Swagger / OpenAPI with Swashbuckle
- Data annotations validation

## Project Structure

```text
GameStore/
  Data/           EF Core DbContext, database setup, and migrations
  Dtos/           Request and response records
  EndPoints/      Minimal API route mappings
  Models/         Entity models
  Properties/     Launch settings
  Program.cs      Application startup and service configuration
  games.http      Sample HTTP requests
```

Important files:

- `GameStore/Data/GameStoreContext.cs` defines `Games` and `Genres` tables.
- `GameStore/Data/DataExtensions.cs` configures SQLite, runs migrations, and seeds default genres.
- `GameStore/EndPoints/GameStore.cs` defines the `/games` API.
- `GameStore/EndPoints/GenreEP.cs` defines the `/genres` API.
- `GameStore/appsettings.json` contains the SQLite connection string.

## Getting Started

Clone the repository, then restore and build:

```powershell
dotnet restore GameStore/GameStore.csproj
dotnet build GameStore/GameStore.csproj
```

Run the API:

```powershell
dotnet run --project GameStore/GameStore.csproj --launch-profile http
```

Default HTTP URL: `http://localhost:5137`

Swagger UI is available in Development at:

```text
http://localhost:5137/swagger
```

The OpenAPI JSON document is available at:

```text
http://localhost:5137/swagger/v1/swagger.json
```

The app uses `GameStore.db` as a local SQLite database. On startup, pending migrations are applied automatically and default genres are seeded:

- Action
- Adventure
- RPG
- Strategy
- Sports

## Database Commands

Create a migration:

```powershell
dotnet ef migrations add InitialCreate --project GameStore --output-dir Data/Migrations
```

Apply migrations manually:

```powershell
dotnet ef database update --project GameStore
```

Check EF design-time configuration:

```powershell
dotnet ef dbcontext info --project GameStore
```

## API Endpoints

Game routes:

| Method | Route | Description |
| --- | --- | --- |
| GET | `/games` | Get all games |
| GET | `/games/{id}` | Get one game by ID |
| POST | `/games` | Create a game |
| PUT | `/games/{id}` | Update a game |
| DELETE | `/games/{id}` | Delete a game |

Genre routes:

| Method | Route | Description |
| --- | --- | --- |
| GET | `/genres` | Get all genres |
| GET | `/genres/{id}` | Get one genre by ID |
| POST | `/genres` | Create a genre |
| PUT | `/genres/{id}` | Update a genre |
| DELETE | `/genres/{id}` | Delete a genre |

## Request Examples

Create a game:

```http
POST http://localhost:5137/games
Content-Type: application/json

{
  "name": "Game of Thrones: The Role-Playing Game",
  "genreId": 1,
  "price": 85.10,
  "releaseDate": "2020-12-10"
}
```

Update a game:

```http
PUT http://localhost:5137/games/1
Content-Type: application/json

{
  "name": "Transformers: The Last Knight",
  "genreId": 4,
  "price": 25.99,
  "releaseDate": "2022-03-03"
}
```

Create a genre:

```http
POST http://localhost:5137/genres
Content-Type: application/json

{
  "name": "Racing"
}
```

## Validation Rules

Game create and update requests use these validation rules:

- `name` is required and has a maximum length of 100 characters.
- `genreId` is required and must be at least `1`.
- `price` is required and must be between `10` and `99.99`.
- `releaseDate` is required and uses `YYYY-MM-DD` format.

Genre create and update requests require `name` with a maximum length of 100 characters.

## Audit Notes

- Build status: `dotnet build GameStore/GameStore.csproj` succeeds with no warnings or errors.
- The app applies EF Core migrations at startup through `app.MigrationDb()`.
- Default genres are seeded only when the `Genres` table is empty.
- Deleting a genre also deletes related games because the current migration uses cascade delete.
- There is no automated test project yet.

## Local Development Notes

Use `GameStore/games.http` to send sample requests from supported IDEs. Generated files such as `bin/`, `obj/`, `.vscode/`, `*.user`, and local SQLite database files are ignored by Git.

There is currently no test project. When tests are added, run them with:

```powershell
dotnet test
```
