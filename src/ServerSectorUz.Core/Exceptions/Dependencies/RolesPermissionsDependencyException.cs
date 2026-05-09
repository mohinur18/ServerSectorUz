namespace ServerSectorUz.Core.Exceptions.Dependencies;

public class RolesPermissionsDependencyException : Exception
{
    public RolesPermissionsDependencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
