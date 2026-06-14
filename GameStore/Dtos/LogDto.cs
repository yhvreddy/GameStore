namespace GameStore.Dtos;

public record LogDto(
    int Id,
    string Level,
    string Message,
    string? Source,
    int? UserId,
    string? Exception,
    DateTime CreatedAt);
