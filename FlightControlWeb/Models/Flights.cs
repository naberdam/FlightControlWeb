using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.Json.Serialization;

namespace FlightControl.Models
{
    public class Flights
    {
        public string Flight_id
        {
            get;

            set;

        }
        public double Longitude
        {
            get;
            set;
        }
        public double Latitude
        {
            get;
            set;
        }
        public int Passengers
        {
            get;
            set;
        }
        public string Company_name
        {
            get;
            set;
        }
        public string Date_time
        {
            get;
            set;
        }
        public string Is_external
        {
            get;
            set;
        }
    }
}