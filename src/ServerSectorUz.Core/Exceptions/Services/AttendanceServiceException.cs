namespace ServerSectorUz.Core.Exceptions.Services;

public class AttendanceServiceException : Exception
{
    public AttendanceServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}
