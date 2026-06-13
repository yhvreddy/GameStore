using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Data.Seeders;

public static class RoleSeeder
{
    public static void Seed(DbContext context)
    {
        if (context.Set<Role>().Any()) return;

        context.Set<Role>().AddRange(
            new Role { Name = "Admin", Slug = "admin" },
            new Role { Name = "Customer", Slug = "customer" }
        );

        context.SaveChanges();
    }
}
