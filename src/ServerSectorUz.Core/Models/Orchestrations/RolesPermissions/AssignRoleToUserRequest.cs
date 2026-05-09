namespace ServerSectorUz.Core.Models.Orchestrations.RolesPermissions;

public class AssignRoleToUserRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
