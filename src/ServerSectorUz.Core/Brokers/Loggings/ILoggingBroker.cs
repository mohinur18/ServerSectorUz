namespace ServerSectorUz.Core.Brokers.Loggings;

public interface ILoggingBroker
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(Exception exception);
}
