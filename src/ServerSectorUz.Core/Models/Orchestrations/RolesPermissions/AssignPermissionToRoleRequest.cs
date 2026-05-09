namespace ServerSectorUz.Core.Models.Orchestrations.RolesPermissions;

public class AssignPermissionToRoleRequest
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
