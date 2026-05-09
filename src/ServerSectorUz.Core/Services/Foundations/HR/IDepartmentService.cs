using ServerSectorUz.Core.Models.Foundations.HR;

namespace ServerSectorUz.Core.Services.Foundations.HR;

public interface IDepartmentService
{
    ValueTask<Department> AddDepartmentAsync(Department department);
    IQueryable<Department> RetrieveAllDepartments();
    ValueTask<Department?> RetrieveDepartmentByIdAsync(Guid departmentId);
    ValueTask<Department> ModifyDepartmentAsync(Department department);
    ValueTask<Department> RemoveDepartmentAsync(Guid departmentId);
}
