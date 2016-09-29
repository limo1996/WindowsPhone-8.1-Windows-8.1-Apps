/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: 
//=> be careful about correct input strings  -> check function create                                   @TEST
//=> return overall time and range                                                                      @
//=> add new items to returned tuple                                                                    @
//=> make tuple used like <string[], string[], string[], int, int>                                      @
//=> implement FindDepartureTime and Arrival time into PathBuilder                                      @
//=> make autocorrection through sqlite database                                                        /@TEST
//=> make new form on adding lines and so on..
//=> GUI in WPF
//=> comment heavily all methods                                                                        @
//=> make login                                                                                         @TEST
//=> rework get time and other methods that they will return DateTime                                   @
//=> only direct connections, just search lines wether are two searching stations on the same line      @
//=> departure time make more clever                                                                    @
//=> finish static method to get TimeSpan into string
//=> finish departure time, errors and load graph                                                       @
//=> make class that will save lines into databases and check for correct user inputs
//=> think when should be graph loaded -> make this program as fast as possible
//=> WPF, WPF, WPF learn learn learn
//=> findShortestWay if there are two shortest ways ??
//=> button -> click -> tap location -> automaticaly search for closest station

namespace Timetable
{
   
    /// <summary>
    /// class that reads informations from database and connect it to the final route made by Graph
    /// </summary>
    class PathBuilder
    {
        Graph CP;
        SQLiteLoader sqliteLoader;

        /// <summary>
        /// Only Constructor.. databaseName is neccessary
        /// </summary>
        /// <param name="databaseName"></param>
        public PathBuilder(string databaseName)
        {
            SQLiteLoader.DbName = databaseName;
            sqliteLoader = new SQLiteLoader();
            CP = new Graph();
            CP.LoadGraph();
            CP.LoadPositionOfStop();
        }

        public PathBuilder()
        {
            sqliteLoader = new SQLiteLoader();
            CP = new Graph();
            CP.LoadGraph();
            CP.LoadPositionOfStop();
        }

        public string DatabaseName
        {
            get { return SQLiteLoader.DbName; }
            set { SQLiteLoader.DbName = value; }
        }

        public bool ExistWay(string start, string end)
        {
            return CP.ExistsWay(start, end);
        }

        /// <summary>
        /// returns TimeSpan in string, Format like: "10 hours, 4 minutes, 3 seconds"
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string TimeSpanToString(TimeSpan timeSpan)
        {
            //TODO
            return string.Empty;
        }

        /// <summary>
        /// reads from database autocorrection strings
        /// </summary>
        /// <returns></returns>
        public string[] GetAutocorrectionStrings()
        { 
            List<string> returned = new List<string>();
            var table = sqliteLoader.GetAutocorrectionStrings();
            foreach (var line in table)
            {
                returned.Add(line.AutocorrectionString);
            }
            return returned.ToArray();
        }

        /// <summary>
        /// Normal log in
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CheckLogIn(string userName, string password)
        { 
            var table = sqliteLoader.GetUserData();
            foreach (var item in table)
            { 
                if(item.UserName == userName && item.UserPassword == password)
                {
                    return true;
                }
            }
            return false;
        }

        public User GetUser(string userName, string password)
        {
            UserData data = sqliteLoader.GetUserData(userName, password);
            return new User()
            {
                ID = data.ID,
                UserName = data.UserName,
                FullName = data.FullName,
                UserPassword = data.UserPassword
            };
        }

        /// <summary>
        /// Change diacritics for internal purposes
        /// </summary>
        /// <param name="wordToChange"></param>
        /// <returns></returns>
        public static string ChangeDiacritics(string wordToChange)
        {
            string finalWord = wordToChange;
            finalWord = finalWord.ToLower();
            finalWord = finalWord.Replace(" ", "");
            finalWord = finalWord.Replace("\n", "");
            finalWord = finalWord.Replace("\t", "");
            finalWord = finalWord.Replace("š", "s");
            finalWord = finalWord.Replace("ľ", "l");
            finalWord = finalWord.Replace("č", "c");
            finalWord = finalWord.Replace("ť", "t");
            finalWord = finalWord.Replace("ž", "z");
            finalWord = finalWord.Replace("ý", "y");
            finalWord = finalWord.Replace("á", "a");
            finalWord = finalWord.Replace("í", "i");
            finalWord = finalWord.Replace("é", "e");
            finalWord = finalWord.Replace("ď", "d");
            finalWord = finalWord.Replace("ň", "n");
            finalWord = finalWord.Replace("ú", "u");
            finalWord = finalWord.Replace("ó", "o");
            finalWord = finalWord.Replace("ô", "o");

            return finalWord;
        }

