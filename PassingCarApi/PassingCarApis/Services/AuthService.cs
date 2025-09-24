using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PassingCarApis.Models;
using PassingCarApis.Utils;

namespace PassingCarApis.Services;

public class AuthService
{
    private readonly TokenSettings _tokenSettings;
    private readonly IConfiguration _configuration;

    public AuthService(IOptions<TokenSettings> tokenSettings, IConfiguration configuration)
    {
        _tokenSettings = tokenSettings.Value;
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user, ProfileType profile)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("Id", user.Id.ToString()),
            new("Email", user.Email ?? string.Empty),
            new("Profile", profile.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _tokenSettings.Issuer,
            audience: _tokenSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_tokenSettings.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public UserIdAndProfile GetUserProfileFromToken(string jwtToken)
    {
        try
        {
            int userId = 0;
            ProfileType profile = ProfileType.Fisica;
            JwtSecurityToken jwtTokenDetails = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            IEnumerable<Claim> claimsWithIdVal = jwtTokenDetails.Claims.Where(c => c.Type == "Id");
            IEnumerable<Claim> claimsWithProfileVal = jwtTokenDetails.Claims.Where(c => c.Type == "Profile");
            if (claimsWithIdVal != null && claimsWithIdVal.Count() > 0)
            {
                Claim userClaim = claimsWithIdVal.FirstOrDefault()!;
                if (userClaim != null)
                {
                    _ = int.TryParse(userClaim.Value, out userId);
                }
            }
            if (claimsWithProfileVal != null && claimsWithProfileVal.Count() > 0)
            {
                Claim userClaim = claimsWithProfileVal.FirstOrDefault()!;
                if (userClaim != null)
                {
                    _ = Enum.TryParse(userClaim.Value, out profile);
                }
            }
            return new()
            {
                Id = userId,
                Profile = profile,
            };
        }
        catch (Exception ex)
        {
            // Log the error if needed
            Console.WriteLine($"Error parsing JWT token: {ex.Message}");
            return new();
        }
    }
}
