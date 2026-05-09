using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Infrastructure.Services.Foundations.AuthenticationUsers;

public class UserSessionService : IUserSessionService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;

    public UserSessionService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
    }

    public async ValueTask<UserSession> AddUserSessionAsync(UserSession userSession)
    {
        ValidateOnAdd(userSession);

        userSession.Id = Guid.NewGuid();
        userSession.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
        userSession.IsActive = true;

        await this.storageBroker.UserSessions.AddAsync(userSession);
        await this.storageBroker.SaveChangesAsync();

        return userSession;
    }

    public IQueryable<UserSession> RetrieveAllUserSessions() =>
        this.storageBroker.UserSessions.AsNoTracking();

    public async ValueTask<UserSession?> RetrieveUserSessionByIdAsync(Guid sessionId)
    {
        ValidateId(sessionId, nameof(sessionId));

        return await this.storageBroker.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.Id == sessionId);
    }

    public async ValueTask<UserSession?> RetrieveActiveUserSessionByUserIdAsync(Guid userId)
    {
        ValidateId(userId, nameof(userId));

        DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

        return await this.storageBroker.UserSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .Where(session => session.RevokedDate == null)
            .Where(session => session.ExpiresDate > now)
            .OrderByDescending(session => session.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public async ValueTask<UserSession> ModifyUserSessionAsync(UserSession userSession)
    {
        ValidateOnModify(userSession);

        UserSession? maybeSession = await this.storageBroker.UserSessions
            .FirstOrDefaultAsync(storedSession => storedSession.Id == userSession.Id);

        if (maybeSession is null)
        {
            throw new AuthenticationValidationException("User session not found.");
        }

        maybeSession.RefreshTokenHash = userSession.RefreshTokenHash;
        maybeSession.ExpiresDate = userSession.ExpiresDate;
        maybeSession.RevokedDate = userSession.RevokedDate;
        maybeSession.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
        maybeSession.IsActive = userSession.IsActive;

        await this.storageBroker.SaveChangesAsync();

        return maybeSession;
    }

    public async ValueTask<UserSession> RemoveUserSessionAsync(Guid sessionId)
    {
        ValidateId(sessionId, nameof(sessionId));

        UserSession? maybeSession = await this.storageBroker.UserSessions
            .FirstOrDefaultAsync(session => session.Id == sessionId);

        if (maybeSession is null)
        {
            throw new AuthenticationValidationException("User session not found.");
        }

        this.storageBroker.UserSessions.Remove(maybeSession);
        await this.storageBroker.SaveChangesAsync();

        return maybeSession;
    }

    private static void ValidateOnAdd(UserSession userSession)
    {
        if (userSession is null)
        {
            throw new AuthenticationValidationException("User session is required.");
        }

        ValidateId(userSession.UserId, nameof(userSession.UserId));

        if (string.IsNullOrWhiteSpace(userSession.RefreshTokenHash))
        {
            throw new AuthenticationValidationException("Refresh token hash is required.");
        }

        if (userSession.ExpiresDate == default)
        {
            throw new AuthenticationValidationException("Session expiry date is required.");
        }
    }

    private static void ValidateOnModify(UserSession userSession)
    {
        ValidateOnAdd(userSession);
        ValidateId(userSession.Id, nameof(userSession.Id));
    }

    private static void ValidateId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new AuthenticationValidationException($"{parameterName} is invalid.");
        }
    }
}
