using GameStore.Dtos;
using GameStore.Extensions;
using GameStore.Interfaces;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers;

[ApiController]
[Route("cart")]
[Tags("Cart")]
[Authorize]
public class CartController(ICartRepository cart, ILogService logService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> Get(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<CartDto>.Fail("User is not authorized."));
        }

        return Ok(ApiResponse<CartDto>.Ok(await cart.GetCartAsync(userId, cancellationToken), "Cart retrieved successfully."));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddItem(AddCartItemDto addCartItem, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<CartDto>.Fail("User is not authorized."));
        }

        if (addCartItem.Quantity <= 0)
        {
            return BadRequest(ApiResponse<CartDto>.Fail("Quantity must be greater than zero."));
        }

        bool itemAdded = await cart.AddItemAsync(userId, addCartItem, cancellationToken);
        if (!itemAdded)
        {
            return NotFound(ApiResponse<CartDto>.Fail("Game was not found."));
        }

        await logService.LogAsync(
            "Information",
            $"Cart item added or increased for game ID {addCartItem.GameId}.",
            "Cart.AddItem",
            userId,
            cancellationToken: cancellationToken);

        return Ok(ApiResponse<CartDto>.Ok(await cart.GetCartAsync(userId, cancellationToken), "Cart item added successfully."));
    }

    [HttpPut("items/{gameId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateItem(
        int gameId,
        UpdateCartItemDto updateCartItem,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<CartDto>.Fail("User is not authorized."));
        }

        if (updateCartItem.Quantity <= 0)
        {
            return BadRequest(ApiResponse<CartDto>.Fail("Quantity must be greater than zero."));
        }

        bool itemUpdated = await cart.UpdateItemAsync(userId, gameId, updateCartItem.Quantity, cancellationToken);
        if (!itemUpdated)
        {
            return NotFound(ApiResponse<CartDto>.Fail("Cart item was not found."));
        }

        await logService.LogAsync(
            "Information",
            $"Cart item quantity updated for game ID {gameId}.",
            "Cart.UpdateItem",
            userId,
            cancellationToken: cancellationToken);

        return Ok(ApiResponse<CartDto>.Ok(await cart.GetCartAsync(userId, cancellationToken), "Cart item updated successfully."));
    }

    [HttpDelete("items/{gameId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveItem(int gameId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("User is not authorized."));
        }

        bool removed = await cart.RemoveItemAsync(userId, gameId, cancellationToken);
        if (!removed)
        {
            return NotFound(ApiResponse<object>.Fail("Cart item was not found."));
        }

        await logService.LogAsync(
            "Information",
            $"Cart item removed for game ID {gameId}.",
            "Cart.RemoveItem",
            userId,
            cancellationToken: cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Cart item removed successfully."));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> Clear(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out int userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("User is not authorized."));
        }

        await cart.ClearAsync(userId, cancellationToken);
        await logService.LogAsync("Information", "Cart cleared.", "Cart.Clear", userId, cancellationToken: cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Cart cleared successfully."));
    }
}
