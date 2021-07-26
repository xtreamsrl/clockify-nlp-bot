using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Bot.Factories
{
    public static class CloudBlobContainerFactory
    {
        public static CloudBlobContainer CreateCloudBlobContainer(string connectionString, string containerName)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container;
        }
    }
}