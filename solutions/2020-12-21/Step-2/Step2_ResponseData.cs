using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seasons_of_Serverless_Step2
{
    public class Step2_ResponseData
    {
        [JsonProperty("completed")]
        public bool Completed { get; set; }
    }
}
