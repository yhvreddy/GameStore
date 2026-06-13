namespace GameStore.Dtos;

public record OrderItemDto(
    int GameId,
    string GameName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);
