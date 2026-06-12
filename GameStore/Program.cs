using GameStore.Data;
using GameStore.EndPoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
builder.AddGameStoreDb();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

app.MapGameStoreEndpoints();
app.MapGenreEPEndpoints();
app.MigrationDb();

app.Run();
