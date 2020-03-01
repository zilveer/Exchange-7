using System;

namespace Util
{
    public class TimeProvider : ITimeProvider
    {
        public DateTime GetUtcDateTime()
        {
            return DateTime.UtcNow;
        }
    }
}
