using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Infrastructure.Services.Foundations.AuthenticationUsers;

public class UserService : IUserService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;

    public UserService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
    }

    public async ValueTask<User> AddUserAsync(User user)
    {
        ValidateUserOnAdd(user);

        if (await this.storageBroker.Users.AnyAsync(foundUser => foundUser.Email == user.Email))
        {
            throw new AuthenticationValidationException("User with this email already exists.");
        }

        user.Id = Guid.NewGuid();
        user.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
        user.UpdatedDate = null;
        user.IsActive = true;

        await this.storageBroker.Users.AddAsync(user);
        await this.storageBroker.SaveChangesAsync();

        return user;
    }

    public IQueryable<User> RetrieveAllUsers() =>
        this.storageBroker.Users.AsNoTracking();

    public async ValueTask<User?> RetrieveUserByIdAsync(Guid userId)
    {
        ValidateId(userId, nameof(userId));

        return await this.storageBroker.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async ValueTask<User?> RetrieveUserByEmailAsync(string email)
    {
        ValidateString(email, nameof(email));

        return await this.storageBroker.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email.Trim().ToLowerInvariant());
    }

    public async ValueTask<User> ModifyUserAsync(User user)
    {
        ValidateUserOnModify(user);

        User? maybeUser = await this.storageBroker.Users
            .FirstOrDefaultAsync(storedUser => storedUser.Id == user.Id);

        if (maybeUser is null)
        {
            throw new AuthenticationValidationException("User not found.");
        }

        maybeUser.FirstName = user.FirstName.Trim();
        maybeUser.LastName = user.LastName.Trim();
        maybeUser.Email = user.Email.Trim().ToLowerInvariant();
        maybeUser.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
        maybeUser.UpdatedByUserId = user.UpdatedByUserId;
        maybeUser.IsActive = user.IsActive;

        await this.storageBroker.SaveChangesAsync();

        return maybeUser;
    }

    public async ValueTask<User> RemoveUserAsync(Guid userId)
    {
        ValidateId(userId, nameof(userId));

        User? maybeUser = await this.storageBroker.Users
            .FirstOrDefaultAsync(user => user.Id == userId);

        if (maybeUser is null)
        {
            throw new AuthenticationValidationException("User not found.");
        }

        this.storageBroker.Users.Remove(maybeUser);
        await this.storageBroker.SaveChangesAsync();

        return maybeUser;
    }

    private void ValidateUserOnAdd(User user)
    {
        if (user is null)
        {
            throw new AuthenticationValidationException("User is required.");
        }

        ValidateString(user.FirstName, nameof(user.FirstName));
        ValidateString(user.LastName, nameof(user.LastName));
        ValidateString(user.Email, nameof(user.Email));

        user.FirstName = user.FirstName.Trim();
        user.LastName = user.LastName.Trim();
        user.Email = user.Email.Trim().ToLowerInvariant();
    }

    private void ValidateUserOnModify(User user)
    {
        ValidateUserOnAdd(user);
        ValidateId(user.Id, nameof(user.Id));
    }

    private static void ValidateId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new AuthenticationValidationException($"{parameterName} is invalid.");
        }
    }

    private static void ValidateString(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AuthenticationValidationException($"{parameterName} is required.");
        }
    }
}
