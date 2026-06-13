using System.ComponentModel.DataAnnotations;

namespace GameStore.Dtos;

public record LoginUserDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
