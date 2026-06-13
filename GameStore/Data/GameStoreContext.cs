using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<RevokedToken>()
            .HasIndex(revokedToken => revokedToken.JwtId)
            .IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasIndex(cartItem => new { cartItem.UserId, cartItem.GameId })
            .IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasOne(cartItem => cartItem.User)
            .WithMany()
            .HasForeignKey(cartItem => cartItem.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(cartItem => cartItem.Game)
            .WithMany()
            .HasForeignKey(cartItem => cartItem.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.User)
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Game)
            .WithMany()
            .HasForeignKey(orderItem => orderItem.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
