using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Models.Foundations;
using ServerSectorUz.Core.Models.Foundations.Attendance;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Models.Foundations.Expenses;
using ServerSectorUz.Core.Models.Foundations.HR;
using ServerSectorUz.Core.Models.Foundations.RolesPermissions;
using ServerSectorUz.Core.Models.Foundations.Salaries;
using ServerSectorUz.Core.Models.Foundations.Tasks;

namespace ServerSectorUz.Infrastructure.Brokers.Storages;

public class StorageBroker : DbContext, IStorageBroker
{
    public StorageBroker(DbContextOptions<StorageBroker> options)
        : base(options)
    { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserCredential> UserCredentials { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<SalaryStructure> SalaryStructures { get; set; }
    public DbSet<EmployeeSalary> EmployeeSalaries { get; set; }
    public DbSet<ExpenseClaim> ExpenseClaims { get; set; }
    public DbSet<ExpenseItem> ExpenseItems { get; set; }

    public async ValueTask<int> SaveChangesAsync() =>
        await base.SaveChangesAsync();

    public async ValueTask<ExpenseItem> InsertExpenseItemAsync(
        ExpenseItem expenseItem)
    {
        EntityEntry<ExpenseItem> expenseItemEntityEntry =
            await this.ExpenseItems.AddAsync(expenseItem);

        await this.SaveChangesAsync();

        return expenseItemEntityEntry.Entity;
    }

    public IQueryable<ExpenseItem> SelectAllExpenseItems() =>
        this.ExpenseItems;

    public async ValueTask<ExpenseItem> SelectExpenseItemByIdAsync(
        Guid expenseItemId) =>
            await this.ExpenseItems.FindAsync(expenseItemId);

    public async ValueTask<ExpenseItem> UpdateExpenseItemAsync(
        ExpenseItem expenseItem)
    {
        EntityEntry<ExpenseItem> expenseItemEntityEntry =
            this.ExpenseItems.Update(expenseItem);

        await this.SaveChangesAsync();

        return expenseItemEntityEntry.Entity;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBaseEntity(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<UserCredential>().ToTable("UserCredentials");
        modelBuilder.Entity<UserSession>().ToTable("UserSessions");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<Permission>().ToTable("Permissions");
        modelBuilder.Entity<UserRole>().ToTable("UserRoles");
        modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");
        modelBuilder.Entity<Employee>().ToTable("Employees");
        modelBuilder.Entity<Department>().ToTable("Departments");
        modelBuilder.Entity<AttendanceRecord>().ToTable("AttendanceRecords");
        modelBuilder.Entity<TaskItem>().ToTable("TaskItems");
        modelBuilder.Entity<SalaryStructure>().ToTable("SalaryStructures");
        modelBuilder.Entity<EmployeeSalary>().ToTable("EmployeeSalaries");
        modelBuilder.Entity<ExpenseClaim>().ToTable("ExpenseClaims");
        modelBuilder.Entity<ExpenseItem>().ToTable("ExpenseItems");
        modelBuilder.Entity<ExpenseClaim>()
    .Property(expenseClaim => expenseClaim.TotalAmount)
    .HasPrecision(18, 2);

        modelBuilder.Entity<ExpenseItem>()
            .Property(expenseItem => expenseItem.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<UserCredential>()
            .HasIndex(userCredential => userCredential.UserId)
            .IsUnique();

        modelBuilder.Entity<UserCredential>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<UserCredential>(userCredential => userCredential.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSession>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(userSession => userSession.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasIndex(role => role.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(permission => permission.Code)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
            .IsUnique();

        modelBuilder.Entity<RolePermission>()
            .HasIndex(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne<Permission>()
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Department>()
            .HasIndex(department => department.Name)
            .IsUnique();

        modelBuilder.Entity<Department>()
            .HasIndex(department => department.Code)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.EmployeeNumber)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasOne<Department>()
            .WithMany()
            .HasForeignKey(employee => employee.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(employee => employee.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(record => new { record.EmployeeId, record.WorkDate });

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne<Employee>()
            .WithMany()
            .HasForeignKey(record => record.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(task => task.Status);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(task => new { task.AssignedEmployeeId, task.Status });

        modelBuilder.Entity<TaskItem>()
            .HasOne<Employee>()
            .WithMany()
            .HasForeignKey(task => task.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalaryStructure>()
            .HasIndex(structure => structure.Name)
            .IsUnique();

        modelBuilder.Entity<EmployeeSalary>()
            .HasIndex(salary => new { salary.EmployeeId, salary.IsActive });

        modelBuilder.Entity<EmployeeSalary>()
            .HasOne<Employee>()
            .WithMany()
            .HasForeignKey(salary => salary.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeSalary>()
            .HasOne<SalaryStructure>()
            .WithMany()
            .HasForeignKey(salary => salary.SalaryStructureId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
    public async ValueTask<ExpenseClaim> InsertExpenseClaimAsync(
    ExpenseClaim expenseClaim)
    {
        EntityEntry<ExpenseClaim> expenseClaimEntityEntry =
            await this.ExpenseClaims.AddAsync(expenseClaim);

        await this.SaveChangesAsync();

        return expenseClaimEntityEntry.Entity;
    }

    public IQueryable<ExpenseClaim> SelectAllExpenseClaims() =>
        this.ExpenseClaims;

    public async ValueTask<ExpenseClaim> SelectExpenseClaimByIdAsync(
        Guid expenseClaimId) =>
            await this.ExpenseClaims.FindAsync(expenseClaimId);

    public async ValueTask<ExpenseClaim> UpdateExpenseClaimAsync(
        ExpenseClaim expenseClaim)
    {
        EntityEntry<ExpenseClaim> expenseClaimEntityEntry =
            this.ExpenseClaims.Update(expenseClaim);

        await this.SaveChangesAsync();

        return expenseClaimEntityEntry.Entity;
    }

    private static void ConfigureBaseEntity(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType).HasKey(nameof(BaseEntity.Id));
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.RowVersion))
                .IsRowVersion();

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.CreatedDate))
                .IsRequired();

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.IsActive))
                .IsRequired();
        }
    }
}
