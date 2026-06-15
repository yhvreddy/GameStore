using GameStore.Dtos;
using GameStore.Extensions;
using GameStore.Interfaces;
using GameStore.Mapping;
using GameStore.Models;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Tags("Orders")]
[Authorize]
public class OrdersController(IOrderRepository orders, ILogService logService) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Checkout(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<OrderDto>.Fail("User is not authorized."));
        }

        Order? order = await orders.CheckoutAsync(userId, cancellationToken);
        if (order is null)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail("Cart is empty."));
        }

        await logService.LogAsync(
            "Information",
            $"Checkout completed for order ID {order.Id}.",
            "Orders.Checkout",
            userId,
            cancellationToken: cancellationToken);

        return Created($"/orders/my/{order.Id}", ApiResponse<OrderDto>.Ok(order.ToDto(), "Checkout completed successfully."));
    }

    [HttpGet("orders/my")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OrderDto>>>> MyOrders(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyCollection<OrderDto>>.Fail("User is not authorized."));
        }

        var orderDtos = (await orders.GetUserOrdersAsync(userId, cancellationToken))
            .Select(order => order.ToDto())
            .ToList();

        return Ok(ApiResponse<IReadOnlyCollection<OrderDto>>.Ok(orderDtos, "Orders retrieved successfully."));
    }

    [HttpGet("orders/my/{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> MyOrder(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<OrderDto>.Fail("User is not authorized."));
        }

        Order? order = await orders.GetUserOrderAsync(userId, id, cancellationToken);
        return order is null
            ? NotFound(ApiResponse<OrderDto>.Fail("Order was not found."))
            : Ok(ApiResponse<OrderDto>.Ok(order.ToDto(), "Order retrieved successfully."));
    }
}
