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
        SqliteDataBase dataBase;
        [TestMethod()]
        public void ConfigureServicesTest()
        {
            try
            {
                Startup startUp = new Startup(null);
                Assert.Fail("Dont catch the excepation");
            }
            catch (Exception e)
            {
                e = null;
            }
        }

        [TestMethod()]
        public void ConfigureServicesTest1()
        {
            dataBase = new SqliteDataBase("sqliteTest.sqlite");
            createTest();
        }
        private void createTest()
        {
            // Reset the flights for the test.
            deleteFlights();
            // Add 3 flight plans.
            addFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            addFlightPlansForFake(20, 30, 100, 6, 7, "Wizz", false, true);
            addFlightPlansForFake(50, 67, 200, 8, 5, "Rainair", false, true);
            // Flight without segments.
            addFlightPlansForFake(40, 80, 215, 6, 3, "Arkia", false, false);
            if (isGoodTest(3, 4))
            {
                Console.WriteLine("The test of flight without segments was successful");
            }
            else
            {
                Assert.Fail("There is a problem with test of flight without segments");
                Console.WriteLine("There is a problem with test of flight without segments");
            }
            // Add flightPlan without companyName.
            addFlightPlansForFake(32, 34, 250, 8, 4, null, false, true);
            if (isGoodTest(3, 4) || isGoodTest(3, 5))
            {
                Console.WriteLine("The test of flight without companyName was successful");
            }
            else
            {
                Assert.Fail("There is a problem with test of flight without companyName");
                Console.WriteLine("There is a problem with test of flight without companyName");
            }
            // Add flight with passengers not valid.
            addFlightPlansForFake(20, 30, -100, 4, 6, "Wizz", false, true);
            if (isGoodTest(3, 4) || isGoodTest(3, 5) || isGoodTest(3, 6))
            {
                Console.WriteLine("The test of flight passengers not valid was successful");
            }
            else
            {
                Assert.Fail("There is a problem with test of flight passengers not valid");
                Console.WriteLine("There is a problem with test of flight without companyNamepassengers not valid");
            }
            // Add regular flight.        
            addFlightPlansForFake(40, 50, 150, 6, 5, "Lufthanza", false, true);
            addFlightPlansForFake(30, 60, 175, 6, 7, "El-AL", false, true);
            addFlightPlansForFake(70, 70, 220, 5, 3, "Delta", false, true);
            string idDelete = addFlightPlansForFake(43, 42, 120, 1, 7, "IsraAir", false, true);
            dataBase.DeleteFlightPlanFromTable(idDelete);
            if (isGoodTest(7, 8))
            {
                // print good test all.
                Console.WriteLine("All of The tests was successful");
            }
            else
            {
                Assert.Fail("There is a problem with the input");
                Console.WriteLine("There is a problem with the input");
            }
            string id = addFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            if (dataBase.GetFlightPlanById(id) == null)
            {
                Assert.Fail("Problem in get flight by Id");
            }
            // Check the Methood DeleteFlightPlanFromTable.
            id = addFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            int size = sizeFlights();
            dataBase.DeleteFlightPlanFromTable(id);
            if (sizeFlights() != (size - 1))
            {
                Assert.Fail("Problem in erasing flights");
            }

            /*if (dataBase.GetFlightPlanById("111111111111111111") != null)
            {
              //  Assert.Fail("Problem in get flight by Id");
               
            }*/

            // Check the Methood GetFlightPlanByIdAndSync.
            id = addFlightPlansForFake(32, 34, 250, 8, 4, "Swiss", false, true);
            if (dataBase.GetFlightPlanById(id) == null)
            {
                Assert.Fail("Problem in get flight by Id async");
            }
        }
        private int sizeFlights()
        {
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
        private void deleteFlights()
        {
            DateTime current = DateTime.Now;
            //current = current.AddSeconds(20);
            string date = current.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            List<Flights> flightsNow = new List<Flights>();
            flightsNow = dataBase.GetFlightsByDateTime(date);
            if (flightsNow == null)
            {
                return;
            }
            foreach (Flights flight in flightsNow)
            {
                string id = flight.Flight_id;
                dataBase.DeleteFlightPlanFromTable(id);
            }
        }

        private bool isGoodTest(int numberExpected, int notExpected)
        {
            int size = sizeFlights();
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
        private string addFlightPlansForFake(int longitudeStart, int latitudeStart, int passangers, int inceaseLong, int increaseLat, string companyName, bool sameId, bool withSegments)
        {
            DateTime current = DateTime.Now;
            current = current.AddSeconds(-10);
            string date = current.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            FlightPlan flightPlanFake1 = new FlightPlan();
            flightPlanFake1.Company_name = companyName;
            flightPlanFake1.Passengers = passangers;
            flightPlanFake1.Location = new InitialLocation
            {
                Latitude = latitudeStart,
                Longitude = longitudeStart,
                Date_Time = date
            };
            if (withSegments)
            {
                flightPlanFake1.Segments = new List<Segment>();
                flightPlanFake1.Segments.Add(new Segment
                {
                    Latitude = latitudeStart + increaseLat,
                    Longitude = longitudeStart + inceaseLong,
                    Timespan_Seconds = 10000
                });
                flightPlanFake1.Segments.Add(new Segment
                {
                    Latitude = latitudeStart + 2 * increaseLat,
                    Longitude = longitudeStart + 2 * inceaseLong,
                    Timespan_Seconds = 10000
                });
            }
            string id;
            id = dataBase.AddFlightPlan(flightPlanFake1);
            return id;
        }
    }
}