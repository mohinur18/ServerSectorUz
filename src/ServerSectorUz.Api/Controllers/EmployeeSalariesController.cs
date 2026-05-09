using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Models.Foundations.Salaries;
using ServerSectorUz.Core.Models.Orchestrations.Salaries;
using ServerSectorUz.Core.Services.Foundations.HR;
using ServerSectorUz.Core.Services.Foundations.Salaries;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeSalariesController : ControllerBase
{
    private readonly IEmployeeSalaryService employeeSalaryService;
    private readonly IEmployeeService employeeService;

    public EmployeeSalariesController(
        IEmployeeSalaryService employeeSalaryService,
        IEmployeeService employeeService)
    {
        this.employeeSalaryService = employeeSalaryService;
        this.employeeService = employeeService;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant)]
    public async ValueTask<ActionResult<EmployeeSalary>> AssignSalaryToEmployeeAsync(AssignEmployeeSalaryRequest request)
    {
        try
        {
            var employeeSalary = new EmployeeSalary
            {
                EmployeeId = request.EmployeeId,
                SalaryStructureId = request.SalaryStructureId,
                EffectiveFromDate = request.EffectiveFromDate,
                EffectiveToDate = request.EffectiveToDate,
                Amount = request.Amount,
                CreatedByUserId = ResolveAuthenticatedUserId()
            };

            EmployeeSalary created = await this.employeeSalaryService.AddEmployeeSalaryAsync(employeeSalary);
            return Ok(created);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPut("{employeeSalaryId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant)]
    public async ValueTask<ActionResult<EmployeeSalary>> UpdateEmployeeSalaryAsync(Guid employeeSalaryId, EmployeeSalary employeeSalary)
    {
        try
        {
            employeeSalary.Id = employeeSalaryId;
            employeeSalary.UpdatedByUserId = ResolveAuthenticatedUserId();

            EmployeeSalary updated = await this.employeeSalaryService.ModifyEmployeeSalaryAsync(employeeSalary);
            return Ok(updated);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPatch("{employeeSalaryId:guid}/deactivate")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant)]
    public async ValueTask<ActionResult<EmployeeSalary>> DeactivateSalaryAsync(Guid employeeSalaryId)
    {
        try
        {
            EmployeeSalary deactivated = await this.employeeSalaryService.DeactivateEmployeeSalaryAsync(
                employeeSalaryId,
                ResolveAuthenticatedUserId());

            return Ok(deactivated);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("employee/{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant + "," + SystemRoles.Hr + "," + SystemRoles.User)]
    public async ValueTask<ActionResult<IReadOnlyList<EmployeeSalary>>> GetSalariesByEmployeeAsync(Guid employeeId)
    {
        try
        {
            bool canReadAll = User.IsInRole(SystemRoles.Admin) ||
                              User.IsInRole(SystemRoles.Accountant) ||
                              User.IsInRole(SystemRoles.Hr);

            if (!canReadAll)
            {
                Employee? ownEmployee = await ResolveOwnEmployeeAsync();

                if (ownEmployee is null || ownEmployee.Id != employeeId)
                {
                    return Forbid();
                }
            }

            IReadOnlyList<EmployeeSalary> salaries =
                await this.employeeSalaryService.RetrieveEmployeeSalariesByEmployeeIdAsync(employeeId);

            return Ok(salaries);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("employee/{employeeId:guid}/active")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant + "," + SystemRoles.Hr + "," + SystemRoles.User)]
    public async ValueTask<ActionResult<EmployeeSalary>> GetActiveSalaryByEmployeeAsync(Guid employeeId)
    {
        try
        {
            bool canReadAll = User.IsInRole(SystemRoles.Admin) ||
                              User.IsInRole(SystemRoles.Accountant) ||
                              User.IsInRole(SystemRoles.Hr);

            if (!canReadAll)
            {
                Employee? ownEmployee = await ResolveOwnEmployeeAsync();

                if (ownEmployee is null || ownEmployee.Id != employeeId)
                {
                    return Forbid();
                }
            }

            EmployeeSalary? activeSalary =
                await this.employeeSalaryService.RetrieveActiveEmployeeSalaryByEmployeeIdAsync(employeeId);

            return activeSalary is null ? NotFound() : Ok(activeSalary);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    private Guid? ResolveAuthenticatedUserId()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId) || userId == Guid.Empty)
        {
            return null;
        }

        return userId;
    }

    private async ValueTask<Employee?> ResolveOwnEmployeeAsync()
    {
        Guid? userId = ResolveAuthenticatedUserId();

        if (!userId.HasValue)
        {
            return null;
        }

        return await this.employeeService.RetrieveEmployeeByUserIdAsync(userId.Value);
    }
}
