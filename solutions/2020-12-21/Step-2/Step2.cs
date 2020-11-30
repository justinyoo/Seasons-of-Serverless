using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using DurableTask.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Seasons_of_Serverless_Step2
{
    public static class Step2
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Step2")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var requestData = await req.Content.ReadAsAsync<Step2_RequestData>();

            var instanceId = starter.StartNewAsync("Step2_Orchestrator", requestData).Result;

            log.LogWarning($"Started orchestration with ID = '{instanceId}'.");

            var orchestratorId = starter.CreateHttpManagementPayload(instanceId);

            return new OkObjectResult(orchestratorId);
        }

        [FunctionName("Step2_Orchestrator")]
        public static async Task<bool> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var step2_RequestData = context.GetInput<Step2_RequestData>();

            VerifyRequest(step2_RequestData);

            if (!context.IsReplaying)
            {
                log.LogInformation($"Start Step2_Orchestrator with 'timeToSliceValue' during : {step2_RequestData.TimeToSliceInMinutes}minute");
            }

            RetryOptions retryPolicy = new RetryOptions(firstRetryInterval: TimeSpan.FromMinutes(1), maxNumberOfAttempts: step2_RequestData.TimeToSliceInMinutes);
            //test¿ë
            //RetryOptions retryPolicy = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(3), maxNumberOfAttempts: step2_RequestData.TimeToSliceInMinutes);

            retryPolicy.Handle = (ex) =>
            {
                TaskFailedException failedEx = ex as TaskFailedException;
                 return failedEx.Name == "Step2_SlicingStatus";
            };

            var activity = await context.CallActivityWithRetryAsync<bool>("Step2_SlicingStatus", retryPolicy, step2_RequestData.CallBackUrl);

            log.LogInformation($"Step2 ended");

            return true;
        } 

        [FunctionName("Step2_SlicingStatus")]
        public static async Task<bool> SlicingStatus([ActivityTrigger] string callBackUrl, ILogger log)
        {
            log.LogInformation($"Check SlicingStatus");

            var random = new Random();

            var randomBool = random.Next(2) == 1;

            if(!randomBool)
            {
                throw new TaskFailedException("The slicing of green onions is not over yet.");
            }
            else
            {
                log.LogInformation($"The slicing of green onions ended");

                var payload = new Step2_ResponseData() { Completed = true };
                var formatter = new JsonMediaTypeFormatter();

                var response = await httpClient.PostAsJsonAsync(callBackUrl, payload);
            }

            return randomBool;
        }

        [Deterministic]
        private static void VerifyRequest(Step2_RequestData requestData)
        {
            if (requestData == null)
            {
                throw new ArgumentNullException(nameof(requestData), "An input object is required.");
            }

            if (requestData.TimeToSliceInMinutes <= 0)
            {
                throw new OverflowException("A input value is not time.");
            }
        }
    }
}