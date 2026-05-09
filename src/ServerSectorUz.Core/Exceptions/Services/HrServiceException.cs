namespace ServerSectorUz.Core.Exceptions.Services;

public class HrServiceException : Exception
{
    public HrServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}
