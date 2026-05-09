using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Infrastructure.Services.Foundations.AuthenticationUsers;

public class UserCredentialService : IUserCredentialService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;

    public UserCredentialService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
    }

    public async ValueTask<UserCredential> AddUserCredentialAsync(UserCredential userCredential)
    {
        ValidateOnAdd(userCredential);

        userCredential.Id = Guid.NewGuid();
        userCredential.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
        userCredential.IsActive = true;

        await this.storageBroker.UserCredentials.AddAsync(userCredential);
        await this.storageBroker.SaveChangesAsync();

        return userCredential;
    }

    public IQueryable<UserCredential> RetrieveAllUserCredentials() =>
        this.storageBroker.UserCredentials.AsNoTracking();

    public async ValueTask<UserCredential?> RetrieveUserCredentialByUserIdAsync(Guid userId)
    {
        ValidateId(userId, nameof(userId));

        return await this.storageBroker.UserCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(credential => credential.UserId == userId);
    }

    public async ValueTask<UserCredential> ModifyUserCredentialAsync(UserCredential userCredential)
    {
        ValidateOnModify(userCredential);

        UserCredential? maybeCredential = await this.storageBroker.UserCredentials
            .FirstOrDefaultAsync(storedCredential => storedCredential.Id == userCredential.Id);

        if (maybeCredential is null)
        {
            throw new AuthenticationValidationException("User credential not found.");
        }

        maybeCredential.PasswordHash = userCredential.PasswordHash;
        maybeCredential.PasswordSalt = userCredential.PasswordSalt;
        maybeCredential.MustChangePassword = userCredential.MustChangePassword;
        maybeCredential.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();

        await this.storageBroker.SaveChangesAsync();

        return maybeCredential;
    }

    public async ValueTask<UserCredential> RemoveUserCredentialByUserIdAsync(Guid userId)
    {
        ValidateId(userId, nameof(userId));

        UserCredential? maybeCredential = await this.storageBroker.UserCredentials
            .FirstOrDefaultAsync(credential => credential.UserId == userId);

        if (maybeCredential is null)
        {
            throw new AuthenticationValidationException("User credential not found.");
        }

        this.storageBroker.UserCredentials.Remove(maybeCredential);
        await this.storageBroker.SaveChangesAsync();

        return maybeCredential;
    }

    private static void ValidateOnAdd(UserCredential userCredential)
    {
        if (userCredential is null)
        {
            throw new AuthenticationValidationException("User credential is required.");
        }

        ValidateId(userCredential.UserId, nameof(userCredential.UserId));

        if (string.IsNullOrWhiteSpace(userCredential.PasswordHash) ||
            string.IsNullOrWhiteSpace(userCredential.PasswordSalt))
        {
            throw new AuthenticationValidationException("Password hash and salt are required.");
        }
    }

    private static void ValidateOnModify(UserCredential userCredential)
    {
        ValidateOnAdd(userCredential);
        ValidateId(userCredential.Id, nameof(userCredential.Id));
    }

    private static void ValidateId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new AuthenticationValidationException($"{parameterName} is invalid.");
        }
    }
}
