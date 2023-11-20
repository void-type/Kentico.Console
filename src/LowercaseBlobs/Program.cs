using System.Diagnostics;
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

var startedTime = Stopwatch.GetTimestamp();
Console.WriteLine($"Started at {DateTimeOffset.Now}");

var blobCount = 0;

await foreach (BlobItem sourceBlobItem in container.GetBlobsAsync(prefix: prefix))
{
    blobCount++;

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

var elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - startedTime);

Console.WriteLine($"Finished in {elapsed} seconds at {DateTimeOffset.Now}. {blobCount} total blobs processed.");
