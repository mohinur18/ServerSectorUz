using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerSectorUz.Core.Brokers.Tokens;
using ServerSectorUz.Core.Models.Configurations;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Infrastructure.Brokers.Tokens;

public class TokenBroker : ITokenBroker
{
    private readonly JwtOptions jwtOptions;

    public TokenBroker(IOptions<JwtOptions> jwtOptions) =>
        this.jwtOptions = jwtOptions.Value;

    public string GenerateAccessToken(User user, IEnumerable<string> roles, DateTimeOffset expiresOn)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresOn.UtcDateTime,
            Issuer = this.jwtOptions.Issuer,
            Audience = this.jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtOptions.SecurityKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
