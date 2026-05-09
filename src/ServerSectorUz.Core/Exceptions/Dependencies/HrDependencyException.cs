namespace ServerSectorUz.Core.Exceptions.Dependencies;

public class HrDependencyException : Exception
{
    public HrDependencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
