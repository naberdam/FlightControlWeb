using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControl.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using FlightControlWeb.DataBase;

namespace FlightControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private static IFlightPlanManager flightPlanManager;
        public FlightPlanController(IFlightPlanManager iFlightPlanManager)
        {
            flightPlanManager = iFlightPlanManager;
        }
        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "GetFlightPlan")]
        public async Task<ActionResult> GetFlightPlan(string id)
        {
            FlightPlan flightPlan = await flightPlanManager.GetFlightPlanById(id);
            if (flightPlan != null)
            {
                return Ok(flightPlan);
            }
            return NotFound(flightPlan);
        }

        // POST: api/FlightPlan
        [HttpPost]
        public ActionResult Post([FromBody] FlightPlan flightPlan)
        {
            if (!CheckObjects.CheckFlightPlan(flightPlan))
            {
                return BadRequest();
            }
            string idOfAddedFlightPlan = flightPlanManager.AddFlightPlan(flightPlan);
            return CreatedAtAction(actionName: "GetFlightPlan",
                new { id = idOfAddedFlightPlan }, flightPlan);
        }
    }
}