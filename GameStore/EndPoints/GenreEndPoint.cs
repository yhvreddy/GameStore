using GameStore.Data;
using GameStore.Dtos;
using GameStore.Models;
using GameStore.Services;
using Microsoft.EntityFrameworkCore;

namespace GameStore.EndPoints;

public static class GenreEndPoint
{
    public static void MapGenreEndpoints(this WebApplication app)
    {
        // Map the genre endpoints
        var genresGroup = app.MapGroup("/genres").WithTags("Genres").RequireAuthorization();

        // Endpoint to get all genres
        // GET /genres
        genresGroup.MapGet("/", async (GameStoreContext context) => await context.Genres
        .Select(genre => new GenreDto(
            genre.Id,
            genre.Name
        ))
        .AsNoTracking()
        .ToListAsync()).WithName("GetAllGenres").AllowAnonymous();

        // Endpoint to get a genre by ID
        // GET /genres/{id}
        genresGroup.MapGet("/{id:int}", async (int id, GameStoreContext context) =>
        {
            var genre = await context.Genres
                .Where(g => g.Id == id)
                .Select(g => new GenreDto(
                    g.Id,
                    g.Name
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return genre is not null ? Results.Ok(genre) : Results.NotFound();
        }).WithName("GetGenreById").AllowAnonymous();

        // Endpoint to create a new genre
        // POST /genres
        genresGroup.MapPost("/", async (GenreCreateDto genreDto, GameStoreContext context, ILogService logService) =>
        {
            var genre = new Genre
            {
                Name = genreDto.Name
            };

            context.Genres.Add(genre);
            await context.SaveChangesAsync();
            await logService.LogAsync("Information", $"Genre created: {genre.Name}.", "Genres.Create");

            return Results.Created($"/genres/{genre.Id}", new GenreDto(genre.Id, genre.Name));
        }).WithName("CreateGenre");


        // Endpoint to update an existing genre
        // PUT /genres/{id}
        genresGroup.MapPut("/{id:int}", async (int id, GenreCreateDto genreDto, GameStoreContext context, ILogService logService) =>
        {
            var genre = await context.Genres.FindAsync(id);
            if (genre is null)
            {
                return Results.NotFound();
            }

            genre.Name = genreDto.Name;
            await context.SaveChangesAsync();
            await logService.LogAsync("Information", $"Genre updated: {genre.Name}.", "Genres.Update");

            return Results.NoContent();
        }).WithName("UpdateGenre");

        // Endpoint to delete a genre
        // DELETE /genres/{id}
        genresGroup.MapDelete("/{id:int}", async (int id, GameStoreContext context, ILogService logService) =>
        {
            var genre = await context.Genres.FindAsync(id);
            if (genre is null)
            {
                return Results.NotFound();
            }

            await context.Genres.Where(g => g.Id == id).ExecuteDeleteAsync();
            await logService.LogAsync("Information", $"Genre deleted: {genre.Name}.", "Genres.Delete");

            return Results.NoContent();
        }).WithName("DeleteGenre");
    }
}
