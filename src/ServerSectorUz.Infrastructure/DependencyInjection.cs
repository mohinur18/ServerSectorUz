using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Securities;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Brokers.Tokens;
using ServerSectorUz.Core.Models.Configurations;
using ServerSectorUz.Core.Services.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Services.Foundations.Attendance;
using ServerSectorUz.Core.Services.Foundations.HR;
using ServerSectorUz.Core.Services.Foundations.RolesPermissions;
using ServerSectorUz.Core.Services.Foundations.Salaries;
using ServerSectorUz.Core.Services.Foundations.Tasks;
using ServerSectorUz.Core.Services.Orchestrations.AuthenticationUsers;
using ServerSectorUz.Infrastructure.Brokers.Securities;
using ServerSectorUz.Infrastructure.Brokers.DateTimes;
using ServerSectorUz.Infrastructure.Brokers.Loggings;
using ServerSectorUz.Infrastructure.Brokers.Storages;
using ServerSectorUz.Infrastructure.Brokers.Tokens;
using ServerSectorUz.Infrastructure.Services.Foundations.AuthenticationUsers;
using ServerSectorUz.Infrastructure.Services.Foundations.Attendance;
using ServerSectorUz.Infrastructure.Services.Foundations.HR;
using ServerSectorUz.Infrastructure.Services.Foundations.RolesPermissions;
using ServerSectorUz.Infrastructure.Services.Foundations.Salaries;
using ServerSectorUz.Infrastructure.Services.Foundations.Tasks;
using ServerSectorUz.Infrastructure.Services.Orchestrations.AuthenticationUsers;

namespace ServerSectorUz.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddDbContext<StorageBroker>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IStorageBroker>(provider =>
            provider.GetRequiredService<StorageBroker>());

        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<IPasswordBroker, PasswordBroker>();
        services.AddTransient<ITokenBroker, TokenBroker>();

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IUserCredentialService, UserCredentialService>();
        services.AddTransient<IUserSessionService, UserSessionService>();
        services.AddTransient<IAttendanceRecordService, AttendanceRecordService>();
        services.AddTransient<IDepartmentService, DepartmentService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IRoleService, RoleService>();
        services.AddTransient<IPermissionService, PermissionService>();
        services.AddTransient<ISalaryStructureService, SalaryStructureService>();
        services.AddTransient<IEmployeeSalaryService, EmployeeSalaryService>();
        services.AddTransient<ITaskItemService, TaskItemService>();
        services.AddTransient<IAuthenticationOrchestrationService, AuthenticationOrchestrationService>();

        return services;
    }
}
