using System.ComponentModel.DataAnnotations;

namespace GameStore.Dtos;

public record CreateGameDto(
    [Required][StringLength(100)] string Name,
    [Required][Range(1, int.MaxValue)] int GenreId,
    [Required][Range(10, 99.99)] decimal Price,
    [Required] DateOnly ReleaseDate
);