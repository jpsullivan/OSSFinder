using System;
using Elmah;

namespace OSSFinder.Infrastructure.Logging
{
    internal static class QuietLog
    {
        public static void LogHandledException(Exception e)
        {
            try
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
            catch
            {
                // logging failed, don't allow exception to escape
            }
        }
    }
}