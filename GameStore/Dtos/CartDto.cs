namespace GameStore.Dtos;

public record CartDto(
    IReadOnlyCollection<CartItemDto> Items,
    decimal Total
);
