using System.Security.Claims;

namespace GameStore.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal claimsPrincipal, out int userId)
    {
        string? userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId);
    }
}
