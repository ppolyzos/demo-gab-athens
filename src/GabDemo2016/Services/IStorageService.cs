using System;
using System.Threading.Tasks;
using ImageResizer.ExtensionMethods;
using Microsoft.Extensions.OptionsModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GabDemo2016.Services
{
    public enum StorageContainer
    {
        Photos,
        Faces
    }

    public interface IStorageService
    {
        Task<byte[]> GetItem(Uri url);
        Task<ICloudBlob> UploadStream(byte[] input, StorageContainer storageContainer, string blobName);
        void Clear(StorageContainer storageContainer);
    }

    public class CloudStorageService : IStorageService
    {
        private readonly CloudBlobClient _cloudBlobClient;

        public CloudStorageService(IOptions<AzureStorageSettings> options)
        {
            var storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            _cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }

        public async Task<byte[]> GetItem(Uri url)
        {
            var blob = await _cloudBlobClient.GetBlobReferenceFromServerAsync(url);
            var stream = await blob.OpenReadAsync();
            return stream.CopyToBytes();
        }

        public async Task<ICloudBlob> UploadStream(byte[] inputArray, StorageContainer storageContainer, string blobName)
        {
            var blobContainer = _cloudBlobClient.GetContainerReference(storageContainer.ToString().ToLowerInvariant());
            await blobContainer.CreateIfNotExistsAsync();

            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });

            var blob = blobContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = "image/jpg";

            await blob.UploadFromByteArrayAsync(inputArray, 0, inputArray.Length);
            return blob;
        }

        public void Clear(StorageContainer storageContainer)
        {
            var blobContainer = _cloudBlobClient.GetContainerReference(storageContainer.ToString().ToLowerInvariant());
            Parallel.ForEach(blobContainer.ListBlobs(), async x => await ((CloudBlob)x).DeleteIfExistsAsync());
        }
    }
}