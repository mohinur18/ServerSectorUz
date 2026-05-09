using ServerSectorUz.Core.Brokers.DateTimes;

namespace ServerSectorUz.Infrastructure.Brokers.DateTimes;

public class DateTimeBroker : IDateTimeBroker
{
    public DateTimeOffset GetCurrentDateTimeOffset() => DateTimeOffset.UtcNow;
}
