using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FlightControlWeb;

namespace FlightControl.Models
{
    public class FlightPlanManager : IFlightPlanManager
    {
        private static IDataBase sqliteDataBase;
        public FlightPlanManager(IDataBase sqliteData)
        {
            sqliteDataBase = sqliteData;
        }
        public string AddFlightPlan(FlightPlan flightPlan)
        {
            return sqliteDataBase.AddFlightPlan(flightPlan);
        }

        public async Task<FlightPlan> GetFlightPlanById(string id)
        {
            return await sqliteDataBase.GetFlightPlanById(id);
        }

        /*public bool DeleteFlightPlanById(string id)
        {
            return sqliteDataBase.DeleteFlightPlanFromTable(id);
        }*/

        /*public async Task<FlightPlan> GetFlightPlanByIdAndSync(string id)
        {
            return await sqliteDataBase.GetFlightPlanByIdAndSync(id);
        }*/
    }
}