        /// <summary>
        /// Check wether exists stations tapped by user or not. Prevents later errors in the code.
        /// </summary>
        /// <param name="station1"></param>
        /// <param name="station2"></param>
        /// <returns></returns>
        public bool CheckCorrectInputStations(string station1, string station2)
        {
            string[] stops = sqliteLoader.GetStopNames();

            bool station1Exists = false, station2Exists = false;

            foreach (string stop in stops)
            {
                if (stop == station1)
                    station1Exists = true;
                if (stop == station2)
                    station2Exists = true;
            }

            if (station1Exists && station2Exists)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stations">stations of the shortest way</param>
        /// <param name="tableOfRoutes"></param>
        /// <param name="columnOfRoutes"></param>
        /// <returns>tuple of</returns>
        private Tuple<string[],string[][]> FilterStations(string[] stations)
        {
            if (stations == null)
                return null;

            Tuple<string, string[]>[] myData = new Tuple<string, string[]>[stations.Length];
            int counter = 0;
            foreach (string station in stations)
            {
                string[] tmp = sqliteLoader.GetRoutes(station);
                List<string> Routes = new List<string>();
                foreach(var item in tmp)
                {
                    if (item != null && item != "")
                        Routes.Add(item);

                }
                //string[] Routes = tmp[0].Split(',');//error
                myData[counter] = new Tuple<string, string[]>(station, Routes.ToArray());                
                counter++;
            }

            List<string> hashset = new List<string>();
            counter = 0;
            string[] inter = myData[0].Item2;
            string[] itinerary = null;
            hashset.Add(myData[counter].Item1);

            bool continued = false;
            List<string[]> stopItinerary = new List<string[]>();

            while (counter < myData.Length)
            {
                while (inter != null)
                {

                    if (counter == myData.Length)
                    {
                        continued = true;
                        break;
                    }
                    inter = Intersection(inter, myData[counter].Item2);
                    if (inter != null)
                        itinerary = inter;
                    counter++;
                }
                if (continued)
                    break;
                hashset.Add(myData[counter - 2].Item1);
                stopItinerary.Add(itinerary);
                inter = myData[counter - 2].Item2;
                counter--;
            }
            hashset.Add(myData[myData.Length - 1].Item1);
            stopItinerary.Add(itinerary);
            string[] newPath = new string[hashset.Count];
            hashset.CopyTo(newPath);
            return new Tuple<string[], string[][]> (newPath, stopItinerary.ToArray());
        }

        /// <summary>
        /// performs intersection of two sets
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        private static string[] Intersection(string[] set1, string[] set2)
        {
            List<string> intersection = new List<string>();

            foreach (string s1 in set1)
            {

                foreach (string s2 in set2)
                {
                    if (s1 == s2)
                        intersection.Add(s1);
                }
            }
            if (intersection.Count != 0)
                return intersection.ToArray();
            else
                return null;
        }

        public Tuple<string[], string[]> GetPathInfo(string line,DateTime time, string station1, string station2)
        {
            string[] stations = sqliteLoader.GetStations(line);
            for (int i = 0 ; i < stations.Length; i++ )
            {
                if (stations[i] == station1)
                {
                    if (i + 1 >= stations.Length || stations[i + 1] != station2)
                    {
                        stations = stations.Reverse().ToArray();
                        break;
                    }
                }
            }
            string[] arrivalDepartures = new string[stations.GetLength(0)];
            DateTime startTime = GetTime(line, time);
            arrivalDepartures[0] = string.Format("{0:00}",startTime.Hour) + ":"
                + string.Format("{0:00}",startTime.Minute);
            for (int i = 1; i < stations.GetLength(0); i++)
            {
                startTime = startTime.AddMinutes(sqliteLoader.GetTimeSpan(stations[i - 1], stations[i]));
                arrivalDepartures[i] = string.Format("{0:00}",startTime.Hour) + ":"
                    + string.Format("{0:00}",startTime.Minute);
            }

            return Tuple.Create<string[], string[]>(stations, arrivalDepartures);
        }

       /// <summary>
       /// main function that calculates the shortest path. Departures and transfers.
       /// </summary>
       /// <param name="time">time from we are searching</param>
       /// <param name="start">start station</param>
       /// <param name="end">end station</param>
       /// <param name="criterium">criterium</param>
       /// <param name="onlyDirectConnections">Only direct connection ?</param>
       /// <returns>Tuple: Item1:Stations, Item2: Lines, Item3: Departure and ArrivalTime, Item4: Overall Distance, Item5: OverallTime</returns>
        public Tuple<string[], string[], DateTime[], int, TimeSpan> BuildPath(DateTime time, string start, string end, CRIT criterium, 
            bool onlyDirectConnections)
        {
            Tuple<string[], string[][]> stationAndRoute;
            if (!CP.ExistsWay(start, end))
                return null;

            if(onlyDirectConnections)
            {
                var tmp = CP.FindShortestDirectWay(start, end, criterium);
                if (tmp != null)
                    stationAndRoute = FilterStations(tmp.Item1);
                else
                    stationAndRoute = null;
            }              
            else
                stationAndRoute = FilterStations(CP.FindShortestWay(start, end, criterium).Item1);

            if (stationAndRoute == null)
                return null;

            string[] Item1 = PrepareItem1(stationAndRoute.Item1);
            string[] Item2 = new string[Item1.Length];
            DateTime[] Item3 = new DateTime[Item1.Length];

            Tuple<string[], DateTime[]> returned = PrepareItem2And3(stationAndRoute.Item2, time, Item1);

            Item2 = returned.Item1;
            Item3 = returned.Item2;

            TimeSpan overallTime = Item3[Item3.Length - 1] - Item3[0];

            return new Tuple<string[], string[], DateTime[], int, TimeSpan>(Item1, Item2, Item3, 
                                CP.FindShortestWay(start, end, criterium).Item2, overallTime);
        }

        private string[] PrepareItem1(string[] way)
        {
            List<string> returned = new List<string>();
            returned.Add(way[0]);
            for (int i = 1; i < way.Length - 1; i++)
            {
                returned.Add(way[i]);
                returned.Add(way[i]);
            }
            returned.Add(way[way.Length-1]);
            return returned.ToArray();
        }

        /// <summary>
        /// Prepares items 2 and 3 of the final tuple in BuildPath
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="tableOfDepartures"></param>
        /// <param name="date"></param>
        /// <param name="columnNameOfStop"></param>
        /// <param name="columnIdOfRoute"></param>
        /// <param name="way"></param>
        /// <returns>string array of lines and array of DateTimes(departure and arrival times)</returns>
        private Tuple<string[],DateTime[]> PrepareItem2And3(string[][] routes, DateTime date,string[] way)
        {
            int counter = 0;
            string[] item2 = new string[way.Length];
            DateTime[] item3 = new DateTime[way.Length];

            DateTime actual = this.GetTime(routes[0][0], date);

            foreach (string[] line in routes)
            {
                if (line == null)
                    return null;
                for (int i = 0; i < 2; i++)
                {
                    if (line.GetLength(0) == 1)
                    {
                        item2[counter] = line[0];
                        if (counter % 2 == 0)
                        {
                            actual = CP.FindDepartureTime(actual, way[counter + 1], way[counter], line[0], false);
                            item3[counter] = actual;
                        }
                        else
                        {
                            actual = CP.FindArrivalTime(actual, line[0], way[counter - 1], way[counter]);
                            item3[counter] = actual;
                        }
                        counter++;
                    }
                    else
                    {
                        string shortestRoute = this.RoutesComparer(line, date);
                        item2[counter] = shortestRoute;
                        if (counter % 2 == 0)
                        {
                            actual = CP.FindDepartureTime(actual, way[counter + 1], way[counter], line[0], false);
                            item3[counter] = actual;
                        }
                        else
                        {
                            actual = CP.FindArrivalTime(actual, line[0], way[counter - 1], way[counter]);
                            item3[counter] = actual;
                        }
                        counter++;
                    }
                }

            }
            return new Tuple<string[], DateTime[]>(item2,item3);
        }

        //finds shortest route via given argumets
        private string RoutesComparer(string[] Routes, DateTime date)
        {
            Tuple<string, int>[] comparedField = new Tuple<string, int>[Routes.Length];
            int counter = 0;
            foreach (string route in Routes)
            {
                DateTime tmp = GetTime(route,date);
                int compValue = tmp.Hour * 100 + tmp.Minute;
                comparedField[counter] = new Tuple<string, int>(route, compValue);
                counter++;
            }

            List<Tuple<string, int>> sortArray = new List<Tuple<string, int>>(comparedField);
            sortArray.Sort((x,y)=>y.Item2.CompareTo(x.Item2));
            return sortArray[0].Item1;
        }

        //get time of departure from start station from selected time
        public DateTime GetTime(string Route, DateTime date)
        { 
            int hourInt = date.Hour;
            int? minutesInt = 0;
            bool continued = true;

            while (true)
            {
                int? selected = sqliteLoader.GetDepartureTime(Route, hourInt);
                minutesInt = selected;
                if (selected == null)
                {
                    date = date.AddHours(1);
                    hourInt++;
                    if (hourInt > 23)
                        hourInt = 0;
                    continued = false;
                }
                else
                {
                    if (selected < date.Minute && continued)
                    {
                        minutesInt = selected;
                        date = date.AddHours(1);
                        hourInt++;
                        if (hourInt > 23)
                            hourInt = 0;
                    }
                    else
                    {
                        break;
                    }
                    continued = false;
                }
            }
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, (int)minutesInt, 0);
        }

        
    }
}
