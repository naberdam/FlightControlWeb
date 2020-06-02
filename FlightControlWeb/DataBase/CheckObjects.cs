using FlightControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlightControlWeb.DataBase
{
    public class CheckObjects
    {
        private const double MaxLatitude = 90.0;
        private const double MinLatitude = -90.0;
        private const double MaxLongitude = 180.0;
        private const double MinLongitude = -180.0;
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
            if (flightPlan.CompanyName == null) { return false; }
            if (flightPlan.Passengers <= 0) { return false; }
            if (flightPlan.Location.DateTime == default) { return false; }
            if (!CheckLatitude(flightPlan.Location.Latitude)) { return false; }
            if (!CheckLongitude(flightPlan.Location.Longitude)) { return false; }
            int i = 0;
            for (; i < flightPlan.Segments.Count; ++i)
            {
                if (flightPlan.Segments[i].TimespanSeconds <= 0) { return false; }
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
            if (flight.CompanyName == default
                || flight.DateTime == default
                || flight.FlightId == default
                || flight.Passengers <= 0) { return false; }
            if (!CheckLatitude(flight.Latitude)) { return false; }
            if (!CheckLongitude(flight.Longitude)) { return false; }
            return true;

        }
        // Function that checks if the givven stringDateTime is correct and there is time 
        // Like this.
        public static bool CheckDateTime(string stringDateTime)
        {
            // The pattern we asked for.
            string pattern = @"(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})Z";
            if (Regex.IsMatch(stringDateTime, pattern))
            {
                Match match = Regex.Match(stringDateTime, pattern);
                int year = Convert.ToInt32(match.Groups[1].Value);
                int month = Convert.ToInt32(match.Groups[2].Value);
                int day = Convert.ToInt32(match.Groups[3].Value);
                int hour = Convert.ToInt32(match.Groups[4].Value);
                int minute = Convert.ToInt32(match.Groups[5].Value);
                int second = Convert.ToInt32(match.Groups[6].Value);
                try
                {
                    DateTime checkDateTime = new DateTime(year, month, day, hour, minute, second);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
    }
}
