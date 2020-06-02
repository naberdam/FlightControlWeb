using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.Json.Serialization;
using FlightControl.Models.FlightPlanObjects;
using Newtonsoft.Json;

namespace FlightControl.Models
{
    public class FlightPlan
    {
        [JsonProperty("passengers")]
        [JsonPropertyName("passengers")]
        public int Passengers
        {
            get;
            set;
        }
        [JsonProperty("company_name")]
        [JsonPropertyName("company_name")]
        public string CompanyName
        {
            get;
            set;
        }
        [JsonProperty("initial_location")]
        [JsonPropertyName("initial_location")]
        public InitialLocation Location { get; set; }
        [JsonProperty("segments")]
        [JsonPropertyName("segments")]
        public List<Segment> Segments { get; set; }
    }


}