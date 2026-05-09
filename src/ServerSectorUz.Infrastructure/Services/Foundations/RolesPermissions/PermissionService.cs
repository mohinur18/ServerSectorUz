using Microsoft.EntityFrameworkCore;
using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Exceptions.Dependencies;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Foundations.RolesPermissions;
using ServerSectorUz.Core.Services.Foundations.RolesPermissions;

namespace ServerSectorUz.Infrastructure.Services.Foundations.RolesPermissions;

public class PermissionService : IPermissionService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public PermissionService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<Permission> AddPermissionAsync(Permission permission)
    {
        try
        {
            ValidatePermissionOnAdd(permission);

            bool exists = await this.storageBroker.Permissions
                .AnyAsync(storedPermission => storedPermission.Code == permission.Code);

            if (exists)
            {
                throw new RolesPermissionsValidationException("Permission with this code already exists.");
            }

            permission.Id = Guid.NewGuid();
            permission.CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            permission.UpdatedDate = null;
            permission.IsActive = true;

            await this.storageBroker.Permissions.AddAsync(permission);
            await this.storageBroker.SaveChangesAsync();

            return permission;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsDependencyException("Failed to add permission.", exception);
        }
    }

    public IQueryable<Permission> RetrieveAllPermissions() =>
        this.storageBroker.Permissions.AsNoTracking().OrderBy(permission => permission.Name);

    public async ValueTask<Permission?> RetrievePermissionByIdAsync(Guid permissionId)
    {
        try
        {
            ValidateId(permissionId, nameof(permissionId));

            return await this.storageBroker.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(permission => permission.Id == permissionId);
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsServiceException("Failed to retrieve permission by id.", exception);
        }
    }

    public async ValueTask<Permission> ModifyPermissionAsync(Permission permission)
    {
        try
        {
            ValidatePermissionOnModify(permission);

            Permission? storedPermission = await this.storageBroker.Permissions
                .FirstOrDefaultAsync(foundPermission => foundPermission.Id == permission.Id);

            if (storedPermission is null)
            {
                throw new RolesPermissionsValidationException("Permission not found.");
            }

            storedPermission.Code = permission.Code.Trim();
            storedPermission.Name = permission.Name.Trim();
            storedPermission.Description = permission.Description.Trim();
            storedPermission.IsActive = permission.IsActive;
            storedPermission.UpdatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset();
            storedPermission.UpdatedByUserId = permission.UpdatedByUserId;

            await this.storageBroker.SaveChangesAsync();

            return storedPermission;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsServiceException("Failed to modify permission.", exception);
        }
    }

    public async ValueTask<Permission> RemovePermissionAsync(Guid permissionId)
    {
        try
        {
            ValidateId(permissionId, nameof(permissionId));

            Permission? permission = await this.storageBroker.Permissions
                .FirstOrDefaultAsync(storedPermission => storedPermission.Id == permissionId);

            if (permission is null)
            {
                throw new RolesPermissionsValidationException("Permission not found.");
            }

            this.storageBroker.Permissions.Remove(permission);
            await this.storageBroker.SaveChangesAsync();

            return permission;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsDependencyException("Failed to remove permission.", exception);
        }
    }

    public async ValueTask<RolePermission> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        try
        {
            ValidateId(roleId, nameof(roleId));
            ValidateId(permissionId, nameof(permissionId));

            bool roleExists = await this.storageBroker.Roles
                .AnyAsync(role => role.Id == roleId && role.IsActive);

            if (!roleExists)
            {
                throw new RolesPermissionsValidationException("Role was not found or inactive.");
            }

            bool permissionExists = await this.storageBroker.Permissions
                .AnyAsync(permission => permission.Id == permissionId && permission.IsActive);

            if (!permissionExists)
            {
                throw new RolesPermissionsValidationException("Permission was not found or inactive.");
            }

            RolePermission? existing = await this.storageBroker.RolePermissions
                .FirstOrDefaultAsync(rolePermission =>
                    rolePermission.RoleId == roleId && rolePermission.PermissionId == permissionId);

            if (existing is not null)
            {
                return existing;
            }

            var rolePermission = new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                IsActive = true,
                CreatedDate = this.dateTimeBroker.GetCurrentDateTimeOffset()
            };

            await this.storageBroker.RolePermissions.AddAsync(rolePermission);
            await this.storageBroker.SaveChangesAsync();

            return rolePermission;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsDependencyException("Failed to assign permission to role.", exception);
        }
    }

    public async ValueTask<IReadOnlyList<Permission>> RetrievePermissionsByRoleIdAsync(Guid roleId)
    {
        try
        {
            ValidateId(roleId, nameof(roleId));

            List<Permission> permissions = await (
                from rolePermission in this.storageBroker.RolePermissions.AsNoTracking()
                join permission in this.storageBroker.Permissions.AsNoTracking()
                    on rolePermission.PermissionId equals permission.Id
                where rolePermission.RoleId == roleId && rolePermission.IsActive && permission.IsActive
                orderby permission.Name
                select permission)
                .Distinct()
                .ToListAsync();

            return permissions;
        }
        catch (RolesPermissionsValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            this.loggingBroker.LogError(exception);
            throw new RolesPermissionsServiceException("Failed to retrieve role permissions.", exception);
        }
    }

    private static void ValidatePermissionOnAdd(Permission permission)
    {
        if (permission is null)
        {
            throw new RolesPermissionsValidationException("Permission is required.");
        }

        ValidateString(permission.Code, nameof(permission.Code));
        ValidateString(permission.Name, nameof(permission.Name));
        ValidateString(permission.Description, nameof(permission.Description));

        permission.Code = permission.Code.Trim();
        permission.Name = permission.Name.Trim();
        permission.Description = permission.Description.Trim();
    }

    private static void ValidatePermissionOnModify(Permission permission)
    {
        ValidatePermissionOnAdd(permission);
        ValidateId(permission.Id, nameof(permission.Id));
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
