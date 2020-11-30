using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Seasons_of_Serverless_Step7
{
    public class Step7_RequestData
    {
        [JsonProperty("callbackUrl")]
        public string CallBackUrl { get; set; }
    }
}
