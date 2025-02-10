using Core.Ports;

namespace Infrastructure;

public class SystemDateTimeProvider : DateTimeProvider
{
    public DateTime Now()
    {
        return DateTime.UtcNow;
    }
}
