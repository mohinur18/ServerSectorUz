using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;

public interface IUserCredentialService
{
    ValueTask<UserCredential> AddUserCredentialAsync(UserCredential userCredential);
    IQueryable<UserCredential> RetrieveAllUserCredentials();
    ValueTask<UserCredential?> RetrieveUserCredentialByUserIdAsync(Guid userId);
    ValueTask<UserCredential> ModifyUserCredentialAsync(UserCredential userCredential);
    ValueTask<UserCredential> RemoveUserCredentialByUserIdAsync(Guid userId);
}
