using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BalanceFlow.Api.Controllers;

/// <summary>
/// Public authentication controller allowing developers and reviewers to quickly generate
/// mock JWT Bearer tokens to test the API's role-based access restrictions.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        // 1. Validation
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("Username is required.");
        }

        var allowedRoles = new[] { "Accountant", "Auditor", "Admin" };
        if (!allowedRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid role. Must be 'Accountant', 'Auditor', or 'Admin'.");
        }

        // 2. Load signing configurations
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "BalanceFlowIdentityServer";
        var audience = _configuration["Jwt:Audience"] ?? "BalanceFlowApi";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Populate Claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, request.Role) // Injected role claim checked by [Authorize(Roles = "...")]
        };

        // 4. Generate token options
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new TokenResponse(tokenString));
    }
}

public sealed record TokenRequest(string Username, string Role);
public sealed record TokenResponse(string Token);
