namespace ServerSectorUz.Core.Exceptions.Dependencies;

public class SalaryDependencyException : Exception
{
    public SalaryDependencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
