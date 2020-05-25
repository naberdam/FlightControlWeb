using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControl.Models.FlightPlanObjects
{
    public class Segment
    {
        [JsonPropertyName("longitude")]
        public double Longitude
        {
            get;
            set;
        }
        [JsonPropertyName("latitude")]
        public double Latitude
        {
            get;
            set;
        }
        [JsonPropertyName("timespan_seconds")]
        public int Timespan_Seconds
        {
            get;
            set;
        }
    }
}
