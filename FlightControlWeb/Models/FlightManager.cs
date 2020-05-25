using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FlightControl.Controllers;
using FlightControlWeb;

namespace FlightControl.Models
{
    public class FlightManager : IFlightManager
    {
        private IDataBase sqliteDataBase;
        public FlightManager(IDataBase sqliteData)
        {
            sqliteDataBase = sqliteData;
        }
        public bool DeleteFlightById(string id)
        {
            return sqliteDataBase.DeleteFlightPlanFromTable(id);
        }

        public IEnumerable<Flights> GetFlightsByDateTime(string dateTime)
        {
            return sqliteDataBase.GetFlightsByDateTime(dateTime);
        }

        public async Task<IEnumerable<Flights>> GetFlightsByDateTimeAndSync(string dateTime)
        {
            return await sqliteDataBase.GetFlightsByDateTimeAndSync(dateTime);
        }
    }

}