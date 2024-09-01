using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Health.Api.Models.Constants;
using Health.Api.Models.Responses;
using Microsoft.IdentityModel.Tokens;

namespace Health.Api.Authentication;

public interface ITokenManager
{
    string GetAccessToken(LoginResponse loginResponse);
    (Guid, string) GetRefreshToken();
    bool TryValidateRefreshToken(string token, out Guid tokenId);
}

public class TokenManager : ITokenManager
{
    private IConfiguration _Configuration;

    public TokenManager(IConfiguration configuration)
    {
        _Configuration = configuration;
    }

    public string GetAccessToken(LoginResponse loginResponse)
    {
        var issuer = _Configuration["Jwt:Issuer"];
        var audience = _Configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(_Configuration["Jwt:Key"] ?? "access-token-key");
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimType.AppUserId, loginResponse.AppUserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, loginResponse.Username),
                new Claim(JwtRegisteredClaimNames.GivenName, loginResponse.Firstname),
                new Claim(JwtRegisteredClaimNames.FamilyName, loginResponse.Lastname),
                new Claim(ClaimType.CustomerId, loginResponse.CustomerId.ToString()),
                new Claim(ClaimType.CustomerCode, loginResponse.CustomerCode),
                new Claim(ClaimType.CustomerShortName, loginResponse.CustomerShortName),
                new Claim(ClaimType.CustomerName, loginResponse.CustomerName),
                new Claim(JwtRegisteredClaimNames.Email, loginResponse.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimType.LanguageCode, loginResponse.LanguageCode),
                new Claim(ClaimTypes.Role, loginResponse.RoleCode)
            }),

            Expires = DateTime.UtcNow.AddMinutes(15),
            Audience = audience,
            Issuer = issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key)
                , SecurityAlgorithms.HmacSha512Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }

    public (Guid, string) GetRefreshToken()
    {
        var tokenId = Guid.NewGuid();
        var issuer = _Configuration["Jwt:Issuer"];
        var audience = _Configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(_Configuration["Jwt:RefreshTokenKey"] ?? "refresh-token-key");

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()), }),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = issuer,
            Audience = audience,
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenId, tokenHandler.WriteToken(token));
    }

    public bool TryValidateRefreshToken(string token, out Guid tokenId)
    {
        var issuer = _Configuration["Jwt:Issuer"];
        var audience = _Configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes(_Configuration["Jwt:RefreshTokenKey"] ?? "refresh-token-key");

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidAudience = audience,
            ValidIssuer = issuer,
        };

        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParams, out SecurityToken refreshToken);
            var jwt = (JwtSecurityToken)refreshToken;
            var valid = Guid.TryParse(jwt.Id, out var id);
            tokenId = id;
            return valid;
        }
        catch (Exception)
        {
            tokenId = default;
            return false;
        }
    }

    public static (long customerId, long appUserId) GetCustomerAndAppUser(ClaimsPrincipal claimsPrincipal)
    {
        string GetClaimValue(string key)
        {
            return claimsPrincipal.Claims
                            .Where(c => c.Type.Equals(ClaimType.AppUserId))
                            .Select(s => s.Value).First();
        }

        long appUserId = Convert.ToInt64(GetClaimValue(ClaimType.AppUserId));
        long customerId = Convert.ToInt64(GetClaimValue(ClaimType.CustomerId));
        return (customerId, appUserId);
    }
}
