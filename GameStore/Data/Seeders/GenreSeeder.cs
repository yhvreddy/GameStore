using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Data.Seeders;

public static class GenreSeeder
{
    public static void Seed(DbContext context)
    {
        if (context.Set<Genre>().Any()) return;

        context.Set<Genre>().AddRange(
            new Genre { Name = "Action" },
            new Genre { Name = "Adventure" },
            new Genre { Name = "RPG" },
            new Genre { Name = "Strategy" },
            new Genre { Name = "Sports" }
        );

        context.SaveChanges();
    }
}
