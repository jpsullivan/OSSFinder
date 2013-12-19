using System;

namespace OSSFinder.Core.Entities
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