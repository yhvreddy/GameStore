using GameStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Data.Seeders;

public static class GameSeeder
{
    public static void Seed(DbContext context)
    {
        if (context.Set<Game>().Any()) return;

        var genreIds = context.Set<Genre>()
            .ToDictionary(genre => genre.Name, genre => genre.Id);

        int GenreId(string name) => genreIds[name];

        context.Set<Game>().AddRange(
            new Game { Name = "Street Fighter II", GenreId = GenreId("Action"), Price = 19.99m, ReleaseDate = new DateOnly(1992, 7, 15) },
            new Game { Name = "The Legend of Zelda", GenreId = GenreId("Adventure"), Price = 29.99m, ReleaseDate = new DateOnly(1986, 2, 21) },
            new Game { Name = "Final Fantasy VII", GenreId = GenreId("RPG"), Price = 39.99m, ReleaseDate = new DateOnly(1997, 1, 31) },
            new Game { Name = "StarCraft", GenreId = GenreId("Strategy"), Price = 14.99m, ReleaseDate = new DateOnly(1998, 3, 31) },
            new Game { Name = "FIFA 23", GenreId = GenreId("Sports"), Price = 59.99m, ReleaseDate = new DateOnly(2022, 9, 30) },
            new Game { Name = "DOOM Eternal", GenreId = GenreId("Action"), Price = 49.99m, ReleaseDate = new DateOnly(2020, 3, 20) },
            new Game { Name = "Uncharted 4: A Thief's End", GenreId = GenreId("Adventure"), Price = 39.99m, ReleaseDate = new DateOnly(2016, 5, 10) },
            new Game { Name = "The Witcher 3: Wild Hunt", GenreId = GenreId("RPG"), Price = 49.99m, ReleaseDate = new DateOnly(2015, 5, 19) },
            new Game { Name = "Civilization VI", GenreId = GenreId("Strategy"), Price = 29.99m, ReleaseDate = new DateOnly(2016, 10, 21) },
            new Game { Name = "NBA 2K24", GenreId = GenreId("Sports"), Price = 69.99m, ReleaseDate = new DateOnly(2023, 9, 8) },
            new Game { Name = "Hades", GenreId = GenreId("Action"), Price = 24.99m, ReleaseDate = new DateOnly(2020, 9, 17) },
            new Game { Name = "Tomb Raider", GenreId = GenreId("Adventure"), Price = 19.99m, ReleaseDate = new DateOnly(2013, 3, 5) },
            new Game { Name = "Elden Ring", GenreId = GenreId("RPG"), Price = 59.99m, ReleaseDate = new DateOnly(2022, 2, 25) },
            new Game { Name = "Age of Empires IV", GenreId = GenreId("Strategy"), Price = 39.99m, ReleaseDate = new DateOnly(2021, 10, 28) },
            new Game { Name = "Forza Horizon 5", GenreId = GenreId("Sports"), Price = 59.99m, ReleaseDate = new DateOnly(2021, 11, 9) },
            new Game { Name = "God of War", GenreId = GenreId("Action"), Price = 49.99m, ReleaseDate = new DateOnly(2018, 4, 20) },
            new Game { Name = "Life is Strange", GenreId = GenreId("Adventure"), Price = 19.99m, ReleaseDate = new DateOnly(2015, 1, 30) },
            new Game { Name = "Persona 5 Royal", GenreId = GenreId("RPG"), Price = 59.99m, ReleaseDate = new DateOnly(2019, 10, 31) },
            new Game { Name = "XCOM 2", GenreId = GenreId("Strategy"), Price = 24.99m, ReleaseDate = new DateOnly(2016, 2, 5) },
            new Game { Name = "Rocket League", GenreId = GenreId("Sports"), Price = 19.99m, ReleaseDate = new DateOnly(2015, 7, 7) },
            new Game { Name = "Celeste", GenreId = GenreId("Action"), Price = 19.99m, ReleaseDate = new DateOnly(2018, 1, 25) },
            new Game { Name = "Firewatch", GenreId = GenreId("Adventure"), Price = 19.99m, ReleaseDate = new DateOnly(2016, 2, 9) },
            new Game { Name = "Dragon Age: Inquisition", GenreId = GenreId("RPG"), Price = 29.99m, ReleaseDate = new DateOnly(2014, 11, 18) },
            new Game { Name = "Total War: Warhammer III", GenreId = GenreId("Strategy"), Price = 59.99m, ReleaseDate = new DateOnly(2022, 2, 17) },
            new Game { Name = "Tony Hawk's Pro Skater 1 + 2", GenreId = GenreId("Sports"), Price = 39.99m, ReleaseDate = new DateOnly(2020, 9, 4) }
        );

        context.SaveChanges();
    }
}
