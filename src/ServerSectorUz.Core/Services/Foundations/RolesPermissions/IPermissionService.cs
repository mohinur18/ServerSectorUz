using ServerSectorUz.Core.Models.Foundations.RolesPermissions;

namespace ServerSectorUz.Core.Services.Foundations.RolesPermissions;

public interface IPermissionService
{
    ValueTask<Permission> AddPermissionAsync(Permission permission);
    IQueryable<Permission> RetrieveAllPermissions();
    ValueTask<Permission?> RetrievePermissionByIdAsync(Guid permissionId);
    ValueTask<Permission> ModifyPermissionAsync(Permission permission);
    ValueTask<Permission> RemovePermissionAsync(Guid permissionId);

    ValueTask<RolePermission> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId);
    ValueTask<IReadOnlyList<Permission>> RetrievePermissionsByRoleIdAsync(Guid roleId);
}
