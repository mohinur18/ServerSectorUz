namespace ServerSectorUz.Core.Exceptions.Dependencies;

public class AttendanceDependencyException : Exception
{
    public AttendanceDependencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
