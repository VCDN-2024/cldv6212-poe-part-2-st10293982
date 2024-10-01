using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

public static class FileShareFunction
{
    [Function("UploadToFileShare")]
    public static async Task<IActionResult> UploadToFileShare(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Function, "post", Route = "fileshare/upload")] HttpRequest req)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string shareName = "myfileshare";
        string fileName = req.Form.Files["file"].FileName;

        var fileShareClient = new ShareClient(connectionString, shareName);
        await fileShareClient.CreateIfNotExistsAsync();

        var directoryClient = fileShareClient.GetRootDirectoryClient();
        var fileClient = directoryClient.GetFileClient(fileName);

        using var stream = req.Form.Files["file"].OpenReadStream();
        await fileClient.CreateAsync(stream.Length);
        await fileClient.UploadRangeAsync(new Azure.HttpRange(0, stream.Length), stream);

        return new OkObjectResult($"File {fileName} uploaded to file share.");
    }
}
