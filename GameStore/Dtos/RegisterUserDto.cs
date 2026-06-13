using System.ComponentModel.DataAnnotations;

namespace GameStore.Dtos;

public record RegisterUserDto(
    [Required][StringLength(100)] string FullName,
    [Required][EmailAddress][StringLength(255)] string Email,
    [Required][MinLength(6)][StringLength(100)] string Password
);
