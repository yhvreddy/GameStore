namespace GameStore.Dtos;

public record CartItemDto(
    int GameId,
    string GameName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);
