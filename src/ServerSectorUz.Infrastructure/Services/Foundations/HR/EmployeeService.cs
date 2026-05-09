using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Services.Foundations.HR;

namespace ServerSectorUz.Infrastructure.Services.Foundations.HR;

public class EmployeeService : IEmployeeService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public EmployeeService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<Employee> AddEmployeeAsync(Employee employee)
    {
        try
        {
            await ValidateEmployeeOnAddAsync(employee);

            employee.Id = Guid.NewGuid();
            employee.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            employee.IsActive = true;

            await this.storageBroker.Employees.AddAsync(employee);
            await this.storageBroker.SaveChangesAsync();

            return employee;
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrDependencyException("Failed to add employee.", exception);
        }
    }

    public IQueryable<Employee> RetrieveAllEmployees() =>
        this.storageBroker.Employees.AsNoTracking().OrderBy(employee => employee.EmployeeNumber);

    public async ValueTask<Employee?> RetrieveEmployeeByIdAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            return await this.storageBroker.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(employee => employee.Id == employeeId);
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrServiceException("Failed to retrieve employee by id.", exception);
        }
    }

    public async ValueTask<Employee?> RetrieveEmployeeByUserIdAsync(Guid userId)
    {
        try
        {
            ValidateId(userId, nameof(userId));

            return await this.storageBroker.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(employee => employee.UserId == userId && employee.IsActive);
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrServiceException("Failed to retrieve employee by user id.", exception);
        }
    }

    public async ValueTask<Employee> ModifyEmployeeAsync(Employee employee)
    {
        try
        {
            await ValidateEmployeeOnModifyAsync(employee);

            Employee? storedEmployee = await this.storageBroker.Employees
                .FirstOrDefaultAsync(foundEmployee => foundEmployee.Id == employee.Id);

            if (storedEmployee is null)
            {
                throw new HrValidationException("Employee not found.");
            }

            storedEmployee.EmployeeNumber = employee.EmployeeNumber;
            storedEmployee.UserId = employee.UserId;
            storedEmployee.DepartmentId = employee.DepartmentId;
            storedEmployee.JoinDate = employee.JoinDate;
            storedEmployee.IsActive = employee.IsActive;
            storedEmployee.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            storedEmployee.UpdatedByUserId = employee.UpdatedByUserId;

            await this.storageBroker.SaveChangesAsync();

            return storedEmployee;
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrServiceException("Failed to modify employee.", exception);
        }
    }

    public async ValueTask<Employee> RemoveEmployeeAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            Employee? employee = await this.storageBroker.Employees
                .FirstOrDefaultAsync(foundEmployee => foundEmployee.Id == employeeId);

            if (employee is null)
            {
                throw new HrValidationException("Employee not found.");
            }

            this.storageBroker.Employees.Remove(employee);
            await this.storageBroker.SaveChangesAsync();

            return employee;
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrDependencyException("Failed to remove employee.", exception);
        }
    }

    private async ValueTask ValidateEmployeeOnAddAsync(Employee employee)
    {
        if (employee is null)
        {
            throw new HrValidationException("Employee is required.");
        }

        ValidateString(employee.EmployeeNumber, nameof(employee.EmployeeNumber));
        ValidateId(employee.DepartmentId, nameof(employee.DepartmentId));

        if (employee.JoinDate == default)
        {
            throw new HrValidationException("JoinDate is required.");
        }

        employee.EmployeeNumber = employee.EmployeeNumber.Trim();

        bool departmentExists = await this.storageBroker.Departments
            .AnyAsync(department => department.Id == employee.DepartmentId && department.IsActive);

        if (!departmentExists)
        {
            throw new HrValidationException("Department was not found or inactive.");
        }

        bool employeeNumberExists = await this.storageBroker.Employees
            .AnyAsync(storedEmployee => storedEmployee.EmployeeNumber == employee.EmployeeNumber);

        if (employeeNumberExists)
        {
            throw new HrValidationException("Employee number already exists.");
        }

        if (employee.UserId.HasValue)
        {
            bool duplicateUser = await this.storageBroker.Employees
                .AnyAsync(storedEmployee => storedEmployee.UserId == employee.UserId);

            if (duplicateUser)
            {
                throw new HrValidationException("This user is already attached to another employee profile.");
            }
        }
    }

    private async ValueTask ValidateEmployeeOnModifyAsync(Employee employee)
    {
        await ValidateEmployeeOnAddAsync(employee);
        ValidateId(employee.Id, nameof(employee.Id));

        bool employeeNumberExists = await this.storageBroker.Employees
            .AnyAsync(storedEmployee =>
                storedEmployee.Id != employee.Id &&
                storedEmployee.EmployeeNumber == employee.EmployeeNumber);

        if (employeeNumberExists)
        {
            throw new HrValidationException("Employee number already exists.");
        }

        if (employee.UserId.HasValue)
        {
            bool duplicateUser = await this.storageBroker.Employees
                .AnyAsync(storedEmployee =>
                    storedEmployee.Id != employee.Id && storedEmployee.UserId == employee.UserId);

            if (duplicateUser)
            {
                throw new HrValidationException("This user is already attached to another employee profile.");
            }
        }
    }

    private static void ValidateId(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new HrValidationException($"{name} is invalid.");
        }
    }

    private static void ValidateString(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new HrValidationException($"{name} is required.");
        }
    }
}
