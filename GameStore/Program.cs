using GameStore.Data;
using GameStore.Dtos;
using GameStore.Repositories;
using GameStore.Services;
using GameStore.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            string[] errors = context.ModelState
                .Where(modelState => modelState.Value?.Errors.Count > 0)
                .SelectMany(modelState => modelState.Value!.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "The request is invalid."
                    : error.ErrorMessage)
                .ToArray();

            return new BadRequestObjectResult(
                ApiResponse<object>.Fail("Validation failed.", errors));
        };
    });
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
builder.Services.AddRepositories();

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
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("Authentication is required."));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("You do not have permission to access this resource."));
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

app.MapGet("/", () => Results.Ok(ApiResponse<string>.Ok("GameStore API", "API is running."))).ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(ApiResponse<object>.Ok(new { status = "Healthy" }, "Health check completed."))).ExcludeFromDescription();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MigrationDb();

app.Run();
