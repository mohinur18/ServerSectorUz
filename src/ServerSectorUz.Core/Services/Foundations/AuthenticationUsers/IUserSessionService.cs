using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;

public interface IUserSessionService
{
    ValueTask<UserSession> AddUserSessionAsync(UserSession userSession);
    IQueryable<UserSession> RetrieveAllUserSessions();
    ValueTask<UserSession?> RetrieveUserSessionByIdAsync(Guid sessionId);
    ValueTask<UserSession?> RetrieveActiveUserSessionByUserIdAsync(Guid userId);
    ValueTask<UserSession> ModifyUserSessionAsync(UserSession userSession);
    ValueTask<UserSession> RemoveUserSessionAsync(Guid sessionId);
}
