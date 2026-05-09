using ServerSectorUz.Core.Models.Foundations.Attendance;

namespace ServerSectorUz.Core.Services.Foundations.Attendance;

public interface IAttendanceRecordService
{
    ValueTask<AttendanceRecord> CheckInAsync(Guid employeeId);
    ValueTask<AttendanceRecord> CheckOutAsync(Guid employeeId);
    ValueTask<IReadOnlyList<AttendanceRecord>> RetrieveByEmployeeIdAsync(Guid employeeId);
    ValueTask<IReadOnlyList<AttendanceRecord>> RetrieveByDateAsync(DateOnly workDate);
    ValueTask<IReadOnlyList<AttendanceRecord>> RetrieveByDateRangeAsync(DateOnly fromDate, DateOnly toDate);
}
