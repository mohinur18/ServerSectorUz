namespace ServerSectorUz.Core.Exceptions.Services;

public class RolesPermissionsServiceException : Exception
{
    public RolesPermissionsServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}
