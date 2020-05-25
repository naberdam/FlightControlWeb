using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightControl.Models
{
    public interface IFlightManager
    {
        IEnumerable<Flights> GetFlightsByDateTime(string dateTime);
        Task<IEnumerable<Flights>> GetFlightsByDateTimeAndSync(string dateTime);
        bool DeleteFlightById(string id);
    }
}
