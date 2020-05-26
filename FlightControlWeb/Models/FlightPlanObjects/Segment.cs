using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControl.Models.FlightPlanObjects
{
    public class Segment
    {
        [JsonProperty("longitude")]
        [JsonPropertyName("longitude")]
        public double Longitude
        {
            get;
            set;
        }
        [JsonProperty("latitude")]
        [JsonPropertyName("latitude")]
        public double Latitude
        {
            get;
            set;
        }
        [JsonProperty("timespan_seconds")]
        [JsonPropertyName("timespan_seconds")]
        public int Timespan_Seconds
        {
            get;
            set;
        }
    }
}
