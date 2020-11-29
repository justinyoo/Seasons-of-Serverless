using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DurableFunctionApp
{
    public static class Scheduler
    {
        [FunctionName("Scheduler_HttpStart")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/{orchestratorName}")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            string orchestratorName,
            ILogger log)
        {
            var input = default(RequestModel);
            using (var reader = new StreamReader(req.Body))
            {
                var serialised = await reader.ReadToEndAsync();

                log.LogInformation(serialised);

                input = JsonConvert.DeserializeObject<RequestModel>(serialised);
            }

            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync<RequestModel>(orchestratorName, instanceId: null, input: input);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var mgmt = starter.CreateHttpManagementPayload(instanceId);

            return new OkObjectResult(mgmt);
        }

        [FunctionName("scheduler")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var req = context.GetInput<RequestModel>();

            var mins = req.TimeToSoakInMinutes;

            var initiated = context.CurrentUtcDateTime;
            var scheduled = initiated.AddMinutes(mins);

            await context.CreateTimer(scheduled, CancellationToken.None);

            await context.CallActivityAsync("SchedulerActivity", req.CallbackUrl);

            return true;
        }

        [FunctionName("SchedulerActivity")]
        public static async Task<bool> CallResult(
            [ActivityTrigger] string callbackUrl,
            [Queue("sample")] IAsyncCollector<string> queue,
            ILogger log)
        {
            log.LogInformation($"Callback URL = '{callbackUrl}'.");

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                return false;
            }

            await queue.AddAsync(callbackUrl);

            return true;
        }
    }
}