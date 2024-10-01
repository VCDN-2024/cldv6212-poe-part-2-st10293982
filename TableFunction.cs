using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public static class TableFunction
{
    [Function("AddOrderToTable")]
    public static async Task<IActionResult> AddOrderToTable(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Function, "post", Route = "table/add")] HttpRequest req,
        ILogger log)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableServiceClient = new TableServiceClient(connectionString);
        var tableClient = tableServiceClient.GetTableClient("Orders");

        await tableClient.CreateIfNotExistsAsync();

        var order = new TableEntity("OrdersPartition", Guid.NewGuid().ToString())
        {
            { "Customer_Id", "1" },
            { "Product_Id", "100" },
            { "Order_Location", "New York" },
            { "Order_Date", DateTime.UtcNow.ToString() }
        };

        await tableClient.AddEntityAsync(order);
        return new OkObjectResult("Order added.");
    }

    [Function("GetAllOrders")]
    public static async Task<IActionResult> GetAllOrders(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Function, "get", Route = "table/orders")] HttpRequest req,
        ILogger log)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableServiceClient = new TableServiceClient(connectionString);
        var tableClient = tableServiceClient.GetTableClient("Orders");

        var orders = tableClient.Query<TableEntity>();
        return new OkObjectResult(orders);
    }
}
