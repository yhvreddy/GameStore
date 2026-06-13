namespace GameStore.Models;

public class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
