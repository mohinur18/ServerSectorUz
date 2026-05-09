using ServerSectorUz.Core.Models.Foundations.HR;

namespace ServerSectorUz.Core.Services.Foundations.HR;

public interface IEmployeeService
{
    ValueTask<Employee> AddEmployeeAsync(Employee employee);
    IQueryable<Employee> RetrieveAllEmployees();
    ValueTask<Employee?> RetrieveEmployeeByIdAsync(Guid employeeId);
    ValueTask<Employee?> RetrieveEmployeeByUserIdAsync(Guid userId);
    ValueTask<Employee> ModifyEmployeeAsync(Employee employee);
    ValueTask<Employee> RemoveEmployeeAsync(Guid employeeId);
}
