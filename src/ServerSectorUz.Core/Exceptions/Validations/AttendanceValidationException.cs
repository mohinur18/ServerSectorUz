namespace ServerSectorUz.Core.Exceptions.Validations;

public class AttendanceValidationException : Exception
{
    public AttendanceValidationException(string message)
        : base(message) { }
}
