using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogProject.Api.Services;

public class JwtService(IConfiguration configuration)
{
    private readonly string? _secretKey = configuration["Jwt:Key"];
    private readonly string? _issuer = configuration["Jwt:Issuer"];
    private readonly string? _audience = configuration["Jwt:Audience"];

    public string GenerateToken(string userId, List<string> roles)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            throw new InvalidOperationException("JWT secret key is not configured. Проверьте, что в appsettings.json или переменных окружения задан Jwt:Key.");

        if (string.IsNullOrWhiteSpace(_issuer))
            throw new InvalidOperationException("JWT issuer is not configured. Проверьте, что в appsettings.json или переменных окружения задан Jwt:Issuer.");

        if (string.IsNullOrWhiteSpace(_audience))
            throw new InvalidOperationException("JWT audience is not configured. Проверьте, что в appsettings.json или переменных окружения задан Jwt:Audience.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
       {
           new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId),
           new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
       };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}