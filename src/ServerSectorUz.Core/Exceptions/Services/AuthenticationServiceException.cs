namespace ServerSectorUz.Core.Exceptions.Services;

public class AuthenticationServiceException : Exception
{
    public AuthenticationServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}
