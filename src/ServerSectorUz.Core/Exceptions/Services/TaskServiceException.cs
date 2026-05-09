namespace ServerSectorUz.Core.Exceptions.Services;

public class TaskServiceException : Exception
{
    public TaskServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}
