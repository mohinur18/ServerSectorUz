namespace ServerSectorUz.Core.Exceptions.Validations;

public class HrValidationException : Exception
{
    public HrValidationException(string message)
        : base(message) { }
}
