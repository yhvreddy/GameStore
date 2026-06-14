using System.Security.Claims;
using GameStore.Data;
using GameStore.Dtos;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GameStore.EndPoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/checkout", [Authorize] async (
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context,
            ILogService logService) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            List<CartItem> cartItems = await context.CartItems
                .Where(item => item.UserId == userId)
                .Include(item => item.Game)
                .OrderBy(item => item.Id)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return Results.BadRequest("Cart is empty.");
            }

            await using var transaction = await context.Database.BeginTransactionAsync();

            Order order = new()
            {
                UserId = userId,
                Items = cartItems.Select(item => new OrderItem
                {
                    GameId = item.GameId,
                    GameName = item.Game!.Name,
                    UnitPrice = item.Game.Price,
                    Quantity = item.Quantity
                }).ToList()
            };
            order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);

            context.Orders.Add(order);
            context.CartItems.RemoveRange(cartItems);
            await context.SaveChangesAsync();
            await logService.LogAsync(
                "Information",
                $"Checkout completed for order ID {order.Id}.",
                "Orders.Checkout",
                userId);
            await transaction.CommitAsync();

            return Results.Created($"/orders/my/{order.Id}", ToOrderDto(order));
        })
        .WithTags("Orders")
        .RequireAuthorization();

        var ordersGroup = app.MapGroup("/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        ordersGroup.MapGet("/my", [Authorize] async (ClaimsPrincipal claimsPrincipal, GameStoreContext context) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            List<Order> orders = await context.Orders
                .Where(order => order.UserId == userId)
                .Include(order => order.Items)
                .OrderByDescending(order => order.OrderedAt)
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(orders.Select(ToOrderDto));
        });

        ordersGroup.MapGet("/my/{id:int}", [Authorize] async (
            int id,
            ClaimsPrincipal claimsPrincipal,
            GameStoreContext context) =>
        {
            if (!TryGetUserId(claimsPrincipal, out int userId))
            {
                return Results.Unauthorized();
            }

            Order? order = await context.Orders
                .Include(order => order.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(order => order.Id == id && order.UserId == userId);

            return order is null ? Results.NotFound() : Results.Ok(ToOrderDto(order));
        });
    }

    private static OrderDto ToOrderDto(Order order)
    {
        List<OrderItemDto> items = order.Items
            .OrderBy(item => item.Id)
            .Select(item => new OrderItemDto(
                item.GameId,
                item.GameName,
                item.UnitPrice,
                item.Quantity,
                item.UnitPrice * item.Quantity
            ))
            .ToList();

        return new OrderDto(order.Id, order.OrderedAt, order.Total, items);
    }

    private static bool TryGetUserId(ClaimsPrincipal claimsPrincipal, out int userId)
    {
        string? userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }
}
