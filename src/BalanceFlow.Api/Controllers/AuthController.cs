using BalanceFlow.Infrastructure.Data;
using BalanceFlow.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BalanceFlow.Api.Controllers;

/// <summary>
/// Core authentication controller handling user verification and secure JWT token creation.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates user credentials against the PostgreSQL database using PBKDF2 cryptography.
    /// Returns a signed JWT token on success.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower() && !u.IsDeleted);

        if (user == null || !user.IsActive || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password.");
        }

        // Load JWT configurations
        var secret = _configuration["Jwt:Secret"] ?? "SuperSecretSecuritySigningKeyForBalanceFlowApplicationSystemAuditServiceService2026";
        var issuer = _configuration["Jwt:Issuer"] ?? "BalanceFlowIdentityServer";
        var audience = _configuration["Jwt:Audience"] ?? "BalanceFlowApi";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Populate claims with DB data
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role) // Role is loaded dynamically from database record!
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new TokenResponse(tokenString));
    }
}

public sealed record LoginRequest(string Username, string Password);
public sealed record TokenResponse(string Token);
