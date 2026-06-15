using GameStore.Dtos;
using GameStore.Interfaces;
using GameStore.Mapping;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Route("genres")]
[Tags("Genres")]
[Authorize]
public class GenresController(IGenreRepository genres, ILogService logService) : ControllerBase
{
    [HttpGet(Name = "GetAllGenres")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GenreDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var genreDtos = (await genres.GetAllAsync(cancellationToken))
            .Select(genre => genre.ToDto())
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<GenreDto>>.Ok(genreDtos, "Genres retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetGenreById")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GenreDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        Genre? genre = await genres.GetByIdAsync(id, cancellationToken);
        return genre is null
            ? NotFound(ApiResponse<GenreDto>.Fail("Genre was not found."))
            : Ok(ApiResponse<GenreDto>.Ok(genre.ToDto(), "Genre retrieved successfully."));
    }

    [HttpPost(Name = "CreateGenre")]
    public async Task<ActionResult<ApiResponse<GenreDto>>> Create(GenreCreateDto genreDto, CancellationToken cancellationToken)
    {
        Genre genre = new() { Name = genreDto.Name };
        await genres.AddAsync(genre, cancellationToken);
        await logService.LogAsync("Information", $"Genre created: {genre.Name}.", "Genres.Create", cancellationToken: cancellationToken);

        return CreatedAtRoute(
            "GetGenreById",
            new { id = genre.Id },
            ApiResponse<GenreDto>.Ok(genre.ToDto(), "Genre created successfully."));
    }

    [HttpPut("{id:int}", Name = "UpdateGenre")]
    public async Task<ActionResult<ApiResponse<GenreDto>>> Update(int id, GenreCreateDto genreDto, CancellationToken cancellationToken)
    {
        Genre? genre = await genres.GetByIdAsync(id, cancellationToken);
        if (genre is null)
        {
            return NotFound(ApiResponse<GenreDto>.Fail("Genre was not found."));
        }

        genre.Name = genreDto.Name;
        await genres.SaveChangesAsync(cancellationToken);
        await logService.LogAsync("Information", $"Genre updated: {genre.Name}.", "Genres.Update", cancellationToken: cancellationToken);

        return Ok(ApiResponse<GenreDto>.Ok(genre.ToDto(), "Genre updated successfully."));
    }

    [HttpDelete("{id:int}", Name = "DeleteGenre")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        Genre? genre = await genres.GetByIdAsync(id, cancellationToken);
        if (genre is null)
        {
            return NotFound(ApiResponse<object>.Fail("Genre was not found."));
        }

        await genres.DeleteAsync(genre, cancellationToken);
        await logService.LogAsync("Information", $"Genre deleted: {genre.Name}.", "Genres.Delete", cancellationToken: cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Genre deleted successfully."));
    }
}
