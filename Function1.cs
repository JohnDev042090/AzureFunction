using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using AzureFunctionText07152023.Model;

namespace AzureFunctionText07152023
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosURL = "https://cosmodbnosql07152023.documents.azure.com:443/";
            var cosmosKey = Environment.GetEnvironmentVariable("COSMOS_KEY");
            var databaseName = "CustomerDB";

            if (string.IsNullOrEmpty(cosmosURL) || string.IsNullOrEmpty(cosmosKey))
            {
                return new BadRequestObjectResult("Cosmos DB credentials are missing.");
            }

            try
            {
                CosmosClient client = new CosmosClient(cosmosURL, cosmosKey);
                Database db = await client.CreateDatabaseIfNotExistsAsync(databaseName);
                Container cont = await db.CreateContainerIfNotExistsAsync("Customer", "/id", 400);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var customer = JsonConvert.DeserializeObject<Customer>(requestBody);
                customer.id = Guid.NewGuid().ToString();

                var response = await cont.CreateItemAsync(customer);
                log.LogInformation("Saved Successfully");
                return new OkObjectResult("Saved Successfully!");
            }
            catch (Exception ex)
            {
                log.LogInformation("Failed: " + ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            
        }
    }
}
