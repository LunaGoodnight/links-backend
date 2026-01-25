using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LinksService.Data;
using LinksService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LinksService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LinksContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(LinksContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { error = "Invalid username or password" });
        }

        var token = GenerateJwtToken(user.Username);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
                       _configuration["Jwt:Secret"] ??
                       throw new InvalidOperationException("JWT secret not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: "LinksService",
            audience: "LinksAdmin",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
