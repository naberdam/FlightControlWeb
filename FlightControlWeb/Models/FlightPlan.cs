using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.Json.Serialization;
using FlightControl.Models.FlightPlanObjects;

namespace FlightControl.Models
{
    public class FlightPlan
    {
        [JsonPropertyName("passengers")]
        public int Passengers
        {
            get;
            set;
        }
        [JsonPropertyName("company_name")]
        public string Company_name
        {
            get;
            set;
        }
        [JsonPropertyName("initial_location")]
        public InitialLocation Location { get; set; }
        [JsonPropertyName("segments")]
        public List<Segment> Segments { get; set; }
    }


}