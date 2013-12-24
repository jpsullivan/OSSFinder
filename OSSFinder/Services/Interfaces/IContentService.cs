using System;
using System.Threading.Tasks;
using System.Web;

namespace OSSFinder.Services.Interfaces
{
    public interface IContentService
    {
        Task<IHtmlString> GetContentItemAsync(string name, TimeSpan expiresIn);
    }
}
