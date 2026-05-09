using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ServerSectorUz.Core.Brokers.Securities;

namespace ServerSectorUz.Infrastructure.Brokers.Securities;

public class PasswordBroker : IPasswordBroker
{
    public (string PasswordHash, string PasswordSalt) HashPassword(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(32);

        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32));

        return (hash, Convert.ToBase64String(saltBytes));
    }

    public bool VerifyPassword(string password, string passwordHash, string passwordSalt)
    {
        byte[] saltBytes = Convert.FromBase64String(passwordSalt);

        string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32));

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(passwordHash));
    }
}
