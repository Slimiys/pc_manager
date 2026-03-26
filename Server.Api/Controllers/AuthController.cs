using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Server.Api.Controllers;

/// <summary>
/// Контроллер выдачи тестового JWT токена.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Создает контроллер авторизации.
    /// </summary>
    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Возвращает JWT токен для роли.
    /// </summary>
    /// <param name="role">Роль пользователя.</param>
    [HttpPost("token")]
    public ActionResult<string> IssueToken([FromQuery] string role = "Operator")
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? "CHANGE_ME_FOR_PRODUCTION_32+_CHARS_SECRET";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "demo-user"),
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);
        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
