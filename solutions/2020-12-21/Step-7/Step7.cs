using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using DurableTask.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Seasons_of_Serverless_Step7
{
    public static class Step7
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Step7")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "step-7")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var requestData = await req.Content.ReadAsAsync<Step7_RequestData>();

            var instanceId = starter.StartNewAsync("Step7_Orchestrator", requestData).Result;

            log.LogWarning($"Started orchestration with ID = '{instanceId}'.");

            var orchestratorId = starter.CreateHttpManagementPayload(instanceId);

            return new OkObjectResult(orchestratorId);
        }

        [FunctionName("Step7_Orchestrator")]
        public static async Task<bool> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var step7_RequestData = context.GetInput<Step7_RequestData>();

            if (!context.IsReplaying)
            {
                log.LogInformation($"Start Step7_Orchestrator");
            }
            RetryOptions retryPolicy = new RetryOptions(firstRetryInterval: TimeSpan.FromMinutes(1), maxNumberOfAttempts: 10);
            //test
            //RetryOptions retryPolicy = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(3), maxNumberOfAttempts: 10);

            var addgarlic = await context.CallActivityAsync<bool>("Step7_AddGarlic", null);

            var addSoySauce = await context.CallActivityAsync<bool>("Step7_AddSoySauce", null);

            var addSalt = await context.CallActivityWithRetryAsync<bool>("Step7_AddSalt", retryPolicy, step7_RequestData.CallBackUrl);

            log.LogInformation($"Step7 ended");

            return true;
        }

        [FunctionName("Step7_AddGarlic")]
        public static bool AddGarlic([ActivityTrigger] string callBackUrl, ILogger log)
        {
            log.LogInformation($"AddGarlic");

            return true;
        }

        [FunctionName("Step7_AddSoySauce")]
        public static bool AddSoySauce([ActivityTrigger] string callBackUrl, ILogger log)
        {
            log.LogInformation($"AddSoySauce");

            return true;
        }

        [FunctionName("Step7_AddSalt")]
        public static async Task<bool> AddSalt([ActivityTrigger] string callBackUrl, ILogger log)
        {
            log.LogInformation($"AddSalt");

            var random = new Random();

            var randomBool = random.Next(2) == 1;
            // var randomBool = true;

            if (!randomBool)
            {
                throw new FunctionFailedException("Seasoning is not over yet.");
            }
            else
            {
                log.LogInformation($"Seasoning ended");

                var payload = JsonConvert.SerializeObject(new Step7_ResponseData() { Completed = true });

                var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(callBackUrl, httpContent);
            }

            return randomBool;
        }

        [Deterministic]
        private static void VerifyRequest(Step7_RequestData requestData)
        {
            if (requestData == null)
            {
                throw new ArgumentNullException(nameof(requestData), "An input object is required.");
            }

            if (requestData.CallBackUrl == string.Empty)
            {
                throw new Exception("CallBackUrl not defined.");
            }
        }

    }
}