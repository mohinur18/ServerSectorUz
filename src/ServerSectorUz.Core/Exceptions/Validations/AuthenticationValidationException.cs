namespace ServerSectorUz.Core.Exceptions.Validations;

public class AuthenticationValidationException : Exception
{
    public AuthenticationValidationException(string message)
        : base(message) { }
}
