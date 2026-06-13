namespace GameStore.Dtos;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);
