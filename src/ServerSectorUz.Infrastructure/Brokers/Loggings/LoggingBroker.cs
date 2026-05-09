using Microsoft.Extensions.Logging;
using ServerSectorUz.Core.Brokers.Loggings;

namespace ServerSectorUz.Infrastructure.Brokers.Loggings;

public class LoggingBroker : ILoggingBroker
{
    private readonly ILogger logger;

    public LoggingBroker(ILoggerFactory loggerFactory) =>
        this.logger = loggerFactory.CreateLogger(nameof(LoggingBroker));

    public void LogInformation(string message) =>
        this.logger.LogInformation(message);

    public void LogWarning(string message) =>
        this.logger.LogWarning(message);

    public void LogError(Exception exception) =>
        this.logger.LogError(exception, exception.Message);
}
