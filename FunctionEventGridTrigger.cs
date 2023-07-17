// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using AzureFunctionText07152023.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
//using System.Text.Json;

namespace AzureFunctionSaveCustomer
{
    public static class FunctionEventGridTrigger
    {
        [FunctionName("FunctionEventGridTrigger")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation("Event Grid Data: " + eventGridEvent.Data.ToString());
            var cosmosURL = "https://cosmodbnosql07152023.documents.azure.com:443/";
            var cosmosKey = Environment.GetEnvironmentVariable("COSMOS_KEY");
            var databaseName = "CustomerDB";

            if (string.IsNullOrEmpty(cosmosURL) || string.IsNullOrEmpty(cosmosKey))
            {
                log.LogError("Cosmos DB credentials are missing.");
            }

            try
            {
                string dataToSave = eventGridEvent?.Data?.ToString();
                dataToSave = dataToSave.Replace("\\r\\n", "").Replace("\\u0022", "\"");
                string cleanedJsonString = dataToSave.Replace("{", "").Replace("}", "");
                string[] extractedData = ExtractDataFromJson(cleanedJsonString);
                string firstName = extractedData[0];
                string lastName = extractedData[1];
                int birthdayInEpoch = int.Parse(extractedData[2]);
                string email = extractedData[3];
                Customer customerData = new Customer()
                {
                    id = Guid.NewGuid().ToString(),
                    FirstName = firstName,
                    LastName = lastName,
                    BirthdayInEpoch = birthdayInEpoch,
                    Email = email
                };

                log.LogInformation("customerData: " + dataToSave);
                log.LogInformation("customerData Array: " + extractedData);
                log.LogInformation("customerData Actual: " + customerData);
                //JObject jsonObject = JObject.Parse(dataToSave);
                //log.LogInformation("customerData Parse: " + jsonObject);
                //Customer customerData = JsonConvert.DeserializeObject<Customer>(dataToSave);
                //customerData.id = Guid.NewGuid().ToString();
                //log.LogInformation("customerData: " + customerData);
                //var item = new { id = Guid.NewGuid().ToString(), Data = dataToSave };

                CosmosClient client = new CosmosClient(cosmosURL, cosmosKey);
                Database db = await client.CreateDatabaseIfNotExistsAsync(databaseName);
                Container cont = await db.CreateContainerIfNotExistsAsync("Customer", "/id", 400);

                var response = await cont.CreateItemAsync(customerData);
                log.LogInformation("Saved Successfully");
            }
            catch (Exception ex)
            {
                log.LogError("Function Error : " + ex.Message);
            }
            
        }

        private static string[] ExtractDataFromJson(string jsonString)
        {
            // Split the JSON string into an array of key-value pairs
            string[] keyValuePairs = jsonString.Split(',');

            // Create an array to store the extracted data
            string[] extractedData = new string[keyValuePairs.Length];

            // Extract data from the array of key-value pairs and store in the extractedData array
            for (int i = 0; i < keyValuePairs.Length; i++)
            {
                string[] keyValuePair = keyValuePairs[i].Split(':');
                if (keyValuePair.Length == 2)
                {
                    extractedData[i] = keyValuePair[1].Trim('\"');
                }
            }

            return extractedData;
        }

    }
}
