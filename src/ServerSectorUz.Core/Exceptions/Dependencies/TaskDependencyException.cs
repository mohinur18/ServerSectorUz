namespace ServerSectorUz.Core.Exceptions.Dependencies;

public class TaskDependencyException : Exception
{
    public TaskDependencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
