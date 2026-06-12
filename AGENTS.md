# Repository Guidelines

## Project Structure & Module Organization

This repository contains a single ASP.NET Core project in `GameStore/`, referenced by `GameStore.slnx`.

- `GameStore/Program.cs` configures services, endpoint mapping, and database migration startup.
- `GameStore/EndPoints/` contains minimal API endpoint groups, currently `GameStore.cs`.
- `GameStore/Dtos/` contains request and response DTOs such as `CreateGameDto` and `GameSummaryDto`.
- `GameStore/Models/` contains EF Core entity classes.
- `GameStore/Data/` contains `GameStoreContext`, data setup extensions, and EF migrations in `Data/Migrations/`.
- `GameStore/*.http` files are local request samples for manual API testing.

Generated folders such as `bin/`, `obj/`, `.vscode/`, and local SQLite database files are ignored.

## Build, Test, and Development Commands

Run commands from the repository root unless noted.

- `dotnet build GameStore/GameStore.csproj` builds the application.
- `dotnet run --project GameStore/GameStore.csproj` starts the local API.
- `dotnet ef migrations add <Name> --project GameStore --output-dir Data/Migrations` creates an EF Core migration.
- `dotnet ef database update --project GameStore` applies migrations to the configured SQLite database.
- `dotnet ef dbcontext info --project GameStore` verifies design-time EF configuration.

No test project is currently present. Add one before introducing behavior that needs automated coverage.

## Coding Style & Naming Conventions

Use standard C# conventions with nullable reference types enabled. Prefer 4-space indentation, file-scoped namespaces, PascalCase for types and public members, camelCase for locals and parameters, and descriptive DTO names ending in `Dto`.

Keep minimal API route registration in endpoint extension classes under `EndPoints/`. Keep persistence concerns in `Data/`, and avoid mixing EF entity models with external request DTOs.

## Testing Guidelines

When tests are added, use a separate test project such as `GameStore.Tests/`. Name test classes after the unit under test, for example `GameStoreEndpointsTests`, and test methods by behavior, for example `CreateGame_ReturnsCreatedGame`.

Run tests with `dotnet test` from the repository root after adding the test project.

## Commit & Pull Request Guidelines

This repository has no established commit history yet. Use short, imperative commit messages such as `Add game creation endpoint` or `Configure SQLite migrations`.

Pull requests should include a concise description, commands run for verification, linked issues when applicable, and sample request/response details for API changes. Include screenshots only when a user interface is introduced or changed.

## Security & Configuration Tips

Do not commit local database files, user-specific project files, or secrets. Store connection strings in `appsettings.Development.json`, user secrets, or environment variables when values differ by machine. Keep committed configuration safe for local development only.
