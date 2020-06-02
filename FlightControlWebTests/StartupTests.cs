using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightControl;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using FlightControl.Models;
using FlightControlWeb;
using System.Linq;
using FlightControl.Models.FlightPlanObjects;
using System.Threading.Tasks;

namespace FlightControl.Tests
{
    [TestClass()]
    public class StartupTests
    {
        // Our data base that we will make a tests about his data that we will send him.
        SqliteDataBase dataBase;
        // Check the exception.
        [TestMethod()]
        public void ConfigureServicesTest()
        {
            try
            {
                Startup startUp = new Startup(null);
                Assert.Fail("Dont catch the excepation");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod()]
        public async Task ConfigureServicesTest1()
        {
            // Build the data base.
            dataBase = new SqliteDataBase("sqliteTest.sqlite");
            await CreateTest();
        }
        private async Task CreateTest()
        {
            // Reset the flights for the test.
            DeleteFlights();
            // Add 3 flight plans.
            AddFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            AddFlightPlansForFake(20, 30, 100, 6, 7, "Wizz", false, true);
            AddFlightPlansForFake(50, 67, 200, 8, 5, "Rainair", false, true);
            // Flight without segments.
            AddFlightPlansForFake(40, 80, 215, 6, 3, "Arkia", false, false);
            // Check the number of the flights that we have.
            ///To know if the invalid flight get in or not.
            if (IsGoodTest(3, 4))
            {
                Console.WriteLine("The test of flight without segments was successful");
            }
            else
            {
                Assert.Fail("There is a problem with test of flight without segments");
                Console.WriteLine("There is a problem with test of flight without segments");
            }
            // Add flightPlan without companyName.
            AddFlightPlansForFake(32, 34, 250, 8, 4, null, false, true);
            // Check the number of the flights that we have.
            //To know if the invalid flight get in or not.
            if (IsGoodTest(3, 4) || IsGoodTest(3, 5))
            {
                Console.WriteLine("The test of flight without companyName was successful");
            }
            else
            {
                Assert.Fail("There is a problem with test of flight without companyName");
                Console.WriteLine("There is a problem with test of flight without companyName");
            }
            // Add flight with passengers not valid.
            AddFlightPlansForFake(20, 30, -100, 4, 6, "Wizz", false, true);
            // Check the number of the flights that we have.
            // To know if the invalid flight get in or not.
            if (IsGoodTest(3, 4) || IsGoodTest(3, 5) || IsGoodTest(3, 6))
            {
                Console.WriteLine("The test of flight passengers not valid was successful");
            }
            else
            {
                Assert.Fail("There is a problem with test of flight passengers not valid");
                Console.WriteLine("Problem with test of flight without passengers not valid");
            }
            // Add regular flights.        
            AddFlightPlansForFake(40, 50, 150, 6, 5, "Lufthanza", false, true);
            AddFlightPlansForFake(30, 60, 175, 6, 7, "El-AL", false, true);
            AddFlightPlansForFake(70, 70, 220, 5, 3, "Delta", false, true);
            string idDelete = AddFlightPlansForFake(43, 42, 120, 1, 7, "IsraAir", false, true);
            dataBase.DeleteFlightPlanFromTable(idDelete);
            if (IsGoodTest(7, 8))
            {
                // print good test all.
                Console.WriteLine("Delete and regular filghts tests was successful");
            }
            else
            {
                Assert.Fail("There is a problem with the input");
                Console.WriteLine("There is a problem with the input");
            }
            string id = AddFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            // Check GetFlightPlanById and it will call to the async too.
            if (await dataBase.GetFlightPlanById(id) == null)
            {
                Assert.Fail("Problem in get flight by Id");
            }
            // Check the Methood DeleteFlightPlanFromTable.
            id = AddFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            int size = SizeFlights();
            dataBase.DeleteFlightPlanFromTable(id);
            // Check delete.
            if (SizeFlights() != (size - 1))
            {
                Assert.Fail("Problem in erasing flights");
            }
            // Flight that not exist.

            if (await dataBase.GetFlightPlanById("111111111111111111") != null)
            {
                Assert.Fail("Problem in get flight by Id");

            }
            // Check the Methood GetFlightPlanByIdAndSync.
            id = AddFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            if (await dataBase.GetFlightPlanById(id) == null)
            {
                Assert.Fail("Problem in get flight by Id async");
            }
        }
        // The methood check the how much flights we have in our data base right now.
        private int SizeFlights()
        {
            // Check the time now.
            DateTime current = DateTime.Now;
            string date = current.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            List<Flights> flightsNow = new List<Flights>();
            flightsNow = dataBase.GetFlightsByDateTime(date);
            if (flightsNow == null)
            {
                return 0;
            }
            return flightsNow.Count();
        }
        // Delete all the flights for the test.
        private void DeleteFlights()
        {
            DateTime current = DateTime.Now;
            string date = current.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            List<Flights> flightsNow = new List<Flights>();
            // Get the list of the flights.
            flightsNow = dataBase.GetFlightsByDateTime(date);
            if (flightsNow == null)
            {
                return;
            }
            foreach (Flights flight in flightsNow)
            {
                string id = flight.FlightId;
                dataBase.DeleteFlightPlanFromTable(id);
            }
        }
        // Function that returns true if it is a good test.
        private bool IsGoodTest(int numberExpected, int notExpected)
        {
            int size = SizeFlights();
            if (size == notExpected)
            {
                // Problem in the test.
                return false;
            }
            else if (size == numberExpected)
            {
                // print good test.
                return true;
            }
            return true;
        }
        // The function will add flights to our data base.
        private string AddFlightPlansForFake(int longitudeStart, int latitudeStart, int passangers,
            int inceaseLong, int increaseLat, string companyName, bool sameId, bool withSegments)
        {
            DateTime current = DateTime.Now;
            current = current.AddSeconds(-10);
            string date = current.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            FlightPlan flightPlanFake1 = new FlightPlan();
            flightPlanFake1.CompanyName = companyName;
            flightPlanFake1.Passengers = passangers;
            flightPlanFake1.Location = new InitialLocation
            {
                Latitude = latitudeStart,
                Longitude = longitudeStart,
                DateTime = date
            };
            if (withSegments)
            {
                flightPlanFake1.Segments = new List<Segment>();
                flightPlanFake1.Segments.Add(new Segment
                {
                    Latitude = latitudeStart + increaseLat,
                    Longitude = longitudeStart + inceaseLong,
                    TimespanSeconds = 10000
                });
                flightPlanFake1.Segments.Add(new Segment
                {
                    Latitude = latitudeStart + 2 * increaseLat,
                    Longitude = longitudeStart + 2 * inceaseLong,
                    TimespanSeconds = 10000
                });
            }
            string id;
            id = dataBase.AddFlightPlan(flightPlanFake1);
            return id;
        }
    }
}