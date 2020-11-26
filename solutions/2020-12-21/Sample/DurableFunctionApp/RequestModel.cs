using Newtonsoft.Json;

namespace DurableFunctionApp
{
    public class RequestModel
    {
        [JsonProperty("boughtSlicedGaraetteok")]
        public virtual bool BoughtSlicedGaraetteok { get; set; }

        [JsonProperty("timeToSoakInMinutes")]
        public virtual int TimeToSoakInMinutes { get; set; }

        [JsonProperty("callbackUrl")]
        public virtual string CallbackUrl { get; set; }
    }
}