using System.ComponentModel.DataAnnotations;

namespace GameStore.Dtos;

public record GenreCreateDto(
    [Required][StringLength(100)]
    string Name
);
