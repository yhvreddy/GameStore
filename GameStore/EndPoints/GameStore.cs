using GameStore.Data;
using GameStore.Dtos;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.EndPoints;

public static class GameStore
{

    const string GetGameByIdEndpointName = "GetGameById";

    public static void MapGameStoreEndpoints(this WebApplication app)
    {
        // Map the game store endpoints

        var gamesGroup = app.MapGroup("/games").WithTags("Game Store");

        // Endpoint to get all games
        // GET /games
        gamesGroup.MapGet("/", async (GameStoreContext context) => await context.Games
        .Include(game => game.Genre)
        .Select(game => new GameSummaryDto(
            game.Id,
            game.Name,
            game.Genre!.Name,
            game.Price,
            game.ReleaseDate
        ))
        .AsNoTracking()
        .ToListAsync()).WithName("GetAllGames");

        // Endpoint to get a game by ID
        // GET /games/{id}
        gamesGroup.MapGet("/{id}", async (int id, GameStoreContext context) =>
        {
            var game = await context.Games.FindAsync(id);
            return game is null ? Results.NotFound() : Results.Ok(
                new GameDetailsDto(
                    game.Id,
                    game.Name,
                    game.GenreId,
                    game.Price,
                    game.ReleaseDate
                )
            );
        }).WithName(GetGameByIdEndpointName);

        // Endpoint to add a new game
        // POST /games
        gamesGroup.MapPost("/", async (CreateGameDto newGame, GameStoreContext context) =>
        {

            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };
            context.Games.Add(game);
            await context.SaveChangesAsync();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameByIdEndpointName, new { id = gameDto.Id }, gameDto);
        });

        // Endpoint to update an existing game
        // PUT /games/{id}
        gamesGroup.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext context) =>
        {
            var game = await context.Games.FindAsync(id);
            if (game is null)
            {
                return Results.NotFound();
            }

            game.Name = updatedGame.Name;
            game.GenreId = updatedGame.GenreId;
            game.Price = updatedGame.Price;
            game.ReleaseDate = updatedGame.ReleaseDate;

            await context.SaveChangesAsync();
            return Results.NoContent();
        });

        // Endpoint to delete a game
        // DELETE /games/{id}
        gamesGroup.MapDelete("/{id}", async (int id, GameStoreContext context) =>
        {
            var game = await context.Games.FindAsync(id);
            if (game is null)
            {
                return Results.NotFound();
            }
            await context.Games.Where(g => g.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });
    }
}