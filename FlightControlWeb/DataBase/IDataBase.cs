using FlightControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb
{
    public interface IDataBase
    {
        bool DeleteFlightPlanFromTable(string id);
        List<Flights> GetFlightsByDateTime(string stringDateTime);
        Task<List<Flights>> GetFlightsByDateTimeAndSync(string stringDateTime);
        string AddFlightPlan(FlightPlan flightPlan);
        Task<FlightPlan> GetFlightPlanById(string id);
        /*Task<FlightPlan> GetFlightPlanByIdAndSync(string id);*/
        bool DeleteServer(string id);
        List<Server> GetExternalServers();
        string AddServer(Server server);
    }
}