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
using System.Text;

namespace Seasons_of_Serverless_Step5
{
    public static class Step5
    {
        [FunctionName("Step5")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "step-5")] HttpRequestMessage req,
            ILogger log)
        {
            var init = new Step5_ResponseData() { Removed = false };

            var requestData = await req.Content.ReadAsAsync<Step5_RequestData>();

            if(requestData.BubbleAppeared)
            {
                init.Removed = true;
            }

            // var payload = JsonConvert.SerializeObject(init);
            var payload = init;

            return new OkObjectResult(payload);
        }
    }
}
