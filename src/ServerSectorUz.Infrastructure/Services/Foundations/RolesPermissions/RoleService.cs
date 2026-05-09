using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.AuthenticationUsers;
using ServerSectorUz.Core.Models.Foundations.RolesPermissions;
using ServerSectorUz.Core.Services.Foundations.RolesPermissions;

namespace ServerSectorUz.Infrastructure.Services.Foundations.RolesPermissions;

public class RoleService : IRoleService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public RoleService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<Role> AddRoleAsync(Role role)
    {
        try
        {
            ValidateRoleOnAdd(role);

            bool roleExists = await this.storageBroker.Roles
                .AnyAsync(storedRole => storedRole.Name == role.Name);

            if (roleExists)
            {
                throw new RolesPermissionsValidationException("Role with this name already exists.");
            }

            role.Id = Guid.NewGuid();
            role.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            role.UpdatedDate = null;
            role.IsActive = true;

            await this.storageBroker.Roles.AddAsync(role);
            await this.storageBroker.SaveChangesAsync();

            return role;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsDependencyException("Failed to add role.", exception);
        }
    }

    public IQueryable<Role> RetrieveAllRoles() =>
        this.storageBroker.Roles.AsNoTracking().OrderBy(role => role.Name);

    public async ValueTask<Role?> RetrieveRoleByIdAsync(Guid roleId)
    {
        try
        {
            ValidateId(roleId, nameof(roleId));

            return await this.storageBroker.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(role => role.Id == roleId);
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsServiceException("Failed to retrieve role by id.", exception);
        }
    }

    public async ValueTask<Role> ModifyRoleAsync(Role role)
    {
        try
        {
            ValidateRoleOnModify(role);

            Role? storageRole = await this.storageBroker.Roles
                .FirstOrDefaultAsync(storedRole => storedRole.Id == role.Id);

            if (storageRole is null)
            {
                throw new RolesPermissionsValidationException("Role not found.");
            }

            storageRole.Name = role.Name.Trim();
            storageRole.Description = role.Description.Trim();
            storageRole.IsActive = role.IsActive;
            storageRole.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            storageRole.UpdatedByUserId = role.UpdatedByUserId;

            await this.storageBroker.SaveChangesAsync();

            return storageRole;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsServiceException("Failed to modify role.", exception);
        }
    }

    public async ValueTask<Role> RemoveRoleAsync(Guid roleId)
    {
        try
        {
            ValidateId(roleId, nameof(roleId));

            Role? role = await this.storageBroker.Roles
                .FirstOrDefaultAsync(storedRole => storedRole.Id == roleId);

            if (role is null)
            {
                throw new RolesPermissionsValidationException("Role not found.");
            }

            this.storageBroker.Roles.Remove(role);
            await this.storageBroker.SaveChangesAsync();

            return role;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsDependencyException("Failed to remove role.", exception);
        }
    }

    public async ValueTask<UserRole> AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        try
        {
            ValidateId(userId, nameof(userId));
            ValidateId(roleId, nameof(roleId));

            bool userExists = await this.storageBroker.Users
                .AnyAsync(user => user.Id == userId && user.IsActive);

            if (!userExists)
            {
                throw new RolesPermissionsValidationException("User was not found or inactive.");
            }

            bool roleExists = await this.storageBroker.Roles
                .AnyAsync(role => role.Id == roleId && role.IsActive);

            if (!roleExists)
            {
                throw new RolesPermissionsValidationException("Role was not found or inactive.");
            }

            UserRole? existingAssignment = await this.storageBroker.UserRoles
                .FirstOrDefaultAsync(userRole => userRole.UserId == userId && userRole.RoleId == roleId);

            if (existingAssignment is not null)
            {
                return existingAssignment;
            }

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                IsActive = true,
                CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset()
            };

            await this.storageBroker.UserRoles.AddAsync(userRole);
            await this.storageBroker.SaveChangesAsync();

            return userRole;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsDependencyException("Failed to assign role to user.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<Role>> RetrieveRolesByUserIdAsync(Guid userId)
    {
        try
        {
            ValidateId(userId, nameof(userId));

            List<Role> roles = await (
                from userRole in this.storageBroker.UserRoles.AsNoTracking()
                join role in this.storageBroker.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == userId && userRole.IsActive && role.IsActive
                orderby role.Name
                select role)
                .Distinct()
                .ToListAsync();

            return roles;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsServiceException("Failed to retrieve user roles.", exception);
        }
    }

    private static void ValidateRoleOnAdd(Role role)
    {
        if (role is null)
        {
            throw new RolesPermissionsValidationException("Role is required.");
        }

        ValidateString(role.Name, nameof(role.Name));
        ValidateString(role.Description, nameof(role.Description));

        role.Name = role.Name.Trim();
        role.Description = role.Description.Trim();
    }

    private static void ValidateRoleOnModify(Role role)
    {
        ValidateRoleOnAdd(role);
        ValidateId(role.Id, nameof(role.Id));
    }

    private static void ValidateId(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new RolesPermissionsValidationException($"{name} is invalid.");
        }
    }

    private static void ValidateString(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RolesPermissionsValidationException($"{name} is required.");
        }
    }
}
