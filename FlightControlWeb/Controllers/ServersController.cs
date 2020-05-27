using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControl.Models;
using System.Text.RegularExpressions;

namespace FlightControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        public static IServersManager serverManager;
        public ServersController(IServersManager iServerManager)
        {
            serverManager = iServerManager;
        }
        // GET: api/Server
        [HttpGet(Name = "GetAllExternalServers")]
        public ActionResult<IEnumerable<Server>> GetGetAllExternalServers()
        {
            IEnumerable<Server> ienumrableServer = serverManager.GetServers();
            if (ienumrableServer == null)
            {
                return NotFound(ienumrableServer);
            }
            return Ok(ienumrableServer);
        }
        // POST: api/Server
        [HttpPost]
        public ActionResult Post([FromBody] Server server)
        {
            string idOfAddedServer = serverManager.AddServer(server);
            if (idOfAddedServer == null)
            {
                return BadRequest(idOfAddedServer);
            }
            return Ok(idOfAddedServer);
        }

        // DELETE: api/Server/5
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            bool succeed = serverManager.DeleteServer(id);
            if (!succeed)
            {
                return NotFound(succeed);
            }
            return Ok(succeed);
        }
    }
}