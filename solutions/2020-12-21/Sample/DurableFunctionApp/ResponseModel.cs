using Newtonsoft.Json;

namespace DurableFunctionApp
{
    public class ResponseModel
    {
        [JsonProperty("completed")]
        public virtual bool Completed { get; set; }
    }
}