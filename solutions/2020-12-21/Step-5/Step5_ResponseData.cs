using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seasons_of_Serverless_Step5
{
    public class Step5_ResponseData
    {
        [JsonProperty("removed")]
        public bool Removed { get; set; }
    }
}
