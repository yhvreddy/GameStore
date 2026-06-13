namespace GameStore.Dtos;

public record UserDto(
    int Id,
    string FullName,
    string Email,
    int RoleId
);
