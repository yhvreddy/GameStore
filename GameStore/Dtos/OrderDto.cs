namespace GameStore.Dtos;

public record OrderDto(
    int Id,
    DateTime OrderedAt,
    decimal Total,
    IReadOnlyCollection<OrderItemDto> Items
);
