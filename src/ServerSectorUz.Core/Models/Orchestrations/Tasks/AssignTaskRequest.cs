namespace ServerSectorUz.Core.Models.Orchestrations.Tasks;

public class AssignTaskRequest
{
    public Guid TaskId { get; set; }
    public Guid AssignedEmployeeId { get; set; }
}
