using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Seasons_of_Serverless_Step2
{
    public static class Step_2
    {
        [FunctionName("Step_2_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Step_2", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Step_2_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var slicingGreenOnionResult = await context.CallActivityAsync<>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Step_2_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Step_2_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Step_2_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Slicing_GreenOnion")]
        public static string Slicing_GreenOnion([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            for (int i = 0; i < 10; i++)
            {
                DateTime deadline = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                await context.CreateTimer(deadline, CancellationToken.None);
                await context.CallActivityAsync("SendBillingEvent");
            }
            return $"Hello {name}!";
        }

        [FunctionName("Status")]
        public static bool Status([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");

            for (int i = 0; i < 10; i++)
            {
                DateTime deadline = context.CurrentUtcDateTime.Add(TimeSpan.FromDays(1));
                await context.CreateTimer(deadline, CancellationToken.None);
                await context.CallActivityAsync("SendBillingEvent");
            }
            return $"Hello {name}!";
        }



        public class Slicing_Result
        {
            public Slicing_Result()
            {
                completed = false;
            }
            public bool completed { get; set; }
        }
    }
}