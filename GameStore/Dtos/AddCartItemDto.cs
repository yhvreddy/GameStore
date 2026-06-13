namespace GameStore.Dtos;

public record AddCartItemDto(
    int GameId,
    int Quantity
);
