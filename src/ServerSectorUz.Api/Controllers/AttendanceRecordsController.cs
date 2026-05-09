using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Foundations.Attendance;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Services.Foundations.Attendance;
using ServerSectorUz.Core.Services.Foundations.HR;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceRecordsController : ControllerBase
{
    private readonly IAttendanceRecordService attendanceRecordService;
    private readonly IEmployeeService employeeService;

    public AttendanceRecordsController(
        IAttendanceRecordService attendanceRecordService,
        IEmployeeService employeeService)
    {
        this.attendanceRecordService = attendanceRecordService;
        this.employeeService = employeeService;
    }

    [HttpPost("check-in/{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<AttendanceRecord>> CheckInAsync(Guid employeeId)
    {
        try
        {
            AttendanceRecord record = await this.attendanceRecordService.CheckInAsync(employeeId);
            return Ok(record);
        }
        catch (AttendanceValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AttendanceServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost("check-out/{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<AttendanceRecord>> CheckOutAsync(Guid employeeId)
    {
        try
        {
            AttendanceRecord record = await this.attendanceRecordService.CheckOutAsync(employeeId);
            return Ok(record);
        }
        catch (AttendanceValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AttendanceServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("employee/{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager + "," + SystemRoles.User)]
    public async ValueTask<ActionResult<IReadOnlyList<AttendanceRecord>>> GetByEmployeeAsync(Guid employeeId)
    {
        try
        {
            bool canReadAll = User.IsInRole(SystemRoles.Admin) ||
                              User.IsInRole(SystemRoles.Hr) ||
                              User.IsInRole(SystemRoles.OfficeManager);

            if (!canReadAll)
            {
                Employee? ownEmployee = await ResolveOwnEmployeeAsync();

                if (ownEmployee is null || ownEmployee.Id != employeeId)
                {
                    return Forbid();
                }
            }

            IReadOnlyList<AttendanceRecord> records =
                await this.attendanceRecordService.RetrieveByEmployeeIdAsync(employeeId);

            return Ok(records);
        }
        catch (AttendanceValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AttendanceServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("date/{workDate}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<IReadOnlyList<AttendanceRecord>>> GetByDateAsync(DateOnly workDate)
    {
        try
        {
            IReadOnlyList<AttendanceRecord> records =
                await this.attendanceRecordService.RetrieveByDateAsync(workDate);

            return Ok(records);
        }
        catch (AttendanceValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AttendanceServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("range")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<IReadOnlyList<AttendanceRecord>>> GetByDateRangeAsync(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate)
    {
        try
        {
            IReadOnlyList<AttendanceRecord> records =
                await this.attendanceRecordService.RetrieveByDateRangeAsync(fromDate, toDate);

            return Ok(records);
        }
        catch (AttendanceValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AttendanceServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = SystemRoles.User + "," + SystemRoles.Admin + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<IReadOnlyList<AttendanceRecord>>> GetOwnAttendanceAsync()
    {
        try
        {
            Employee? ownEmployee = await ResolveOwnEmployeeAsync();

            if (ownEmployee is null)
            {
                return NotFound(new { Error = "Employee profile for authenticated user not found." });
            }

            IReadOnlyList<AttendanceRecord> records =
                await this.attendanceRecordService.RetrieveByEmployeeIdAsync(ownEmployee.Id);

            return Ok(records);
        }
        catch (AttendanceValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (AttendanceServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    private async ValueTask<Employee?> ResolveOwnEmployeeAsync()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId) || userId == Guid.Empty)
        {
            return null;
        }

        return await this.employeeService.RetrieveEmployeeByUserIdAsync(userId);
    }
}
