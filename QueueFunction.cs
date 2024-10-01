using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.Http;

public static class QueueFunction
{
    [Function("SendMessageToQueue")]
    public static async Task SendMessageToQueue(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Function, "post", Route = "queue/send")] HttpRequest req,
        ILogger log)
    {
        string messageContent = await req.ReadAsStringAsync();
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string queueName = "orders";

        var queueClient = new QueueClient(connectionString, queueName);
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(messageContent);
    }
}
