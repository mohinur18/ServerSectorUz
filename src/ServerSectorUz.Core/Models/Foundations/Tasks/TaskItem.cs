using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.Tasks;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid AssignedEmployeeId { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}
