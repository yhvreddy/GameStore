using Microsoft.EntityFrameworkCore;

namespace GameStore.Data.Seeders;

public static class DatabaseSeeder
{
    public static void Seed(DbContext context)
    {
        RoleSeeder.Seed(context);
        GenreSeeder.Seed(context);
    }
}
