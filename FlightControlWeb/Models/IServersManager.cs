using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightControl.Models
{
    public interface IServersManager
    {
        bool DeleteServer(string id);
        IEnumerable<Server> GetServers();
        string AddServer(Server s);
    }
}