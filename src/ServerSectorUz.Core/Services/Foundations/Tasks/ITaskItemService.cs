using ServerSectorUz.Core.Models.Foundations.Tasks;

namespace ServerSectorUz.Core.Services.Foundations.Tasks;

public interface ITaskItemService
{
    ValueTask<TaskItem> AddTaskItemAsync(TaskItem taskItem);
    ValueTask<TaskItem> AssignTaskAsync(Guid taskId, Guid assignedEmployeeId, Guid? updatedByUserId);
    ValueTask<TaskItem> ModifyTaskItemAsync(TaskItem taskItem, bool isAdmin);
    ValueTask<TaskItem> ChangeTaskStatusAsync(Guid taskId, string status, Guid actorEmployeeId, bool isAdmin);
    ValueTask<TaskItem> RemoveTaskItemAsync(Guid taskId);

    ValueTask<TaskItem?> RetrieveTaskItemByIdAsync(Guid taskId);
    ValueTask<IReadOnlyList<TaskItem>> RetrieveTasksByEmployeeIdAsync(Guid employeeId);
    ValueTask<IReadOnlyList<TaskItem>> RetrieveTasksByStatusAsync(string status);
    ValueTask<IReadOnlyList<TaskItem>> RetrieveOverdueTasksAsync();
}
