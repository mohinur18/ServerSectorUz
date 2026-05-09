using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Services.Foundations.HR;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService departmentService;

    public DepartmentsController(IDepartmentService departmentService) =>
        this.departmentService = departmentService;

    [HttpGet]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public ActionResult<IQueryable<Department>> GetAllDepartments() =>
        Ok(this.departmentService.RetrieveAllDepartments());

    [HttpGet("{departmentId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<Department>> GetDepartmentByIdAsync(Guid departmentId)
    {
        try
        {
            Department? department = await this.departmentService.RetrieveDepartmentByIdAsync(departmentId);
            return department is null ? NotFound() : Ok(department);
        }
        catch (HrValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (HrServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<Department>> PostDepartmentAsync(Department department)
    {
        try
        {
            Department createdDepartment = await this.departmentService.AddDepartmentAsync(department);
            return CreatedAtAction(nameof(GetDepartmentByIdAsync), new { departmentId = createdDepartment.Id }, createdDepartment);
        }
        catch (HrValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (HrServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPut("{departmentId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<Department>> PutDepartmentAsync(Guid departmentId, Department department)
    {
        try
        {
            department.Id = departmentId;
            Department modifiedDepartment = await this.departmentService.ModifyDepartmentAsync(department);
            return Ok(modifiedDepartment);
        }
        catch (HrValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (HrServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpDelete("{departmentId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Department>> DeleteDepartmentAsync(Guid departmentId)
    {
        try
        {
            Department removedDepartment = await this.departmentService.RemoveDepartmentAsync(departmentId);
            return Ok(removedDepartment);
        }
        catch (HrValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (HrServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }
}
