namespace GameStore.Dtos;

public record CreateLogDto(
    string Level,
    string Message,
    string? Source,
    int? UserId,
    string? Exception);
