namespace ServerSectorUz.Core.Exceptions.Dependencies;

public class AuthenticationDependencyException : Exception
{
    public AuthenticationDependencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
