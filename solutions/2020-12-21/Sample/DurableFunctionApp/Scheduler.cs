using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DurableFunctionApp
{
    public static class Scheduler
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Scheduler_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/{orchestratorName}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string orchestratorName,
            ILogger log)
        {
            var input = await req.Content.ReadAsAsync<RequestModel>();

            var serialised = JsonConvert.SerializeObject(input);
            log.LogInformation(serialised);

            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync<RequestModel>(orchestratorName, instanceId: null, input: input);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
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
        public static async Task<bool> CallResult([ActivityTrigger] string callbackUrl, ILogger log)
        {
            log.LogInformation($"Callback URL = '{callbackUrl}'.");

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                return false;
            }

            var payload = new ResponseModel() { Completed = true };
            var formatter = new JsonMediaTypeFormatter();

            var response = await httpClient.PostAsJsonAsync(callbackUrl, payload);

            return true;
        }
    }
}