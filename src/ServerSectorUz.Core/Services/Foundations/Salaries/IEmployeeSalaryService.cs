using ServerSectorUz.Core.Models.Foundations.Salaries;

namespace ServerSectorUz.Core.Services.Foundations.Salaries;

public interface IEmployeeSalaryService
{
    ValueTask<EmployeeSalary> AddEmployeeSalaryAsync(EmployeeSalary employeeSalary);
    ValueTask<EmployeeSalary> ModifyEmployeeSalaryAsync(EmployeeSalary employeeSalary);
    ValueTask<EmployeeSalary> DeactivateEmployeeSalaryAsync(Guid employeeSalaryId, Guid? updatedByUserId);

    ValueTask<IReadOnlyList<EmployeeSalary>> RetrieveEmployeeSalariesByEmployeeIdAsync(Guid employeeId);
    ValueTask<EmployeeSalary?> RetrieveActiveEmployeeSalaryByEmployeeIdAsync(Guid employeeId);
    ValueTask<EmployeeSalary?> RetrieveEmployeeSalaryByIdAsync(Guid employeeSalaryId);
}
