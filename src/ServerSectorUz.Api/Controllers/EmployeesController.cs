using System.Security.Claims;
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
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService employeeService;

    public EmployeesController(IEmployeeService employeeService) =>
        this.employeeService = employeeService;

    [HttpGet]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public ActionResult<IQueryable<Employee>> GetAllEmployees() =>
        Ok(this.employeeService.RetrieveAllEmployees());

    [HttpGet("{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<Employee>> GetEmployeeByIdAsync(Guid employeeId)
    {
        try
        {
            Employee? employee = await this.employeeService.RetrieveEmployeeByIdAsync(employeeId);
            return employee is null ? NotFound() : Ok(employee);
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

    [HttpGet("me")]
    [Authorize(Roles = SystemRoles.User + "," + SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<Employee>> GetOwnEmployeeProfileAsync()
    {
        try
        {
            string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out Guid userId) || userId == Guid.Empty)
            {
                return Unauthorized(new { Error = "Authenticated user identifier is invalid." });
            }

            Employee? employee = await this.employeeService.RetrieveEmployeeByUserIdAsync(userId);
            return employee is null ? NotFound() : Ok(employee);
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
    public async ValueTask<ActionResult<Employee>> PostEmployeeAsync(Employee employee)
    {
        try
        {
            Employee createdEmployee = await this.employeeService.AddEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployeeByIdAsync), new { employeeId = createdEmployee.Id }, createdEmployee);
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

    [HttpPut("{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<Employee>> PutEmployeeAsync(Guid employeeId, Employee employee)
    {
        try
        {
            employee.Id = employeeId;
            Employee modifiedEmployee = await this.employeeService.ModifyEmployeeAsync(employee);
            return Ok(modifiedEmployee);
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

    [HttpDelete("{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<Employee>> DeleteEmployeeAsync(Guid employeeId)
    {
        try
        {
            Employee removedEmployee = await this.employeeService.RemoveEmployeeAsync(employeeId);
            return Ok(removedEmployee);
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
