using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.Attendance;
using ServerSectorUz.Core.Services.Foundations.Attendance;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Attendance;

public class AttendanceRecordService : IAttendanceRecordService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public AttendanceRecordService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<AttendanceRecord> CheckInAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            bool employeeExists = await this.storageBroker.Employees
                .AnyAsync(employee => employee.Id == employeeId && employee.IsActive);

            if (!employeeExists)
            {
                throw new AttendanceValidationException("Employee was not found or inactive.");
            }

            bool hasOpenAttendance = await this.storageBroker.AttendanceRecords
                .AnyAsync(record =>
                    record.EmployeeId == employeeId &&
                    record.CheckInDate != null &&
                    record.CheckOutDate == null &&
                    record.IsActive);

            if (hasOpenAttendance)
            {
                throw new AttendanceValidationException("Employee already has an open attendance record.");
            }

            DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

            var attendanceRecord = new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                WorkDate = DateOnly.FromDateTime(now.UtcDateTime),
                CheckInDate = now,
                CheckOutDate = null,
                Status = "CheckedIn",
                IsActive = true,
                CreatedDate = now
            };

            await this.storageBroker.AttendanceRecords.AddAsync(attendanceRecord);
            await this.storageBroker.SaveChangesAsync();

            return attendanceRecord;
        }
        catch (AttendanceValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AttendanceDependencyException("Failed to check in employee.", exception);
        }
    }

    public async ValueTask<AttendanceRecord> CheckOutAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            AttendanceRecord? openAttendance = await this.storageBroker.AttendanceRecords
                .FirstOrDefaultAsync(record =>
                    record.EmployeeId == employeeId &&
                    record.CheckInDate != null &&
                    record.CheckOutDate == null &&
                    record.IsActive);

            if (openAttendance is null)
            {
                throw new AttendanceValidationException("Open attendance record was not found.");
            }

            DateTimeOffset checkOutTime = this.dateTimeBroker.GetCurrentDateTimeOffset();

            if (openAttendance.CheckInDate is null)
            {
                throw new AttendanceValidationException("Check-in time is required before checkout.");
            }

            if (checkOutTime < openAttendance.CheckInDate.Value)
            {
                throw new AttendanceValidationException("CheckOutTime cannot be earlier than CheckInTime.");
            }

            openAttendance.CheckOutDate = checkOutTime;
            openAttendance.Status = "CheckedOut";
            openAttendance.UpdatedDate = checkOutTime;

            await this.storageBroker.SaveChangesAsync();

            return openAttendance;
        }
        catch (AttendanceValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AttendanceDependencyException("Failed to check out employee.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<AttendanceRecord>> RetrieveByEmployeeIdAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            return await this.storageBroker.AttendanceRecords
                .AsNoTracking()
                .Where(record => record.EmployeeId == employeeId && record.IsActive)
                .OrderByDescending(record => record.WorkDate)
                .ThenByDescending(record => record.CheckInDate)
                .ToListAsync();
        }
        catch (AttendanceValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AttendanceServiceException("Failed to retrieve attendance by employee.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<AttendanceRecord>> RetrieveByDateAsync(DateOnly workDate)
    {
        try
        {
            ValidateDate(workDate, nameof(workDate));

            return await this.storageBroker.AttendanceRecords
                .AsNoTracking()
                .Where(record => record.WorkDate == workDate && record.IsActive)
                .OrderBy(record => record.EmployeeId)
                .ThenBy(record => record.CheckInDate)
                .ToListAsync();
        }
        catch (AttendanceValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AttendanceServiceException("Failed to retrieve attendance by date.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<AttendanceRecord>> RetrieveByDateRangeAsync(DateOnly fromDate, DateOnly toDate)
    {
        try
        {
            ValidateDate(fromDate, nameof(fromDate));
            ValidateDate(toDate, nameof(toDate));

            if (toDate < fromDate)
            {
                throw new AttendanceValidationException("ToDate cannot be earlier than FromDate.");
            }

            return await this.storageBroker.AttendanceRecords
                .AsNoTracking()
                .Where(record => record.WorkDate >= fromDate && record.WorkDate <= toDate && record.IsActive)
                .OrderBy(record => record.WorkDate)
                .ThenBy(record => record.EmployeeId)
                .ThenBy(record => record.CheckInDate)
                .ToListAsync();
        }
        catch (AttendanceValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new AttendanceServiceException("Failed to retrieve attendance by date range.", exception);
        }
    }

    private static void ValidateId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new AttendanceValidationException($"{parameterName} is invalid.");
        }
    }

    private static void ValidateDate(DateOnly date, string parameterName)
    {
        if (date == default)
        {
            throw new AttendanceValidationException($"{parameterName} is required.");
        }
    }
}
