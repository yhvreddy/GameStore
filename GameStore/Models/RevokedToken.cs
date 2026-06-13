namespace GameStore.Models;

public class RevokedToken
{
    public int Id { get; set; }
    public required string JwtId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
}
