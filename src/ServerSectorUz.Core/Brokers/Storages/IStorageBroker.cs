using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Models.Foundations.Attendance;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Models.Foundations.Expenses;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Models.Foundations.RolesPermissions;
using ServerSectorUz.Core.Models.Foundations.Salaries;
using ServerSectorUz.Core.Models.Foundations.Tasks;

namespace ServerSectorUz.Core.Brokers.Storages;

public interface IStorageBroker
{
    DbSet<User> Users { get; set; }
    DbSet<UserCredential> UserCredentials { get; set; }
    DbSet<UserSession> UserSessions { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<Permission> Permissions { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<RolePermission> RolePermissions { get; set; }
    DbSet<Employee> Employees { get; set; }
    DbSet<Department> Departments { get; set; }
    DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    DbSet<TaskItem> TaskItems { get; set; }
    DbSet<SalaryStructure> SalaryStructures { get; set; }
    DbSet<EmployeeSalary> EmployeeSalaries { get; set; }
    DbSet<ExpenseClaim> ExpenseClaims { get; set; }
    DbSet<ExpenseItem> ExpenseItems { get; set; }

    ValueTask<int> SaveChangesAsync();
}
