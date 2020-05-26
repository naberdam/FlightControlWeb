using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightControl.Models
{
    public interface IFlightPlanManager
    {
        Task<FlightPlan> GetFlightPlanById(string id);
        /*Task<FlightPlan> GetFlightPlanByIdAndSync(string id);*/
        string AddFlightPlan(FlightPlan flightPlan);
        /*bool DeleteFlightPlanById(string id);*/
    }
}