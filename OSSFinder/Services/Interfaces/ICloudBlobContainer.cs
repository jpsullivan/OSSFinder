using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OSSFinder.Services.Interfaces
{
    public interface ICloudBlobContainer
    {
        Task CreateIfNotExistAsync();
        Task SetPermissionsAsync(BlobContainerPermissions permissions);
        ISimpleCloudBlob GetBlobReference(string blobAddressUri);
    }
}
