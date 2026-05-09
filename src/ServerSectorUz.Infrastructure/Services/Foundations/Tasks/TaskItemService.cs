using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.Tasks;
using ServerSectorUz.Core.Services.Foundations.Tasks;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Tasks;

public class TaskItemService : ITaskItemService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public TaskItemService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<TaskItem> AddTaskItemAsync(TaskItem taskItem)
    {
        try
        {
            await ValidateTaskOnAddAsync(taskItem);

            DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

            taskItem.Id = Guid.NewGuid();
            taskItem.Status = string.IsNullOrWhiteSpace(taskItem.Status) ? "New" : taskItem.Status.Trim();
            taskItem.Priority = string.IsNullOrWhiteSpace(taskItem.Priority) ? "Normal" : taskItem.Priority.Trim();
            taskItem.CreatedDate = now;
            taskItem.UpdatedDate = null;
            taskItem.IsActive = true;

            await this.storageBroker.TaskItems.AddAsync(taskItem);
            await this.storageBroker.SaveChangesAsync();

            return taskItem;
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskDependencyException("Failed to add task.", exception);
        }
    }

    public async ValueTask<TaskItem> AssignTaskAsync(Guid taskId, Guid assignedEmployeeId, Guid? updatedByUserId)
    {
        try
        {
            ValidateId(taskId, nameof(taskId));
            ValidateId(assignedEmployeeId, nameof(assignedEmployeeId));

            TaskItem? storedTask = await this.storageBroker.TaskItems
                .FirstOrDefaultAsync(task => task.Id == taskId);

            if (storedTask is null)
            {
                throw new TaskValidationException("Task was not found.");
            }

            if (IsCompleted(storedTask.Status))
            {
                throw new TaskValidationException("Completed tasks cannot be reassigned.");
            }

            bool employeeExists = await this.storageBroker.Employees
                .AnyAsync(employee => employee.Id == assignedEmployeeId && employee.IsActive);

            if (!employeeExists)
            {
                throw new TaskValidationException("Assigned employee was not found or inactive.");
            }

            storedTask.AssignedEmployeeId = assignedEmployeeId;
            storedTask.UpdatedByUserId = updatedByUserId;
            storedTask.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();

            await this.storageBroker.SaveChangesAsync();

            return storedTask;
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to assign task.", exception);
        }
    }

    public async ValueTask<TaskItem> ModifyTaskItemAsync(TaskItem taskItem, bool isAdmin)
    {
        try
        {
            await ValidateTaskOnModifyAsync(taskItem);

            TaskItem? storedTask = await this.storageBroker.TaskItems
                .FirstOrDefaultAsync(task => task.Id == taskItem.Id);

            if (storedTask is null)
            {
                throw new TaskValidationException("Task was not found.");
            }

            if (IsCompleted(storedTask.Status) && !isAdmin)
            {
                throw new TaskValidationException("Completed tasks cannot be edited except by Admin.");
            }

            storedTask.Title = taskItem.Title.Trim();
            storedTask.Description = taskItem.Description.Trim();
            storedTask.AssignedEmployeeId = taskItem.AssignedEmployeeId;
            storedTask.DueDate = taskItem.DueDate;
            storedTask.Priority = string.IsNullOrWhiteSpace(taskItem.Priority)
                ? storedTask.Priority
                : taskItem.Priority.Trim();
            storedTask.Status = string.IsNullOrWhiteSpace(taskItem.Status)
                ? storedTask.Status
                : taskItem.Status.Trim();
            storedTask.UpdatedByUserId = taskItem.UpdatedByUserId;
            storedTask.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            storedTask.IsActive = taskItem.IsActive;

            await this.storageBroker.SaveChangesAsync();

            return storedTask;
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to modify task.", exception);
        }
    }

    public async ValueTask<TaskItem> ChangeTaskStatusAsync(Guid taskId, string status, Guid actorEmployeeId, bool isAdmin)
    {
        try
        {
            ValidateId(taskId, nameof(taskId));
            ValidateId(actorEmployeeId, nameof(actorEmployeeId));
            ValidateString(status, nameof(status));

            TaskItem? storedTask = await this.storageBroker.TaskItems
                .FirstOrDefaultAsync(task => task.Id == taskId && task.IsActive);

            if (storedTask is null)
            {
                throw new TaskValidationException("Task was not found.");
            }

            if (!isAdmin && storedTask.AssignedEmployeeId != actorEmployeeId)
            {
                throw new TaskValidationException("Only assigned employee can change task status.");
            }

            storedTask.Status = status.Trim();
            storedTask.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();

            await this.storageBroker.SaveChangesAsync();

            return storedTask;
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to change task status.", exception);
        }
    }

    public async ValueTask<TaskItem> RemoveTaskItemAsync(Guid taskId)
    {
        try
        {
            ValidateId(taskId, nameof(taskId));

            TaskItem? task = await this.storageBroker.TaskItems
                .FirstOrDefaultAsync(foundTask => foundTask.Id == taskId);

            if (task is null)
            {
                throw new TaskValidationException("Task was not found.");
            }

            this.storageBroker.TaskItems.Remove(task);
            await this.storageBroker.SaveChangesAsync();

            return task;
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskDependencyException("Failed to remove task.", exception);
        }
    }

    public async ValueTask<TaskItem?> RetrieveTaskItemByIdAsync(Guid taskId)
    {
        try
        {
            ValidateId(taskId, nameof(taskId));

            return await this.storageBroker.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(task => task.Id == taskId && task.IsActive);
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to retrieve task by id.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<TaskItem>> RetrieveTasksByEmployeeIdAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            return await this.storageBroker.TaskItems
                .AsNoTracking()
                .Where(task => task.AssignedEmployeeId == employeeId && task.IsActive)
                .OrderByDescending(task => task.CreatedDate)
                .ToListAsync();
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to retrieve tasks by employee.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<TaskItem>> RetrieveTasksByStatusAsync(string status)
    {
        try
        {
            ValidateString(status, nameof(status));

            string normalizedStatus = status.Trim();

            return await this.storageBroker.TaskItems
                .AsNoTracking()
                .Where(task => task.Status == normalizedStatus && task.IsActive)
                .OrderByDescending(task => task.CreatedDate)
                .ToListAsync();
        }
        catch (TaskValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to retrieve tasks by status.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<TaskItem>> RetrieveOverdueTasksAsync()
    {
        try
        {
            DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

            return await this.storageBroker.TaskItems
                .AsNoTracking()
                .Where(task => task.IsActive)
                .Where(task => task.DueDate != null && task.DueDate < now)
                .Where(task => !IsCompleted(task.Status))
                .OrderBy(task => task.DueDate)
                .ToListAsync();
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new TaskServiceException("Failed to retrieve overdue tasks.", exception);
        }
    }

    private async ValueTask ValidateTaskOnAddAsync(TaskItem taskItem)
    {
        if (taskItem is null)
        {
            throw new TaskValidationException("Task is required.");
        }

        ValidateString(taskItem.Title, nameof(taskItem.Title));
        ValidateId(taskItem.AssignedEmployeeId, nameof(taskItem.AssignedEmployeeId));

        if (taskItem.DueDate.HasValue)
        {
            DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

            if (taskItem.DueDate.Value < now)
            {
                throw new TaskValidationException("DueDate cannot be in the past when creating.");
            }
        }

        bool employeeExists = await this.storageBroker.Employees
            .AnyAsync(employee => employee.Id == taskItem.AssignedEmployeeId && employee.IsActive);

        if (!employeeExists)
        {
            throw new TaskValidationException("Assigned employee must exist and be active.");
        }

        taskItem.Title = taskItem.Title.Trim();
        taskItem.Description = taskItem.Description?.Trim() ?? string.Empty;
    }

    private async ValueTask ValidateTaskOnModifyAsync(TaskItem taskItem)
    {
        await ValidateTaskOnAddAsync(taskItem);
        ValidateId(taskItem.Id, nameof(taskItem.Id));
    }

    private static bool IsCompleted(string status) =>
        string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase);

    private static void ValidateId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new TaskValidationException($"{parameterName} is invalid.");
        }
    }

    private static void ValidateString(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new TaskValidationException($"{parameterName} is required.");
        }
    }
}
