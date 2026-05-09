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
public class RolesController : ControllerBase
{
    private readonly IRoleService roleService;

    public RolesController(IRoleService roleService) =>
        this.roleService = roleService;

    [HttpGet]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public ActionResult<IQueryable<Role>> GetAllRoles() =>
        Ok(this.roleService.RetrieveAllRoles());

    [HttpGet("{roleId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<Role>> GetRoleByIdAsync(Guid roleId)
    {
        try
        {
            Role? role = await this.roleService.RetrieveRoleByIdAsync(roleId);
            return role is null ? NotFound() : Ok(role);
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
    public async ValueTask<ActionResult<Role>> PostRoleAsync(Role role)
    {
        try
        {
            Role createdRole = await this.roleService.AddRoleAsync(role);
            return CreatedAtAction(nameof(GetRoleByIdAsync), new { roleId = createdRole.Id }, createdRole);
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

    [HttpPut("{roleId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Role>> PutRoleAsync(Guid roleId, Role role)
    {
        try
        {
            role.Id = roleId;
            Role modifiedRole = await this.roleService.ModifyRoleAsync(role);
            return Ok(modifiedRole);
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

    [HttpDelete("{roleId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Role>> DeleteRoleAsync(Guid roleId)
    {
        try
        {
            Role removedRole = await this.roleService.RemoveRoleAsync(roleId);
            return Ok(removedRole);
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

    [HttpPost("assign-user")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<UserRole>> AssignRoleToUserAsync(AssignRoleToUserRequest request)
    {
        try
        {
            UserRole userRole = await this.roleService.AssignRoleToUserAsync(request.UserId, request.RoleId);
            return Ok(userRole);
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

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<IReadOnlyList<Role>>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            IReadOnlyList<Role> roles = await this.roleService.RetrieveRolesByUserIdAsync(userId);
            return Ok(roles);
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
