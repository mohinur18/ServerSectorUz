using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.HR;

public class Employee : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTimeOffset JoinDate { get; set; }
}
