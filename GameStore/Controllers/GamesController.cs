using GameStore.Dtos;
using GameStore.Interfaces;
using GameStore.Mapping;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Route("games")]
[Tags("Game Store")]
[Authorize]
public class GamesController(IGameRepository games, ILogService logService) : ControllerBase
{
    [HttpGet(Name = "GetAllGames")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GameSummaryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var gameDtos = (await games.GetAllAsync(cancellationToken))
            .Select(game => game.ToSummaryDto())
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<GameSummaryDto>>.Ok(gameDtos, "Games retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetGameById")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GameDetailsDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        Game? game = await games.GetByIdAsync(id, cancellationToken);
        return game is null
            ? NotFound(ApiResponse<GameDetailsDto>.Fail("Game was not found."))
            : Ok(ApiResponse<GameDetailsDto>.Ok(game.ToDetailsDto(), "Game retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GameDetailsDto>>> Create(CreateGameDto newGame, CancellationToken cancellationToken)
    {
        Game game = new()
        {
            Name = newGame.Name,
            GenreId = newGame.GenreId,
            Price = newGame.Price,
            ReleaseDate = newGame.ReleaseDate
        };

        await games.AddAsync(game, cancellationToken);
        await logService.LogAsync("Information", $"Game created: {game.Name}.", "Games.Create", cancellationToken: cancellationToken);

        GameDetailsDto gameDto = game.ToDetailsDto();
        return CreatedAtRoute(
            "GetGameById",
            new { id = gameDto.Id },
            ApiResponse<GameDetailsDto>.Ok(gameDto, "Game created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<GameDetailsDto>>> Update(int id, UpdateGameDto updatedGame, CancellationToken cancellationToken)
    {
        Game? game = await games.GetByIdAsync(id, cancellationToken);
        if (game is null)
        {
            return NotFound(ApiResponse<GameDetailsDto>.Fail("Game was not found."));
        }

        game.Name = updatedGame.Name;
        game.GenreId = updatedGame.GenreId;
        game.Price = updatedGame.Price;
        game.ReleaseDate = updatedGame.ReleaseDate;

        await games.SaveChangesAsync(cancellationToken);
        await logService.LogAsync("Information", $"Game updated: {game.Name}.", "Games.Update", cancellationToken: cancellationToken);

        return Ok(ApiResponse<GameDetailsDto>.Ok(game.ToDetailsDto(), "Game updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        Game? game = await games.GetByIdAsync(id, cancellationToken);
        if (game is null)
        {
            return NotFound(ApiResponse<object>.Fail("Game was not found."));
        }

        await games.DeleteAsync(game, cancellationToken);
        await logService.LogAsync("Information", $"Game deleted: {game.Name}.", "Games.Delete", cancellationToken: cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Game deleted successfully."));
    }
}
