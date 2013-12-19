namespace OSSFinder.Services.Interfaces
{
    public interface ICloudBlobClient
    {
        ICloudBlobContainer GetContainerReference(string containerAddress);
    }
}