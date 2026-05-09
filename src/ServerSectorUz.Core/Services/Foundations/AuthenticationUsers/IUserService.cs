using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;

namespace ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;

public interface IUserService
{
    ValueTask<User> AddUserAsync(User user);
    IQueryable<User> RetrieveAllUsers();
    ValueTask<User?> RetrieveUserByIdAsync(Guid userId);
    ValueTask<User?> RetrieveUserByEmailAsync(string email);
    ValueTask<User> ModifyUserAsync(User user);
    ValueTask<User> RemoveUserAsync(Guid userId);
}
