namespace ServerSectorUz.Core.Exceptions.Validations;

public class TaskValidationException : Exception
{
    public TaskValidationException(string message)
        : base(message) { }
}
