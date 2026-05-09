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

public class DepartmentService : IDepartmentService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public DepartmentService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<Department> AddDepartmentAsync(Department department)
    {
        try
        {
            ValidateDepartmentOnAdd(department);

            bool exists = await this.storageBroker.Departments
                .AnyAsync(storedDepartment =>
                    storedDepartment.Name == department.Name ||
                    storedDepartment.Code == department.Code);

            if (exists)
            {
                throw new HrValidationException("Department with same name or code already exists.");
            }

            department.Id = Guid.NewGuid();
            department.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            department.IsActive = true;

            await this.storageBroker.Departments.AddAsync(department);
            await this.storageBroker.SaveChangesAsync();

            return department;
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrDependencyException("Failed to add department.", exception);
        }
    }

    public IQueryable<Department> RetrieveAllDepartments() =>
        this.storageBroker.Departments.AsNoTracking().OrderBy(department => department.Name);

    public async ValueTask<Department?> RetrieveDepartmentByIdAsync(Guid departmentId)
    {
        try
        {
            ValidateId(departmentId, nameof(departmentId));

            return await this.storageBroker.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(department => department.Id == departmentId);
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrServiceException("Failed to retrieve department by id.", exception);
        }
    }

    public async ValueTask<Department> ModifyDepartmentAsync(Department department)
    {
        try
        {
            ValidateDepartmentOnModify(department);

            Department? storedDepartment = await this.storageBroker.Departments
                .FirstOrDefaultAsync(foundDepartment => foundDepartment.Id == department.Id);

            if (storedDepartment is null)
            {
                throw new HrValidationException("Department not found.");
            }

            bool duplicate = await this.storageBroker.Departments
                .AnyAsync(foundDepartment =>
                    foundDepartment.Id != department.Id &&
                    (foundDepartment.Name == department.Name || foundDepartment.Code == department.Code));

            if (duplicate)
            {
                throw new HrValidationException("Department with same name or code already exists.");
            }

            storedDepartment.Name = department.Name;
            storedDepartment.Code = department.Code;
            storedDepartment.IsActive = department.IsActive;
            storedDepartment.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            storedDepartment.UpdatedByUserId = department.UpdatedByUserId;

            await this.storageBroker.SaveChangesAsync();

            return storedDepartment;
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrServiceException("Failed to modify department.", exception);
        }
    }

    public async ValueTask<Department> RemoveDepartmentAsync(Guid departmentId)
    {
        try
        {
            ValidateId(departmentId, nameof(departmentId));

            Department? department = await this.storageBroker.Departments
                .FirstOrDefaultAsync(foundDepartment => foundDepartment.Id == departmentId);

            if (department is null)
            {
                throw new HrValidationException("Department not found.");
            }

            bool hasEmployees = await this.storageBroker.Employees
                .AnyAsync(employee => employee.DepartmentId == departmentId && employee.IsActive);

            if (hasEmployees)
            {
                throw new HrValidationException("Department has active employees and cannot be removed.");
            }

            this.storageBroker.Departments.Remove(department);
            await this.storageBroker.SaveChangesAsync();

            return department;
        }
        catch (HrValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new HrDependencyException("Failed to remove department.", exception);
        }
    }

    private static void ValidateDepartmentOnAdd(Department department)
    {
        if (department is null)
        {
            throw new HrValidationException("Department is required.");
        }

        ValidateString(department.Name, nameof(department.Name));
        ValidateString(department.Code, nameof(department.Code));

        department.Name = department.Name.Trim();
        department.Code = department.Code.Trim();
    }

    private static void ValidateDepartmentOnModify(Department department)
    {
        ValidateDepartmentOnAdd(department);
        ValidateId(department.Id, nameof(department.Id));
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
