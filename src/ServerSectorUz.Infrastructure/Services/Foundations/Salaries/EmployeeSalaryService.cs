using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.Salaries;
using ServerSectorUz.Core.Services.Foundations.Salaries;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Salaries;

public class EmployeeSalaryService : IEmployeeSalaryService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public EmployeeSalaryService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<EmployeeSalary> AddEmployeeSalaryAsync(EmployeeSalary employeeSalary)
    {
        try
        {
            await ValidateEmployeeSalaryOnAddAsync(employeeSalary);

            DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

            // One active salary per employee rule.
            List<EmployeeSalary> activeSalaries = await this.storageBroker.EmployeeSalaries
                .Where(salary => salary.EmployeeId == employeeSalary.EmployeeId && salary.IsActive)
                .ToListAsync();

            foreach (EmployeeSalary activeSalary in activeSalaries)
            {
                activeSalary.IsActive = false;
                activeSalary.EffectiveToDate ??= DateOnly.FromDateTime(now.UtcDateTime);
                activeSalary.UpdatedDate = now;
            }

            employeeSalary.Id = Guid.NewGuid();
            employeeSalary.CreatedDate = now;
            employeeSalary.IsActive = true;

            await this.storageBroker.EmployeeSalaries.AddAsync(employeeSalary);
            await this.storageBroker.SaveChangesAsync();

            return employeeSalary;
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryDependencyException("Failed to assign salary to employee.", exception);
        }
    }

    public async ValueTask<EmployeeSalary> ModifyEmployeeSalaryAsync(EmployeeSalary employeeSalary)
    {
        try
        {
            await ValidateEmployeeSalaryOnModifyAsync(employeeSalary);

            EmployeeSalary? storedSalary = await this.storageBroker.EmployeeSalaries
                .FirstOrDefaultAsync(salary => salary.Id == employeeSalary.Id);

            if (storedSalary is null)
            {
                throw new SalaryValidationException("Employee salary was not found.");
            }

            if (employeeSalary.EffectiveToDate.HasValue &&
                employeeSalary.EffectiveToDate.Value < employeeSalary.EffectiveFromDate)
            {
                throw new SalaryValidationException("EffectiveTo cannot be earlier than EffectiveFrom.");
            }

            storedSalary.SalaryStructureId = employeeSalary.SalaryStructureId;
            storedSalary.EffectiveFromDate = employeeSalary.EffectiveFromDate;
            storedSalary.EffectiveToDate = employeeSalary.EffectiveToDate;
            storedSalary.Amount = employeeSalary.Amount;
            storedSalary.IsActive = employeeSalary.IsActive;
            storedSalary.UpdatedByUserId = employeeSalary.UpdatedByUserId;
            storedSalary.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();

            await this.storageBroker.SaveChangesAsync();

            return storedSalary;
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to update employee salary.", exception);
        }
    }

    public async ValueTask<EmployeeSalary> DeactivateEmployeeSalaryAsync(Guid employeeSalaryId, Guid? updatedByUserId)
    {
        try
        {
            ValidateId(employeeSalaryId, nameof(employeeSalaryId));

            EmployeeSalary? storedSalary = await this.storageBroker.EmployeeSalaries
                .FirstOrDefaultAsync(salary => salary.Id == employeeSalaryId);

            if (storedSalary is null)
            {
                throw new SalaryValidationException("Employee salary was not found.");
            }

            if (!storedSalary.IsActive)
            {
                return storedSalary;
            }

            DateTimeOffset now = this.dateTimeBroker.GetCurrentDateTimeOffset();

            storedSalary.IsActive = false;
            storedSalary.EffectiveToDate ??= DateOnly.FromDateTime(now.UtcDateTime);
            storedSalary.UpdatedByUserId = updatedByUserId;
            storedSalary.UpdatedDate = now;

            await this.storageBroker.SaveChangesAsync();

            return storedSalary;
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to deactivate employee salary.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<EmployeeSalary>> RetrieveEmployeeSalariesByEmployeeIdAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            return await this.storageBroker.EmployeeSalaries
                .AsNoTracking()
                .Where(salary => salary.EmployeeId == employeeId)
                .OrderByDescending(salary => salary.EffectiveFromDate)
                .ToListAsync();
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to retrieve salaries by employee.", exception);
        }
    }

    public async ValueTask<EmployeeSalary?> RetrieveActiveEmployeeSalaryByEmployeeIdAsync(Guid employeeId)
    {
        try
        {
            ValidateId(employeeId, nameof(employeeId));

            DateOnly today = DateOnly.FromDateTime(this.dateTimeBroker.GetCurrentDateTimeOffset().UtcDateTime);

            return await this.storageBroker.EmployeeSalaries
                .AsNoTracking()
                .Where(salary => salary.EmployeeId == employeeId && salary.IsActive)
                .Where(salary => salary.EffectiveFromDate <= today)
                .Where(salary => salary.EffectiveToDate == null || salary.EffectiveToDate >= today)
                .OrderByDescending(salary => salary.EffectiveFromDate)
                .FirstOrDefaultAsync();
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to retrieve active salary by employee.", exception);
        }
    }

    public async ValueTask<EmployeeSalary?> RetrieveEmployeeSalaryByIdAsync(Guid employeeSalaryId)
    {
        try
        {
            ValidateId(employeeSalaryId, nameof(employeeSalaryId));

            return await this.storageBroker.EmployeeSalaries
                .AsNoTracking()
                .FirstOrDefaultAsync(salary => salary.Id == employeeSalaryId);
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to retrieve employee salary by id.", exception);
        }
    }

    private async ValueTask ValidateEmployeeSalaryOnAddAsync(EmployeeSalary employeeSalary)
    {
        if (employeeSalary is null)
        {
            throw new SalaryValidationException("Employee salary is required.");
        }

        ValidateId(employeeSalary.EmployeeId, nameof(employeeSalary.EmployeeId));
        ValidateId(employeeSalary.SalaryStructureId, nameof(employeeSalary.SalaryStructureId));

        if (employeeSalary.Amount <= 0)
        {
            throw new SalaryValidationException("Base salary must be greater than zero.");
        }

        if (employeeSalary.EffectiveFromDate == default)
        {
            throw new SalaryValidationException("EffectiveFrom date is required.");
        }

        if (employeeSalary.EffectiveToDate.HasValue &&
            employeeSalary.EffectiveToDate.Value < employeeSalary.EffectiveFromDate)
        {
            throw new SalaryValidationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        bool employeeExists = await this.storageBroker.Employees
            .AnyAsync(employee => employee.Id == employeeSalary.EmployeeId && employee.IsActive);

        if (!employeeExists)
        {
            throw new SalaryValidationException("Employee was not found or inactive.");
        }

        bool salaryStructureExists = await this.storageBroker.SalaryStructures
            .AnyAsync(structure => structure.Id == employeeSalary.SalaryStructureId && structure.IsActive);

        if (!salaryStructureExists)
        {
            throw new SalaryValidationException("Salary structure was not found or inactive.");
        }
    }

    private async ValueTask ValidateEmployeeSalaryOnModifyAsync(EmployeeSalary employeeSalary)
    {
        await ValidateEmployeeSalaryOnAddAsync(employeeSalary);
        ValidateId(employeeSalary.Id, nameof(employeeSalary.Id));
    }

    private static void ValidateId(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new SalaryValidationException($"{name} is invalid.");
        }
    }
}
