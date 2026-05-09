using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Foundations.RolesPermissions;
using ServerSectorUz.Core.Models.Orchestrations.RolesPermissions;
using ServerSectorUz.Core.Services.Foundations.RolesPermissions;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService permissionService;

    public PermissionsController(IPermissionService permissionService) =>
        this.permissionService = permissionService;

    [HttpGet]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public ActionResult<IQueryable<Permission>> GetAllPermissions() =>
        Ok(this.permissionService.RetrieveAllPermissions());

    [HttpGet("{permissionId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<Permission>> GetPermissionByIdAsync(Guid permissionId)
    {
        try
        {
            Permission? permission = await this.permissionService.RetrievePermissionByIdAsync(permissionId);
            return permission is null ? NotFound() : Ok(permission);
        }
        catch (RolesPermissionsValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (RolesPermissionsServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Permission>> PostPermissionAsync(Permission permission)
    {
        try
        {
            Permission createdPermission = await this.permissionService.AddPermissionAsync(permission);
            return CreatedAtAction(nameof(GetPermissionByIdAsync), new { permissionId = createdPermission.Id }, createdPermission);
        }
        catch (RolesPermissionsValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (RolesPermissionsServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPut("{permissionId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Permission>> PutPermissionAsync(Guid permissionId, Permission permission)
    {
        try
        {
            permission.Id = permissionId;
            Permission modifiedPermission = await this.permissionService.ModifyPermissionAsync(permission);
            return Ok(modifiedPermission);
        }
        catch (RolesPermissionsValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (RolesPermissionsServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpDelete("{permissionId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Permission>> DeletePermissionAsync(Guid permissionId)
    {
        try
        {
            Permission removedPermission = await this.permissionService.RemovePermissionAsync(permissionId);
            return Ok(removedPermission);
        }
        catch (RolesPermissionsValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (RolesPermissionsServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<RolePermission>> AssignPermissionToRoleAsync(AssignPermissionToRoleRequest request)
    {
        try
        {
            RolePermission assignment =
                await this.permissionService.AssignPermissionToRoleAsync(request.RoleId, request.PermissionId);

            return Ok(assignment);
        }
        catch (RolesPermissionsValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (RolesPermissionsServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("role/{roleId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<IReadOnlyList<Permission>>> GetRolePermissionsAsync(Guid roleId)
    {
        try
        {
            IReadOnlyList<Permission> permissions =
                await this.permissionService.RetrievePermissionsByRoleIdAsync(roleId);

            return Ok(permissions);
        }
        catch (RolesPermissionsValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (RolesPermissionsServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }
}
