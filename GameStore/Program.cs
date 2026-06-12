using GameStore.Data;
using GameStore.EndPoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();

// Database configuration
builder.AddGameStoreDb();

// Add services to the container.

// builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();
// app.UseAuthorization();
// app.MapControllers();




app.MapGet("/", () => "Hello World!");

app.MapGameStoreEndpoints();
app.MapGenreEPEndpoints();
app.MigrationDb();

app.Run();
