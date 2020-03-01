using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    public interface ITimeProvider
    {
        DateTime GetUtcDateTime();
    }
}
