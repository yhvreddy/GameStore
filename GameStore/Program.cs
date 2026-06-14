using GameStore.Data;
using GameStore.EndPoints;
using GameStore.Services;
using GameStore.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token from /users/login."
    });

    options.DocumentFilter<AuthorizeDocumentFilter>();
});

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<ILogService, LogService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                string? jwtId = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (string.IsNullOrWhiteSpace(jwtId))
                {
                    context.Fail("Token is missing a JWT ID.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<GameStoreContext>();
                bool tokenRevoked = await dbContext.RevokedTokens.AnyAsync(token => token.JwtId == jwtId);
                if (tokenRevoked)
                {
                    context.Fail("Token has been logged out.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// Database configuration
builder.AddGameStoreDb();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!").ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).ExcludeFromDescription();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapGameStoreEndpoints();
app.MapGenreEndpoints();
app.MapRoleEndpoints();
app.MapCartEndpoints();
app.MapOrderEndpoints();
app.MapLogEndpoints();
app.MigrationDb();

app.Run();
