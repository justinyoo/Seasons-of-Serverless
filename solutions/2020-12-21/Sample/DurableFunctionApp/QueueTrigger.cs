using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

using Microsoft.Extensions.Logging;

namespace DurableFunctionApp
{
    public static class QueueTrigger
    {
        private static HttpClient httpClient = new HttpClient();
        private static MediaTypeFormatter formatter = new JsonMediaTypeFormatter();

        [FunctionName("QueueTrigger")]
        public static async Task Run(
            [QueueTrigger("sample")]string queue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queue}");

            var payload = new ResponseModel() { Completed = true };
            var formatter = new JsonMediaTypeFormatter();

            var response = await httpClient.PostAsJsonAsync(queue, payload);
        }
    }
}
