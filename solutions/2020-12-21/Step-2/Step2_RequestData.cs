using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seasons_of_Serverless_Step2
{
    public class Step2_RequestData
    {
        [JsonProperty("callbackUrl")]
        public string CallBackUrl { get; set; }

        [JsonProperty("timeToSliceInMinutes")]
        public int TimeToSliceInMinutes { get; set; }
    }
}
