using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Core.Brokers.Tokens;

public interface ITokenBroker
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, DateTimeOffset expiresOn);
    string GenerateRefreshToken();
}
