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
            var cosmosKey = "Bz4Ld1RMEhtlrRDsnxSxBA0duoMRI0cXTS6Dt17ghQyg7wCTljsiR20HEltYUHLqK5CDSiDZiNhYACDbGkMHGQ==";
            var databaseName = "CustomerDB";

            try
            {
                Customer cust = new Customer();
                CosmosClient client = new CosmosClient(cosmosURL, cosmosKey);
                Database db = await client.CreateDatabaseIfNotExistsAsync(databaseName);
                Container cont = await db.CreateContainerIfNotExistsAsync(
                    "Customer", "/id", 400);

                cust = new Customer()
                {
                    id = Guid.NewGuid().ToString(),
                    FirstName = "Mark",
                    LastName = "Monurst",
                    BirthdayInEpoch = 125566,
                    Email = "mark@gmail.com"
                };

                //dynamic customer = new
                //{
                //    id = Guid.NewGuid().ToString(),
                //    partitionKeyPath = "Mark",
                //    LastName = "Monurst",
                //    BirthdayInEpoch = 125566,
                //    Email = "mark@gmail.com"
                //};

                var response = await cont.CreateItemAsync(cust);
                log.LogInformation("Saved Successfully");
                return new OkObjectResult("Saved Successfully!");
            }
            catch (Exception ex)
            {
                log.LogInformation("Failed: " + ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";
            
        }
    }
}
