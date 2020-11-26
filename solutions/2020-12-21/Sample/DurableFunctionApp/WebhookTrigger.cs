using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DurableFunctionApp
{
    public static class WebhookTrigger
    {
        [FunctionName("WebhookTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "callback")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var response = default(ResponseModel);
            using (var reader = new StreamReader(req.Body))
            {
                var payload = await reader.ReadToEndAsync();
                response = JsonConvert.DeserializeObject<ResponseModel>(payload);
            }

            return new OkObjectResult(response);
        }
    }
}
