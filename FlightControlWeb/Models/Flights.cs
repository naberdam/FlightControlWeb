using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace FlightControl.Models
{
    public class Flights
    {
        [JsonProperty("flight_id")]
        [JsonPropertyName("flight_id")]
        public string FlightId
        {
            get;
            set;

        }
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
        [JsonProperty("date_time")]
        [JsonPropertyName("date_time")]
        public string DateTime
        {
            get;
            set;
        }
        [JsonProperty("is_external")]
        [JsonPropertyName("is_external")]
        public bool IsExternal
        {
            get;
            set;
        }
    }
}