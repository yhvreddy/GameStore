using System.ComponentModel.DataAnnotations;

namespace GameStore.Dtos;

public record RoleCreateDto(
    [Required][StringLength(100)] string Name,
    [Required][StringLength(100)] string Slug
);