namespace GameStore.Models;

public class AppLog
{
    public int Id { get; set; }
    public required string Level { get; set; }
    public required string Message { get; set; }
    public string? Source { get; set; }
    public string? Exception { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
