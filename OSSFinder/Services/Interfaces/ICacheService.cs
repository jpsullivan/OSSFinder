using System;

namespace OSSFinder.Services.Interfaces
{
    public interface ICacheService
    {
        object GetItem(string key);
        void SetItem(string key, object item, TimeSpan timeout);
        void RemoveItem(string key);
    }
}