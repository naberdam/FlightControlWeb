using FlightControl.Models;
using FlightControl.Models.FlightPlanObjects;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using FlightControlWeb;

namespace FlightControl
{
    public class SqliteDataBase : IDataBase
    {
        private SqliteConnection myConnection;
        private Mutex mutex;
        public SqliteDataBase()
        {
            mutex = new Mutex();
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();
            // The path of our sqlite that we want to create.
            connectionStringBuilder.DataSource = AppDomain.CurrentDomain.BaseDirectory + @"\Database.sqlite";
            myConnection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            OpenConnection();
            // Create FlightPlanSQL table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS FlightPlanSQL (Id TEXT PRIMARY KEY NOT NULL, Passengers INTEGER, Company_name TEXT NOT NULL, Is_external TEXT NOT NULL)");
            // Create InitialLocationSQL table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS InitialLocationSQL (Id TEXT PRIMARY KEY, Longitude REAL, Latitude REAL , Date_Time TEXT NOT NULL )");
            // Create SegmentSQL table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS SegmentSQL (Id TEXT NOT NULL, Longitude REAL , Latitude REAL , Timespan_Seconds REAL NOT NULL)");
            // Create Servers table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS ServersSQL(ServerId TEXT PRIMARY KEY, ServerURL TEXT NOT NULL)");
            CloseConnection();
        }
        public SqliteDataBase(string nameOfSqlite)
        {
            mutex = new Mutex();
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();
            // The path of our sqlite that we want to create.
            connectionStringBuilder.DataSource = AppDomain.CurrentDomain.BaseDirectory + @"\" + nameOfSqlite;
            myConnection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            OpenConnection();
            // Create FlightPlanSQL table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS FlightPlanSQL (Id TEXT PRIMARY KEY NOT NULL, Passengers INTEGER DEFAULT 0, Company_name TEXT NOT NULL, Is_external TEXT NOT NULL)");
            // Create InitialLocationSQL table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS InitialLocationSQL (Id TEXT PRIMARY KEY, Longitude REAL DEFAULT 34.881505 , Latitude REAL DEFAULT 31.997999, Date_Time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP)");
            // Create SegmentSQL table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS SegmentSQL (Id TEXT NOT NULL, Longitude REAL DEFAULT 34.881505 , Latitude REAL DEFAULT 31.997999, Timespan_Seconds REAL NOT NULL DEFAULT 0)");
            // Create Servers table.
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS ServersSQL(ServerId TEXT PRIMARY KEY, ServerURL TEXT NOT NULL)");
            CloseConnection();
        }
        // Create new SQL table using a text that contains the comaptible creation command.
        private void CreateNewSQLTable(string commandOfCreatingSQLTable)
        {
            SqliteCommand tableCommand = myConnection.CreateCommand();
            tableCommand.CommandText = commandOfCreatingSQLTable;
            try
            {
                tableCommand.ExecuteReader();
            }
            catch (Exception) { }
        }
        // Delete SQL table using a text that contains the comaptible creation command.
        private void DeleteSQLTable(string commandOfCreatingSQLTable)
        {
            OpenConnection();
            SqliteCommand tableCommand = myConnection.CreateCommand();
            tableCommand.CommandText = commandOfCreatingSQLTable;
            try
            {
                tableCommand.ExecuteNonQuery();
            }
            catch (Exception) { }
            finally
            {
                CloseConnection();
            }

        }
        // Adding flightPlan to our SQL tables, and returns the id.
        public string AddFlightPlan(FlightPlan flightPlan)
        {
            if (flightPlan.Location == null || flightPlan.Segments == null) { return null; }
            OpenConnection();
            // Random id.
            string id = SetRandId();
            bool succeedToAddFlightPlan;
            succeedToAddFlightPlan = AddToFlightPlanTable(flightPlan, id);
            if (!succeedToAddFlightPlan) { return null; }
            succeedToAddFlightPlan = AddToInitialLocationTable(flightPlan, id);
            if (!succeedToAddFlightPlan) { return null; }
            succeedToAddFlightPlan = AddListToSegmentTable(flightPlan.Segments, id);
            if (!succeedToAddFlightPlan) { return null; }
            CloseConnection();
            return id;
        }
        private string SetRandId()
        {
            Random random = new Random();
            long randomNumber = random.Next(100000, 1000000000);
            return CreateRandomLetters(2) + randomNumber.ToString().Substring(0, 5) + CreateRandomLetters(3);
        }

