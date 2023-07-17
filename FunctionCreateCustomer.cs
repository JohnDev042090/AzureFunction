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
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.EventGrid;
using System.Collections.Generic;

namespace AzureFunctionText07152023
{
    public static class FunctionCreateCustomer
    {
        [FunctionName("FunctionCreateCustomer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var topicEndpoint = "https://eventgridtopiccustomer.eastasia-1.eventgrid.azure.net/api/events";
            var topicKey = Environment.GetEnvironmentVariable("TOPIC_KEY");

            if (string.IsNullOrEmpty(topicEndpoint) || string.IsNullOrEmpty(topicKey))
            {
                return new BadRequestObjectResult("Event Grid Topic credentials are missing.");
            }

            try
            {
                log.LogInformation("REQUEST BODY: " + req.Body);
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("REQUEST BODY2: " + requestBody);
                var customer = JsonConvert.DeserializeObject<Customer>(requestBody);
                //customer.id = Guid.NewGuid().ToString();

                #region
                // Create the event grid event
                EventGridEvent eventGridEvent = new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Data = requestBody,
                    Subject = "Subject of the event",
                    EventType = "Customer.Created",
                    EventTime = DateTime.UtcNow,
                    DataVersion = "1.0"
                };

                // Create the topic credentials
                string topicHostname = new Uri(topicEndpoint).Host;
                TopicCredentials topicCredentials = new TopicCredentials(topicKey);
                // Create the event grid client
                EventGridClient eventGridClient = new EventGridClient(topicCredentials);
                // Publish the event to the event grid topic
                await eventGridClient.PublishEventsAsync(topicHostname, new List<EventGridEvent> { eventGridEvent });
                #endregion

                //var response = await cont.CreateItemAsync(customer);
                log.LogInformation("Event Grid Triggered Successfully");
                return new OkObjectResult("Event Grid Triggered Successfully!");
            }
            catch (Exception ex)
            {
                log.LogError("Event Grid Triggered Failed: " + ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            
        }
    }
}
