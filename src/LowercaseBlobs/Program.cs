using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var blobConfig = config.GetSection("BlobStorage");
var storageConnectionString = blobConfig.GetValue<string>("ConnectionString") ?? throw new ArgumentNullException("ConnectionString");
var containerName = blobConfig.GetValue<string>("Container") ?? throw new ArgumentNullException("Container");
var prefix = blobConfig.GetValue<string>("Prefix") ?? throw new ArgumentNullException("Prefix");

var service = new BlobServiceClient(storageConnectionString);
var container = service.GetBlobContainerClient(containerName);

Console.WriteLine("Started!");

await foreach (BlobItem sourceBlobItem in container.GetBlobsAsync(prefix: prefix))
{
    var lowerName = sourceBlobItem.Name.ToLower();

    if (sourceBlobItem.Name != lowerName)
    {
        Console.WriteLine($"Renaming {sourceBlobItem.Name} to {lowerName}");

        var sourceBlob = container.GetBlobClient(sourceBlobItem.Name);

        var destBlob = container.GetBlobClient(lowerName);

        await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

        var destProperties = await destBlob.GetPropertiesAsync();

        while (destProperties.Value.BlobCopyStatus == CopyStatus.Pending)
        {
            await Task.Delay(100);
            destProperties = await destBlob.GetPropertiesAsync();
        }

        if (destProperties.Value.BlobCopyStatus != CopyStatus.Success)
        {
            throw new Exception("Rename failed: " + destProperties.Value.BlobCopyStatus);
        }

        await sourceBlob.DeleteAsync();
    }
}

Console.WriteLine("Done!");
