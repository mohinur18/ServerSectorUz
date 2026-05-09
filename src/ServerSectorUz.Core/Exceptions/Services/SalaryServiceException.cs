namespace ServerSectorUz.Core.Exceptions.Services;

public class SalaryServiceException : Exception
{
    public SalaryServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}