        private string CreateRandomLetters(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // Add to FlightPlanSQL.
        private bool AddToFlightPlanTable(FlightPlan flightPlan, string id)
        {
            SqliteCommand addFlightPlanTableCommand = SetSqliteCommand("INSERT INTO FlightPlanSQL VALUES (@Id , @Passengers , @Company_name , @Is_external)");
            addFlightPlanTableCommand.Parameters.AddWithValue("@Id", id);
            addFlightPlanTableCommand.Parameters.AddWithValue("@Passengers", flightPlan.Passengers);
            addFlightPlanTableCommand.Parameters.AddWithValue("@Company_name", flightPlan.Company_name);
            addFlightPlanTableCommand.Parameters.AddWithValue("@Is_external", "false");
            try
            {
                addFlightPlanTableCommand.ExecuteReader();
                return true;
            }
            catch (Exception) { return false; }
        }
        // Add to InitialLocationSQL.
        private bool AddToInitialLocationTable(FlightPlan flightPlan, string id)
        {
            SqliteCommand addInitialLocationTableCommand = SetSqliteCommand("INSERT INTO InitialLocationSQL VALUES (@Id , @Longitude , @Latitude , @Date_Time)");
            addInitialLocationTableCommand.Parameters.AddWithValue("@Id", id);
            addInitialLocationTableCommand.Parameters.AddWithValue("@Longitude", flightPlan.Location.Longitude);
            addInitialLocationTableCommand.Parameters.AddWithValue("@Latitude", flightPlan.Location.Latitude);
            addInitialLocationTableCommand.Parameters.AddWithValue("@Date_Time", flightPlan.Location.Date_Time);
            try
            {
                addInitialLocationTableCommand.ExecuteReader();
                return true;
            }
            catch (Exception)
            {
                DeleteLineFromTable("DELETE FROM FlightPlanSQL WHERE Id=\"" + id + "\"");
                return false;
            }
        }
        // Function that takes list of Segment and insert it to SegmentSQL.
        private bool AddListToSegmentTable(List<Segment> segmentList, string id)
        {
            int i = 0;
            for (; i < segmentList.Count; ++i)
            {
                // Insert a row of Segment.
                bool succeedToAdd = AddRowToSegmentTable(segmentList[i], id);
                if (!succeedToAdd) { return false; }
            }
            return true;
        }
        // Add a row of Segment to SegmentSQL.
        private bool AddRowToSegmentTable(Segment segment, string id)
        {
            SqliteCommand addSegmentTableCommand = SetSqliteCommand("INSERT INTO SegmentSQL VALUES (@Id , @Longitude , @Latitude , @Timespan_Seconds)");
            addSegmentTableCommand.Parameters.AddWithValue("@Id", id);
            addSegmentTableCommand.Parameters.AddWithValue("@Longitude", segment.Longitude);
            addSegmentTableCommand.Parameters.AddWithValue("@Latitude", segment.Latitude);
            addSegmentTableCommand.Parameters.AddWithValue("@Timespan_Seconds", segment.Timespan_Seconds);
            try
            {
                addSegmentTableCommand.ExecuteReader();
                return true;
            }
            catch (Exception)
            {
                DeleteFlightPlanFromTable(id);
                return false;
            }
        }
        // Function that returns FlightPlan according to the given id.
        public FlightPlan GetFlightPlanById(string id)
        {
            // Take the line from FlightPlanSQL which the Id equals to id.
            object[] lineFlightPlanSQL = GetLineInformationFromSQLWithCommand("SELECT * FROM FlightPlanSQL WHERE Id=\"" + id + "\"");
            // If this id is not exist then return null. Otherwise, set a FlightPlan.
            if (lineFlightPlanSQL != null)
            {
                return SetFlightPlanByListObjects(lineFlightPlanSQL, id);
            }
            return null;
        }
        // Function that returns FlightPlan according to the given id in our database and in the other servers.
        public async Task<FlightPlan> GetFlightPlanByIdAndSync(string id)
        {
            FlightPlan flightPlan = GetFlightPlanById(id);
            if (flightPlan != null) { return flightPlan; }
            List<Server> listOfExternalServers = GetExternalServers();
            if (listOfExternalServers == null)
            {
                return null;
            }
            int i = 0;
            for (; i < listOfExternalServers.Count; ++i)
            {
                object[] idFlightFromExternalServer = GetLineInformationFromSQLWithCommand("SELECT IdFlight FROM " + listOfExternalServers[i].ServerId + " WHERE IdFlight=\"" + id + "\"");
                if (idFlightFromExternalServer == null) { continue; }
                string url = GetUrlWithoutSlashInTheEnd(listOfExternalServers[i].ServerURL);
                url += "api/FlightPlan/" + id;
                FlightPlan flightPlanFromExternalServer = await GetGenericFromAnotherServer<FlightPlan>(url);
                if (idFlightFromExternalServer != null) { return flightPlanFromExternalServer; }
            }
            return null;
        }
        // Function that get the objects and turns them to properties of FlightPlan.
        private FlightPlan SetFlightPlanByListObjects(object[] lineFlightPlanSQL, string id)
        {
            // Set the properties while using lineFlightPlanSQL and lineInitialLocationSQL.
            FlightPlan flightPlan = new FlightPlan
            {
                Passengers = Convert.ToInt32(lineFlightPlanSQL[1]),
                Company_name = lineFlightPlanSQL[2].ToString(),
                Location = GetInitialLocation(id)
            };
            // Set the Segment list while using listSegmentSQL.
            flightPlan.Segments = GetSegmentList(id);
            return flightPlan;
        }
        // Function that returns list of information according to the textCommand.
        private List<object[]> GetListOfInformationFromSQLWithCommand(string textCommand)
        {
            OpenConnection();
            // Set getFlightByIdCommand with textCommand and myConnection. 
            SqliteCommand getFlightByIdCommand = SetSqliteCommand(textCommand);
            try
            {
                SqliteDataReader sqliteDataReader = getFlightByIdCommand.ExecuteReader();
                List<object[]> list = new List<object[]>();
                // While there is something to read.
                while (sqliteDataReader.Read())
                {
                    // Set lineInformationFromSQL with the compatible values and add row to list.
                    object[] lineInformationFromSQL = new object[sqliteDataReader.FieldCount];
                    sqliteDataReader.GetValues(lineInformationFromSQL);
                    list.Add(lineInformationFromSQL);
                }
                // Close connection
                sqliteDataReader.Close();
                CloseConnection();
                if (list.Count == 0) { return null; }
                return list;
            }
            catch (Exception)
            {
                CloseConnection();
                return null;
            }
        }

        private object[] GetLineInformationFromSQLWithCommand(string textCommand)
        {
            OpenConnection();
            // Set getFlightByIdCommand with textCommand and myConnection.
            SqliteCommand getFlightByIdCommand = SetSqliteCommand(textCommand);
            try
            {
                SqliteDataReader sqliteDataReader = getFlightByIdCommand.ExecuteReader();
                object[] lineInformationFromSQL = new object[sqliteDataReader.FieldCount];
                // If there is something to read.
                if (sqliteDataReader.Read())
                {
                    // Set lineInformationFromSQL with the compatible values and add row to list.
                    sqliteDataReader.GetValues(lineInformationFromSQL);
                    sqliteDataReader.Close();
                    CloseConnection();
                    return lineInformationFromSQL;
                }
                // Close connection.
                CloseConnection();
                return null;
            }
            catch (Exception)
            {
                CloseConnection();
                return null;
            }
        }
        // Function that set the SqliteCommand with textCommand and myConnection.
        private SqliteCommand SetSqliteCommand(string textCommand)
        {
            return new SqliteCommand
            {
                Connection = myConnection,
                CommandText = textCommand
            };
        }
        // Function for delete flightPlan with the given id.
        public bool DeleteFlightPlanFromTable(string id)
        {
            // Check if this id is exist in our SQL table.
            object[] idFromFlightPlan = GetLineInformationFromSQLWithCommand("SELECT Id FROM FlightPlanSQL WHERE Id=\"" + id + "\"");
            if (idFromFlightPlan == null) { return false; }
            // Set tables with the names of our tables.
            string[] tables = { "FlightPlanSQL", "InitialLocationSQL", "SegmentSQL" };
            int i = 0;
            for (; i < tables.Length; i++)
            {
                DeleteLineFromTable("DELETE FROM " + tables[i] + " WHERE Id=\"" + id + "\"");
            }
            return true;
        }
        private void DeleteLineFromTable(string textCommand)
        {
            OpenConnection();
            SqliteCommand deleteCommand = new SqliteCommand();
            deleteCommand.Connection = myConnection;
            deleteCommand.CommandText = textCommand;
            try
            {
                deleteCommand.ExecuteReader();
            }
            catch (Exception) { }
            finally
            {
                CloseConnection();
            }
        }
        // Function that set the latitude and longitude according where the flight is.
        private void SetCurrentCoordinates(DateTime dateTimeRequest, DateTime startFlight, List<Segment> segmentList,
            Flights flight, InitialLocation initialLocation)
        {
            DateTime dateTimeTemp = startFlight;
            double latitude;
            double longitude;
            int sumOfSeconds = 0;
            int i = 0;
            // Check which segment we need now.
            do
            {
                dateTimeTemp = dateTimeTemp.AddSeconds(segmentList[i].Timespan_Seconds);
                sumOfSeconds += segmentList[i].Timespan_Seconds;
                i++;
            } while (dateTimeRequest.CompareTo(dateTimeTemp) >= 0 && i < segmentList.Count);
            // Get coordinates of start and end.double latitude1
            if (i >= 2)
            {
                latitude = segmentList[i - 2].Latitude;
                longitude = segmentList[i - 2].Longitude;
            }
            else
            {
                latitude = Convert.ToDouble(initialLocation.Latitude);
                longitude = Convert.ToDouble(initialLocation.Longitude);
            }
            DateTime temp1 = dateTimeRequest.AddSeconds(-(sumOfSeconds - segmentList[i - 1].Timespan_Seconds));
            double leftTime = (temp1 - startFlight).TotalSeconds;
            CalculateNewLongitudeLatitude(flight, latitude, longitude, leftTime, segmentList[i - 1]);
        }
        // Function for calculating the update of longitude and latitude according of given leftTime
        // And segment. In this Function we will calculate the partOfSegment that the flight pass for
        // Help calculate longitude and latitude.
        private void CalculateNewLongitudeLatitude(Flights flight, double givenLatitude, double givenLongitude,
            double leftTime, Segment segment)
        {
            double latitude = segment.Latitude;
            double longitude = segment.Longitude;
            double partOfSegment = leftTime / segment.Timespan_Seconds;
            flight.Latitude = GetCurrentLatitude(givenLatitude, latitude, partOfSegment);
            flight.Longitude = GetCurrentLongitude(givenLongitude, longitude, partOfSegment);
        }
        // Calculating the current latitude.
        private double GetCurrentLatitude(double latitude1, double latitude2, double partOfSegment)
        {
            return (latitude2 - latitude1) * partOfSegment + latitude1;
        }
        // Calculating the current longitude.
        private double GetCurrentLongitude(double longitude1, double longitude2, double partOfSegment)
        {
            return (longitude2 - longitude1) * partOfSegment + longitude1;
        }
        // Function that returns the segmentList according to given id.
        private List<Segment> GetSegmentList(string id)
        {
            List<object[]> objectSegmentList = GetListOfInformationFromSQLWithCommand("SELECT Longitude, Latitude, Timespan_Seconds FROM SegmentSQL WHERE Id=\"" + id + "\"");
            List<Segment> segmentList = ChangeListObjectToListSegment(objectSegmentList);
            return segmentList;
        }
        // Function that returns the initialLocation according to given id.
        private InitialLocation GetInitialLocation(string id)
        {
            object[] initialLocationOfIdFlight = GetLineInformationFromSQLWithCommand("SELECT Longitude, Latitude, Date_Time FROM InitialLocationSQL WHERE Id=\"" + id + "\"");
            return new InitialLocation
            {
                Longitude = Convert.ToDouble(initialLocationOfIdFlight[0].ToString()),
                Latitude = Convert.ToDouble(initialLocationOfIdFlight[1].ToString()),
                Date_Time = initialLocationOfIdFlight[2].ToString()
            };
        }
        // Function that takes list of ocject array and turns it to list of Segment.
        private List<Segment> ChangeListObjectToListSegment(List<object[]> objectSegmentList)
        {
            // If objectSegmentList is null, then return null.
            if (objectSegmentList == null) { return null; }
            List<Segment> segmentList = new List<Segment>();
            int i = 0;
            for (; i < objectSegmentList.Count; i++)
            {
                // Add segment to the new list.
                segmentList.Add(new Segment
                {
                    Longitude = Convert.ToDouble(objectSegmentList[i][0].ToString()),
                    Latitude = Convert.ToDouble(objectSegmentList[i][1].ToString()),
                    Timespan_Seconds = Convert.ToInt32(objectSegmentList[i][2].ToString())
                });
            }
            return segmentList;
        }
        // Function that returns flight with the updated values of latitude and longitude, and set the property
        // Is_external according to the given isExternal.
        private Flights AddFlightWithDateTimeAndIsExternal(string id, DateTime dateTime, string isExternal)
        {
            List<Segment> segmentList = GetSegmentList(id);
            InitialLocation initialLocation = GetInitialLocation(id);
            // Create flight with the given values.
            Flights flight = CreateFlight(isExternal, id, initialLocation.Date_Time);
            DateTime dateTimeOfIdFlight = ConvertToDateTime(initialLocation.Date_Time);
            // Set the current coordinates.
            SetCurrentCoordinates(dateTime, dateTimeOfIdFlight, segmentList, flight, initialLocation);
            return flight;
        }
        // Function that return only the internal flights according to the given dateTime.
        public List<Flights> GetFlightsByDateTime(string stringDateTime)
        {
            // Get all internal flights.
            List<Flights> internalFlightList = GetActivatedFlightsIsExternal(stringDateTime, "false");
            return internalFlightList;
        }
        // Function that returns the internal and external flights according to the given dateTime.
        public async Task<List<Flights>> GetFlightsByDateTimeAndSync(string stringDateTime)
        {
            // Get the internal flights.
            List<Flights> internalFlightList = GetFlightsByDateTime(stringDateTime);
            // Get the external flights.
            List<Flights> externalFlightList = await GetFlightsFromServersTable(stringDateTime);
            // There are no external or internal flights that compatible to the givven dateTime.
            if (externalFlightList == null && internalFlightList == null) { return null; }
            // There are external and internal flights that compatible to the givven dateTime.
            if (externalFlightList != null && internalFlightList != null)
            {
                externalFlightList.AddRange(internalFlightList);
                return externalFlightList;
            }
            // There are only external flights that compatible to the givven dateTime.
            if (externalFlightList != null) { return externalFlightList; }
            // There are only internal flights that compatible to the givven dateTime.
            return internalFlightList;
        }
        // Function that return list of activated Flights according to given stringDateTime and external/internal.
        private List<Flights> GetActivatedFlightsIsExternal(string stringDateTime, string isExternal)
        {
            List<Flights> flightList = new List<Flights>();
            DateTime dateTime = ConvertToDateTime(stringDateTime);
            // Get all flights according to given isExternal - internal/external.
            List<string> idList = GetIdListOfExternalOrInternal(isExternal);
            // Get id list of flights according to given isExternal - internal/external.
            List<string> idListActivated = GetIdListActivated(idList, dateTime);
            if (idListActivated == null) { return null; }
            int i = 0;
            // Add the external flights.
            for (; i < idListActivated.Count; ++i)
            {
                flightList.Add(AddFlightWithDateTimeAndIsExternal(idListActivated[i], dateTime, isExternal));
            }
            return flightList;
        }
        // Function that creates Flight.
        public Flights CreateFlight(string isExternal, string id, string dateTime)
        {
            // Set variables according to the given id.
            object[] longitude = GetLineInformationFromSQLWithCommand("SELECT Longitude FROM InitialLocationSQL WHERE id=\"" + id + "\"");
            object[] latitude = GetLineInformationFromSQLWithCommand("SELECT Latitude FROM InitialLocationSQL WHERE id=\"" + id + "\"");
            object[] passengers = GetLineInformationFromSQLWithCommand("SELECT Passengers FROM FlightPlanSQL WHERE id=\"" + id + "\"");
            object[] companyName = GetLineInformationFromSQLWithCommand("SELECT Company_name FROM FlightPlanSQL WHERE id=\"" + id + "\"");
            // Set new Flight.
            return new Flights
            {
                Flight_id = id,
                Longitude = Convert.ToDouble(longitude[0]),
                Latitude = Convert.ToDouble(latitude[0]),
                Passengers = Convert.ToInt32(passengers[0].ToString()),
                Company_name = companyName[0].ToString(),
                Date_time = dateTime,
                Is_external = isExternal
            };
        }
        // Function that returns id list of the external/internal flights according to the given isExternal.
        public List<string> GetIdListOfExternalOrInternal(string isExternal)
        {
            List<object[]> list = GetListOfInformationFromSQLWithCommand("SELECT Id FROM FlightPlanSQL WHERE Is_external=\"" + isExternal + "\"");
            if (list == null) { return null; }
            int i = 0;
            List<string> idList = new List<string>();
            for (; i < list.Count; ++i)
            {
                idList.Add(list[i][0].ToString());
            }
            return idList;
        }
        // Function that returns id list only of activated flights, that is mean that the flight is already started
        // But did not finish yet.
        public List<string> GetIdListActivated(List<string> idListOfExternalOrInternal, DateTime dateTimeFromClient)
        {
            int i = 0;
            List<string> idListOfActivatedFlights = new List<string>();
            if (idListOfExternalOrInternal == null) { return null; }
            for (; i < idListOfExternalOrInternal.Count; ++i)
            {
                object[] rowOfId = GetLineInformationFromSQLWithCommand("SELECT * FROM InitialLocationSQL WHERE Id=\"" + idListOfExternalOrInternal[i] + "\"");
                if (rowOfId != null)
                {
                    // Check if the flight is already started but did not finish yet.
                    if (CheckIfFlightIsAlreadyStarted(rowOfId, dateTimeFromClient)
                        && CheckIfFlightIsNotAlreadyFinished(rowOfId, dateTimeFromClient))
                    {
                        idListOfActivatedFlights.Add(rowOfId[0].ToString());
                    }
                }
            }
            return idListOfActivatedFlights;
        }
        // Boolean function that returns true or false about if the flight is already started.
        public bool CheckIfFlightIsAlreadyStarted(object[] rowOfId, DateTime dateTimeFromClient)
        {
            DateTime dateTimeOfFlightPlan = ConvertToDateTime(rowOfId[3].ToString());
            // Check if this dateTimeOfFlightPlan did not start yet.
            if (DateTime.Compare(dateTimeFromClient, dateTimeOfFlightPlan) < 0)
            {
                return false;
            }
            // Already started.
            return true;
        }
        // Boolean function that returns true or false about if the flight is not already finished.
        private bool CheckIfFlightIsNotAlreadyFinished(object[] rowOfId, DateTime dateTimeFromClient)
        {
            DateTime dateTimeOfFlightPlan = ConvertToDateTime(rowOfId[3].ToString());
            // Sum the Timespan_Seconds for calculating the finish time of flight.
            object[] sumOfTimespanSeconds = GetLineInformationFromSQLWithCommand("SELECT SUM(Timespan_Seconds) FROM SegmentSQL WHERE Id=\"" + rowOfId[0].ToString() + "\"");
            // Problem with specific id.
            if (sumOfTimespanSeconds == null)
            {
                return false;
            }
            int seconds = Convert.ToInt32(sumOfTimespanSeconds[0]);
            DateTime dateTimeOfFinishFlight = dateTimeOfFlightPlan.AddSeconds(seconds);
            // Check if flight finished before the dateTimeFromClient. If it is less then zero, so it is false.
            // Else, return true, because it did not finish yet.
            return !(DateTime.Compare(dateTimeOfFinishFlight, dateTimeFromClient) < 0);
        }
        // Function that add server to Servers.
        public string AddServer(Server server)
        {
            server.ServerId = SetRandId();
            OpenConnection();
            AddToServersSQL(server);
            CloseConnection();
            return server.ServerId;
        }
        // Function that responsible on the communication with Servers in order to add server to Servers.
        private void AddToServersSQL(Server server)
        {
            SqliteCommand addServersTableCommand = SetSqliteCommand("INSERT INTO ServersSQL VALUES (@ServerId , @ServerURL)");
            addServersTableCommand.Parameters.AddWithValue("@ServerId", server.ServerId);
            addServersTableCommand.Parameters.AddWithValue("@ServerURL", server.ServerURL);
            try
            {
                addServersTableCommand.ExecuteReader();
            }
            catch (Exception) { }
        }
        // Function that returns all the flights from the external servers.
        private async Task<List<Flights>> GetFlightsFromServersTable(string stringDateTime)
        {
            int i = 0;
            List<Flights> flightsListFromAllExternalServers = new List<Flights>();
            List<Server> serverIdList = GetExternalServers();
            // There are not external servers.
            if (serverIdList == null) { return null; }
            for (; i < serverIdList.Count; ++i)
            {
                // Set the url for getting the flight list.
                string url = GetUrlWithoutSlashInTheEnd(serverIdList[i].ServerURL);
                url += "/api/Flight?relative_to=" + stringDateTime;
                List<Flights> flightsFromOtherServer = await GetGenericFromAnotherServer<List<Flights>>(url);
                CreateOrDeleteSQLFlightFromOtherServer(flightsFromOtherServer, serverIdList[i].ServerId);
                flightsFromOtherServer = ChangeToExternal(flightsFromOtherServer);
                // If flightsFromOtherServer is not null, so merge between the lists.
                flightsListFromAllExternalServers.AddRange(flightsFromOtherServer);
            }
            return flightsListFromAllExternalServers;
        }
        // Function that checks if the last char in url is '/', and if it is correct then delete it.
        private string GetUrlWithoutSlashInTheEnd(string urlToCheck)
        {
            if (urlToCheck.EndsWith('/'))
            {
                return urlToCheck.Substring(0, urlToCheck.LastIndexOf('/'));
            }
            return urlToCheck;
        }
        // Function that deletes and creates a table in the name of ServerId and updates the fields.
        private void CreateOrDeleteSQLFlightFromOtherServer(List<Flights> flightsFromOtherServer, string idOfServer)
        {
            DeleteSQLTable("DROP TABLE IF EXISTS " + idOfServer);
            if (flightsFromOtherServer == null) { return; }
            OpenConnection();
            CreateNewSQLTable("CREATE TABLE IF NOT EXISTS " + idOfServer + " (IdFlight TEXT PRIMARY KEY NOT NULL)");
            int i = 0;
            for (; i < flightsFromOtherServer.Count; ++i)
            {
                AddFlightIdToNameOfServerSQL(flightsFromOtherServer[i].Flight_id, idOfServer);
            }
            CloseConnection();
        }
        // Function that responsible on the communication with Servers in order to add server to Servers.
        private void AddFlightIdToNameOfServerSQL(string flightId, string idOfServer)
        {
            SqliteCommand addServersTableCommand = SetSqliteCommand("INSERT INTO " + idOfServer + " VALUES (@IdFlight)");
            addServersTableCommand.Parameters.AddWithValue("@IdFlight", flightId);
            try
            {
                addServersTableCommand.ExecuteReader();
            }
            catch (Exception) { }
        }
        // Generic function that returns list of information from one external server, while asking from http
        // The information we want to get.
        private async Task<T> GetGenericFromAnotherServer<T>(string externalUrlServer)
        {
            string url = String.Format(externalUrlServer);
            WebRequest requestObject = WebRequest.Create(url);
            requestObject.Method = "GET";
            HttpWebResponse responseObject = null;
            responseObject = (HttpWebResponse)await requestObject.GetResponseAsync();
            string resultTest = null;
            // Get the data from the url.
            using (Stream stream = responseObject.GetResponseStream())
            {
                StreamReader streamReader = new StreamReader(stream);
                resultTest = streamReader.ReadToEnd();
                streamReader.Close();
            }
            // If there is not information then return null.
            if (resultTest == null || resultTest == "") { return default; }
            // Otherwise, we need to deserialize the string we have.
            T genericListFromHttpResponse = JsonConvert.DeserializeObject<T>(resultTest);
            return genericListFromHttpResponse;
        }
        // Function that changes the property Is_external to true.
        private List<Flights> ChangeToExternal(List<Flights> flightList)
        {
            // Update Is_external to be "true".
            foreach (Flights item in flightList)
            {
                item.Is_external = "true";
            }
            return flightList;
        }
        // Function that return all external servers.
        public List<Server> GetExternalServers()
        {
            List<object[]> serverObjectList = GetListOfInformationFromSQLWithCommand("SELECT * FROM ServersSQL");
            if (serverObjectList == null) { return null; }
            List<Server> serverList = new List<Server>();
            int i = 0;
            for (; i < serverObjectList.Count; ++i)
            {
                serverList.Add(new Server
                {
                    ServerId = serverObjectList[i][0].ToString(),
                    ServerURL = serverObjectList[i][1].ToString()
                });
            }
            return serverList;
        }
        // Function that delete server with this specific id.
        public bool DeleteServer(string id)
        {
            object[] idServer = GetLineInformationFromSQLWithCommand("SELECT ServerId FROM ServersSQL WHERE ServerId=\"" + id + "\"");
            if (idServer == null) { return false; }
            DeleteLineFromTable("DELETE FROM ServersSQL WHERE ServerId=\"" + id + "\"");
            return true;
        }
        // Function that takes a string that represents the dateTime and turns it to varaible in typeOf DateTime.
        private DateTime ConvertToDateTime(string stringDateTime)
        {
            // The pattern we asked for.
            string pattern = @"(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})Z";
            if (Regex.IsMatch(stringDateTime, pattern))
            {
                Match match = Regex.Match(stringDateTime, pattern);
                int year = Convert.ToInt32(match.Groups[1].Value);
                int month = Convert.ToInt32(match.Groups[2].Value);
                int day = Convert.ToInt32(match.Groups[3].Value);
                int hour = Convert.ToInt32(match.Groups[4].Value);
                int minute = Convert.ToInt32(match.Groups[5].Value);
                int second = Convert.ToInt32(match.Groups[6].Value);
                return new DateTime(year, month, day, hour, minute, second);
            }
            else
            {
                throw new Exception("Unable to parse.");
            }
        }
        // Function for openning the connection, if the connection is not already open.
        public void OpenConnection()
        {
            mutex.WaitOne();
            myConnection.Open();
        }
        // Function for closing the connection, if the connection is not already close.
        public void CloseConnection()
        {
            myConnection.Close();
            mutex.ReleaseMutex();
        }
    }
}