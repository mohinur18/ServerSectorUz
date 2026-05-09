namespace ServerSectorUz.Core.Brokers.DateTimes;

public interface IDateTimeBroker
{
    DateTimeOffset GetCurrentDateTimeOffset();
}
