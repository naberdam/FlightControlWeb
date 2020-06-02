using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControl.Models;
using System.Text.RegularExpressions;
using FlightControlWeb.DataBase;
using System.Threading;

namespace FlightControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private IFlightManager flightManager;
        public FlightsController(IFlightManager iFlightManager)
        {
            flightManager = iFlightManager;
        }
        // GET: api/Flights/5
        [HttpGet]
        public async Task<ActionResult<Flights>> Get([FromQuery] string relative_To)
        {
            string urlRequest = Request.QueryString.Value;
            IEnumerable<Flights> flightList = new List<Flights>();
            // The pattern we asked for.
            string patternAndSync = @"^?relative_to=(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})Z+(&sync_all)$";
            string patternWithoutSync = @"^?relative_to=(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})Z$";
            // Check if the givven dateTime in relative_to is availible.
            if (!CheckObjects.CheckDateTime(relative_To))
            {
                return BadRequest();
            }
            if (Regex.IsMatch(urlRequest, patternAndSync))
            {
                flightList = await flightManager.GetFlightsByDateTimeAndSync(relative_To);
            }
            else if (Regex.IsMatch(urlRequest, patternWithoutSync))
            {
                flightList = flightManager.GetFlightsByDateTime(relative_To);
            }
            else
            {
                return BadRequest();
            }
            if (flightList == null) { return NotFound(flightList); }
            return Ok(flightList);
        }
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            string urlRequest = Request.Path;
            string correctPattern = @"^/api/Flights/[a-zA-Z]{2}[0-9]{5}[a-zA-Z]{3}$";
            if (!Regex.IsMatch(urlRequest, correctPattern))
            {
                return BadRequest();
            }
            bool succeed = flightManager.DeleteFlightById(id);
            if (succeed)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
