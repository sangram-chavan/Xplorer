using System;

namespace WebSite.Core
{
    public interface ILogger
    {
        void log(string cmd, OrderedMap _logContext, Object result);
    }
}