using ServerSectorUz.Core.Models.Foundations.Salaries;

namespace ServerSectorUz.Core.Services.Foundations.Salaries;

public interface ISalaryStructureService
{
    ValueTask<SalaryStructure> AddSalaryStructureAsync(SalaryStructure salaryStructure);
    IQueryable<SalaryStructure> RetrieveAllSalaryStructures();
    ValueTask<SalaryStructure?> RetrieveSalaryStructureByIdAsync(Guid salaryStructureId);
    ValueTask<SalaryStructure> ModifySalaryStructureAsync(SalaryStructure salaryStructure);
    ValueTask<SalaryStructure> RemoveSalaryStructureAsync(Guid salaryStructureId);
}
