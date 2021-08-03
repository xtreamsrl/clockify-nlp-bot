using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Bot.Data
{
    public class AzureBlobReader : IAzureBlobReader
    {
        private readonly IConfiguration _configuration;

        public AzureBlobReader(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string[] GetUserKeys()
        {
            string connectionString = _configuration.GetSection("BlobStorage")["DataConnectionString"];
            string containerName = _configuration.GetSection("BlobStorage")["ContainerName"];
            var container = CloudBlobContainerFactory.CreateCloudBlobContainer(connectionString, containerName);
            
            // TODO read with listblobsegmentedasync and build a generator.
            // Gets List of Blobs
            var blobs = container.ListBlobs();

            string[] userKeys = blobs.OfType<CloudBlockBlob>()
                .Select(b => Uri.UnescapeDataString(b.Name)) // unescape %2f in /
                .Where(IsUserState)
                .Where(IsNotEmulator)
                .ToArray();
            
            return userKeys;
        }

        private static bool IsUserState(string key)
        {
            return !key.Contains("conversations");
        }

        private static bool IsNotEmulator(string key)
        {
            return !key.Contains("emulator");
        }
    }
}