using System.IO;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

public static class BlobFunction
{
    [Function("UploadFileToBlob")]
    public static async Task<IActionResult> UploadFileToBlob(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Function, "post", Route = "upload")] HttpRequest req,
        ILogger log)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string containerName = "uploads";
        string fileName = req.Form.Files["file"]?.FileName;

        if (fileName == null)
        {
            return new BadRequestObjectResult("No file uploaded.");
        }

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);
        using (var stream = req.Form.Files["file"].OpenReadStream())
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        return new OkObjectResult($"File {fileName} uploaded to blob storage.");
    }

    [Function("DownloadFileFromBlob")]
    public static async Task<IActionResult> DownloadFileFromBlob(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Function, "get", Route = "download/{fileName}")] HttpRequest req,
        string fileName, ILogger log)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string containerName = "uploads";

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return new NotFoundObjectResult($"File {fileName} not found.");
        }

        var blobStream = await blobClient.OpenReadAsync();
        return new FileStreamResult(blobStream, "application/octet-stream")
        {
            FileDownloadName = fileName
        };
    }
}
