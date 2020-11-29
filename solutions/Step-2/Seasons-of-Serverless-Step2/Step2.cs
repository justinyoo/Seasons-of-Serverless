using System;
using System.Collections.Generic;
using System.Net.Http;
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
        [FunctionName("Step2_HttpStart")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var requestData = await req.Content.ReadAsAsync<Step2_RequestData>();

            string instanceId = await starter.StartNewAsync("Step2", instanceId: null, requestData);

            log.LogWarning($"Started orchestration with ID = '{instanceId}'.");

            return (ActionResult) new OkObjectResult(starter.CreateHttpManagementPayload(instanceId));
        }

        [FunctionName("Step2")]
        public static async Task<SlicingGreenOnionData> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            SlicingGreenOnionData slicingGreenOnionData = new SlicingGreenOnionData();

            var step2_RequestData = context.GetInput<Step2_RequestData>();

            VerifyRequest(step2_RequestData);

            if (!context.IsReplaying)
            {
                log.LogInformation($"Start Step2_Orchestrator with 'timeToSliceValue' during : {step2_RequestData.TimeToSliceInMinutes}minute");
            }

            //RetryOptions retryPolicy = new RetryOptions(firstRetryInterval: TimeSpan.FromMinutes(1), maxNumberOfAttempts: step2_RequestData.TimeToSliceInMinutes);
            //test¿ë
            RetryOptions retryPolicy = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(1), maxNumberOfAttempts: step2_RequestData.TimeToSliceInMinutes);

            retryPolicy.Handle = (ex) =>
            {
                TaskFailedException failedEx = ex as TaskFailedException;
                return (failedEx.Name != "Step2_SlicingStatus") ? false : true;
            };

            try
            {
                slicingGreenOnionData.completed = await context.CallActivityWithRetryAsync<bool>("Step2_SlicingStatus", retryPolicy, null);
            }
            catch(FunctionFailedException ex)
            {                
                log.LogWarning("Step2_SlicingStatus", ex.Message);

                log.LogWarning($"check again in a minute.");
            }

            log.LogInformation($"slicing ended");

            return slicingGreenOnionData;
        } 

        [FunctionName("Step2_SlicingStatus")]
        public static async Task<bool> SlicingStatus([ActivityTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"Check SlicingStatus");

            var random = new Random();

            var randomBool = random.Next(2) == 1;

            if(!randomBool)
            {
                throw new Exception("The slicing of green onions is not over yet.");
            }
            else
            {
                log.LogInformation($"The slicing of green onions ended");
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