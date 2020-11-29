using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DurableFunctionApp
{
    public static class QueueTrigger
    {
        private static HttpClient httpClient = new HttpClient();
        private static JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

        [FunctionName("QueueTrigger")]
        public static async Task Run(
            [QueueTrigger("sample")]string queue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queue}");

            var payload = new ResponseModel() { Completed = true };

            using (var req = new HttpRequestMessage(HttpMethod.Post, queue))
            using (var content = new StringContent(JsonConvert.SerializeObject(payload, formatter.SerializerSettings)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                req.Content = content;

                await httpClient.SendAsync(req);
            }
        }
    }
}
