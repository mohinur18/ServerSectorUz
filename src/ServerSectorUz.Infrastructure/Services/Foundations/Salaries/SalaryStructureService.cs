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

public class SalaryStructureService : ISalaryStructureService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public SalaryStructureService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<SalaryStructure> AddSalaryStructureAsync(SalaryStructure salaryStructure)
    {
        try
        {
            ValidateSalaryStructureOnAdd(salaryStructure);

            bool exists = await this.storageBroker.SalaryStructures
                .AnyAsync(structure => structure.Name == salaryStructure.Name);

            if (exists)
            {
                throw new SalaryValidationException("Salary structure with same name already exists.");
            }

            salaryStructure.Id = Guid.NewGuid();
            salaryStructure.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            salaryStructure.IsActive = true;

            await this.storageBroker.SalaryStructures.AddAsync(salaryStructure);
            await this.storageBroker.SaveChangesAsync();

            return salaryStructure;
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryDependencyException("Failed to add salary structure.", exception);
        }
    }

    public IQueryable<SalaryStructure> RetrieveAllSalaryStructures() =>
        this.storageBroker.SalaryStructures.AsNoTracking().OrderBy(structure => structure.Name);

    public async ValueTask<SalaryStructure?> RetrieveSalaryStructureByIdAsync(Guid salaryStructureId)
    {
        try
        {
            ValidateId(salaryStructureId, nameof(salaryStructureId));

            return await this.storageBroker.SalaryStructures
                .AsNoTracking()
                .FirstOrDefaultAsync(structure => structure.Id == salaryStructureId);
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to retrieve salary structure by id.", exception);
        }
    }

    public async ValueTask<SalaryStructure> ModifySalaryStructureAsync(SalaryStructure salaryStructure)
    {
        try
        {
            ValidateSalaryStructureOnModify(salaryStructure);

            SalaryStructure? storedStructure = await this.storageBroker.SalaryStructures
                .FirstOrDefaultAsync(structure => structure.Id == salaryStructure.Id);

            if (storedStructure is null)
            {
                throw new SalaryValidationException("Salary structure was not found.");
            }

            bool duplicateName = await this.storageBroker.SalaryStructures
                .AnyAsync(structure => structure.Id != salaryStructure.Id && structure.Name == salaryStructure.Name);

            if (duplicateName)
            {
                throw new SalaryValidationException("Salary structure with same name already exists.");
            }

            storedStructure.Name = salaryStructure.Name.Trim();
            storedStructure.BaseAmount = salaryStructure.BaseAmount;
            storedStructure.Currency = salaryStructure.Currency.Trim();
            storedStructure.IsActive = salaryStructure.IsActive;
            storedStructure.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            storedStructure.UpdatedByUserId = salaryStructure.UpdatedByUserId;

            await this.storageBroker.SaveChangesAsync();

            return storedStructure;
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryServiceException("Failed to modify salary structure.", exception);
        }
    }

    public async ValueTask<SalaryStructure> RemoveSalaryStructureAsync(Guid salaryStructureId)
    {
        try
        {
            ValidateId(salaryStructureId, nameof(salaryStructureId));

            SalaryStructure? structure = await this.storageBroker.SalaryStructures
                .FirstOrDefaultAsync(storedStructure => storedStructure.Id == salaryStructureId);

            if (structure is null)
            {
                throw new SalaryValidationException("Salary structure was not found.");
            }

            bool isInUse = await this.storageBroker.EmployeeSalaries
                .AnyAsync(employeeSalary => employeeSalary.SalaryStructureId == salaryStructureId && employeeSalary.IsActive);

            if (isInUse)
            {
                throw new SalaryValidationException("Salary structure is in use and cannot be removed.");
            }

            this.storageBroker.SalaryStructures.Remove(structure);
            await this.storageBroker.SaveChangesAsync();

            return structure;
        }
        catch (SalaryValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new SalaryDependencyException("Failed to remove salary structure.", exception);
        }
    }

    private static void ValidateSalaryStructureOnAdd(SalaryStructure salaryStructure)
    {
        if (salaryStructure is null)
        {
            throw new SalaryValidationException("Salary structure is required.");
        }

        ValidateString(salaryStructure.Name, nameof(salaryStructure.Name));
        ValidateString(salaryStructure.Currency, nameof(salaryStructure.Currency));

        if (salaryStructure.BaseAmount <= 0)
        {
            throw new SalaryValidationException("Base salary must be greater than zero.");
        }

        salaryStructure.Name = salaryStructure.Name.Trim();
        salaryStructure.Currency = salaryStructure.Currency.Trim();
    }

    private static void ValidateSalaryStructureOnModify(SalaryStructure salaryStructure)
    {
        ValidateSalaryStructureOnAdd(salaryStructure);
        ValidateId(salaryStructure.Id, nameof(salaryStructure.Id));
    }

    private static void ValidateId(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new SalaryValidationException($"{name} is invalid.");
        }
    }

    private static void ValidateString(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new SalaryValidationException($"{name} is required.");
        }
    }
}
