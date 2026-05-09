using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.Attendance;

public class AttendanceRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public DateTimeOffset? CheckInDate { get; set; }
    public DateTimeOffset? CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
