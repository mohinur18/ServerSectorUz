namespace ServerSectorUz.Core.Brokers.Securities;

public interface IPasswordBroker
{
    (string PasswordHash, string PasswordSalt) HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash, string passwordSalt);
}
