using FlightControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.DataBase
{
    public class CheckObjects
    {
        private const double MaxLatitude = 180.0;
        private const double MinLatitude = -180.0;
        private const double MaxLongitude = 90.0;
        private const double MinLongitude = -90.0;
        public static bool CheckProperServer(Server server)
        {
            if (server.ServerId == null) { return false; }
            if (server.ServerURL == null) { return false; }
            return true;
        }

        public static bool CheckFlightPlan(FlightPlan flightPlan)
        {
            if (flightPlan == null) { return false; }
            if (flightPlan.Location == null) { return false; }
            if (flightPlan.Segments == null) { return false; }
            if (flightPlan.Company_name == null) { return false; }
            if (flightPlan.Passengers <= 0) { return false; }
            if (flightPlan.Location.Date_Time == default) { return false; }
            if (!CheckLatitude(flightPlan.Location.Latitude)) { return false; }
            if (!CheckLongitude(flightPlan.Location.Longitude)) { return false; }
            int i = 0;
            for (; i < flightPlan.Segments.Count; ++i)
            {
                if (flightPlan.Segments[i].Timespan_Seconds <= 0) { return false; }
                if (!CheckLatitude(flightPlan.Segments[i].Latitude)) { return false; }
                if (!CheckLongitude(flightPlan.Segments[i].Longitude)) { return false; }
            }
            return true;
        }
        private static bool CheckLatitude(double latitude)
        {
            if (latitude < MinLatitude || latitude > MaxLatitude) { return false; }
            return true;
        }
        private static bool CheckLongitude(double longitude)
        {
            if (longitude < MinLongitude || longitude > MaxLongitude) { return false; }
            return true;
        }
        // Function that checks if the givven flight is proper.
        public static bool CheckFlights(Flights flight)
        {
            if (flight == null) { return false; }
            if (flight.Company_name == default
                || flight.Date_time == default
                || flight.Flight_id == default
                || flight.Passengers <= 0) { return false; }
            if (!CheckLatitude(flight.Latitude)) { return false; }
            if (!CheckLongitude(flight.Longitude)) { return false; }
            return true;

        }
    }
}
