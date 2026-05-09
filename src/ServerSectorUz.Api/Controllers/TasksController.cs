using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Models.Foundations.Tasks;
using ServerSectorUz.Core.Models.Orchestrations.Tasks;
using ServerSectorUz.Core.Services.Foundations.HR;
using ServerSectorUz.Core.Services.Foundations.Tasks;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskItemService taskItemService;
    private readonly IEmployeeService employeeService;

    public TasksController(
        ITaskItemService taskItemService,
        IEmployeeService employeeService)
    {
        this.taskItemService = taskItemService;
        this.employeeService = employeeService;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<TaskItem>> CreateTaskAsync(TaskItem taskItem)
    {
        try
        {
            Guid? actorUserId = ResolveAuthenticatedUserId();
            taskItem.CreatedByUserId = actorUserId;

            TaskItem createdTask = await this.taskItemService.AddTaskItemAsync(taskItem);
            return CreatedAtAction(nameof(GetTaskByIdAsync), new { taskId = createdTask.Id }, createdTask);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost("assign")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<TaskItem>> AssignTaskAsync(AssignTaskRequest request)
    {
        try
        {
            Guid? actorUserId = ResolveAuthenticatedUserId();

            TaskItem assignedTask = await this.taskItemService.AssignTaskAsync(
                request.TaskId,
                request.AssignedEmployeeId,
                actorUserId);

            return Ok(assignedTask);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPut("{taskId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<TaskItem>> UpdateTaskAsync(Guid taskId, TaskItem taskItem)
    {
        try
        {
            bool isAdmin = User.IsInRole(SystemRoles.Admin);
            taskItem.Id = taskId;
            taskItem.UpdatedByUserId = ResolveAuthenticatedUserId();

            TaskItem updatedTask = await this.taskItemService.ModifyTaskItemAsync(taskItem, isAdmin);
            return Ok(updatedTask);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPatch("{taskId:guid}/status")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager + "," + SystemRoles.User)]
    public async ValueTask<ActionResult<TaskItem>> ChangeStatusAsync(Guid taskId, ChangeTaskStatusRequest request)
    {
        try
        {
            bool isAdmin = User.IsInRole(SystemRoles.Admin);
            Employee? ownEmployee = await ResolveOwnEmployeeAsync();

            if (ownEmployee is null)
            {
                return Forbid();
            }

            TaskItem updatedTask = await this.taskItemService.ChangeTaskStatusAsync(
                taskId,
                request.Status,
                ownEmployee.Id,
                isAdmin);

            return Ok(updatedTask);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpDelete("{taskId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<TaskItem>> DeleteTaskAsync(Guid taskId)
    {
        try
        {
            TaskItem removedTask = await this.taskItemService.RemoveTaskItemAsync(taskId);
            return Ok(removedTask);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("{taskId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager + "," + SystemRoles.User)]
    public async ValueTask<ActionResult<TaskItem>> GetTaskByIdAsync(Guid taskId)
    {
        try
        {
            TaskItem? task = await this.taskItemService.RetrieveTaskItemByIdAsync(taskId);

            if (task is null)
            {
                return NotFound();
            }

            bool canManageAll = User.IsInRole(SystemRoles.Admin) ||
                                User.IsInRole(SystemRoles.Hr) ||
                                User.IsInRole(SystemRoles.OfficeManager);

            if (!canManageAll)
            {
                Employee? ownEmployee = await ResolveOwnEmployeeAsync();

                if (ownEmployee is null || task.AssignedEmployeeId != ownEmployee.Id)
                {
                    return Forbid();
                }
            }

            return Ok(task);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("employee/{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager + "," + SystemRoles.User)]
    public async ValueTask<ActionResult<IReadOnlyList<TaskItem>>> GetTasksByEmployeeAsync(Guid employeeId)
    {
        try
        {
            bool canManageAll = User.IsInRole(SystemRoles.Admin) ||
                                User.IsInRole(SystemRoles.Hr) ||
                                User.IsInRole(SystemRoles.OfficeManager);

            if (!canManageAll)
            {
                Employee? ownEmployee = await ResolveOwnEmployeeAsync();

                if (ownEmployee is null || ownEmployee.Id != employeeId)
                {
                    return Forbid();
                }
            }

            IReadOnlyList<TaskItem> tasks = await this.taskItemService.RetrieveTasksByEmployeeIdAsync(employeeId);
            return Ok(tasks);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<IReadOnlyList<TaskItem>>> GetTasksByStatusAsync(string status)
    {
        try
        {
            IReadOnlyList<TaskItem> tasks = await this.taskItemService.RetrieveTasksByStatusAsync(status);
            return Ok(tasks);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("overdue")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<IReadOnlyList<TaskItem>>> GetOverdueTasksAsync()
    {
        try
        {
            IReadOnlyList<TaskItem> tasks = await this.taskItemService.RetrieveOverdueTasksAsync();
            return Ok(tasks);
        }
        catch (TaskServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = SystemRoles.User + "," + SystemRoles.Admin + "," + SystemRoles.Hr + "," + SystemRoles.OfficeManager)]
    public async ValueTask<ActionResult<IReadOnlyList<TaskItem>>> GetMyTasksAsync()
    {
        try
        {
            Employee? ownEmployee = await ResolveOwnEmployeeAsync();

            if (ownEmployee is null)
            {
                return NotFound(new { Error = "Employee profile for authenticated user not found." });
            }

            IReadOnlyList<TaskItem> tasks = await this.taskItemService.RetrieveTasksByEmployeeIdAsync(ownEmployee.Id);
            return Ok(tasks);
        }
        catch (TaskValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (TaskServiceException exception)
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
