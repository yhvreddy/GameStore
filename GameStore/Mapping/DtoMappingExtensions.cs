using GameStore.Dtos;
using GameStore.Models;

namespace GameStore.Mapping;

public static class DtoMappingExtensions
{
    public static GameDetailsDto ToDetailsDto(this Game game) =>
        new(game.Id, game.Name, game.GenreId, game.Price, game.ReleaseDate);

    public static GameSummaryDto ToSummaryDto(this Game game) =>
        new(game.Id, game.Name, game.Genre?.Name ?? string.Empty, game.Price, game.ReleaseDate);

    public static GenreDto ToDto(this Genre genre) =>
        new(genre.Id, genre.Name);

    public static RoleDto ToDto(this Role role) =>
        new(role.Id, role.Name, role.Slug);

    public static UserDto ToDto(this User user) =>
        new(user.Id, user.FullName, user.Email, user.RoleId);

    public static LogDto ToDto(this AppLog log) =>
        new(log.Id, log.Level, log.Message, log.Source, log.UserId, log.Exception, log.CreatedAt);

    public static OrderDto ToDto(this Order order)
    {
        List<OrderItemDto> items = order.Items
            .OrderBy(item => item.Id)
            .Select(item => new OrderItemDto(
                item.GameId,
                item.GameName,
                item.UnitPrice,
                item.Quantity,
                item.UnitPrice * item.Quantity))
            .ToList();

        return new OrderDto(order.Id, order.OrderedAt, order.Total, items);
    }
}
