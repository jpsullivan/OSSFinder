using System;

namespace OSSFinder.Infrastructure.Exceptions
{
    [Serializable]
    public class ReadOnlyModeException : Exception
    {
        public ReadOnlyModeException()
        { }

        public ReadOnlyModeException(string message)
            : base(message)
        {
        }

        public ReadOnlyModeException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}