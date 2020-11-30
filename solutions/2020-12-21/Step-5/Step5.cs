using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Net.Http;

namespace Seasons_of_Serverless_Step5
{
    public static class Step5
    {
        [FunctionName("Step5")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            ILogger log)
        {
            var payload = new Step5_ResponseData() { Removed = false };

            var requestData = await req.Content.ReadAsAsync<Step5_RequestData>();

            if(requestData.BubbleAppeared)
            {
                payload.Removed = true;
            }

            return new OkObjectResult(payload);
        }
    }
}
