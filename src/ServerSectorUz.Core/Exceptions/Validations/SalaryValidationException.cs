namespace ServerSectorUz.Core.Exceptions.Validations;

public class SalaryValidationException : Exception
{
    public SalaryValidationException(string message)
        : base(message) { }
}
