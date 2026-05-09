using ServerSectorUz.Core.Models.Foundations.RolesPermissions;

namespace ServerSectorUz.Core.Services.Foundations.RolesPermissions;

public interface IRoleService
{
    ValueTask<Role> AddRoleAsync(Role role);
    IQueryable<Role> RetrieveAllRoles();
    ValueTask<Role?> RetrieveRoleByIdAsync(Guid roleId);
    ValueTask<Role> ModifyRoleAsync(Role role);
    ValueTask<Role> RemoveRoleAsync(Guid roleId);

    ValueTask<UserRole> AssignRoleToUserAsync(Guid userId, Guid roleId);
    ValueTask<IReadOnlyList<Role>> RetrieveRolesByUserIdAsync(Guid userId);
}
