using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameStore.Dtos;
using GameStore.Models;
using Microsoft.IdentityModel.Tokens;

namespace GameStore.Services;

public class JwtTokenService(IConfiguration configuration)
{
    public AuthResponseDto CreateToken(User user)
    {
        string issuer = configuration["Jwt:Issuer"]!;
        string audience = configuration["Jwt:Audience"]!;
        string secretKey = configuration["Jwt:SecretKey"]!;

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
        ];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        DateTime expiresAt = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return new AuthResponseDto(tokenValue, expiresAt, new UserDto(user.Id, user.FullName, user.Email, user.RoleId));
    }
}
