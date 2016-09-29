/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SQLite;
using System.IO;

namespace Timetable
{
        /// <summary
        /// Class that holds operations with SQLite database needed in my project.
        /// saving, loading, deleting
        /// </summary>
        class SQLiteLoader 
        {
            private string databaseName;
            public string DatabaseName 
            {
                get
                {
                    return databaseName;
                }
                set
                {
                    databaseName = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, value);
                }
            }

            public static string DbName = "TimeTable.sqlite";

            public SQLiteLoader()
            {
                DatabaseName = SQLiteLoader.DbName;
            }

            public SQLiteLoader(string databaseName)
            {
                DatabaseName = databaseName;
            }

            public Departures[] GetTableDepartures()
            {
                try
                {
                    using (var database = new SQLiteConnection(this.databaseName))
                    {
                        database.CreateTable<Departures>(CreateFlags.None);
                        var table = database.Table<Departures>();
                        return table.ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public void Reset()
            {
                try
                {
                    using (var database = new SQLiteConnection(this.databaseName))
                    {
                        database.CreateTable<Departures>();
                        database.CreateTable<PositionOfStop>();
                        database.CreateTable<UserData>();
                        database.CreateTable<GraphTimetable>();
                        database.CreateTable<AutocorrectionStrings>();

                        database.DeleteAll<Departures>();
                        database.DeleteAll<PositionOfStop>();
                        database.DeleteAll<UserData>();
                        database.DeleteAll<GraphTimetable>();
                        database.DeleteAll<AutocorrectionStrings>();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }
            }

            public PositionOfStop[] GetTablePositionOfStop()
            {
                try
                {
                    using (var database = new SQLiteConnection(this.databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        var table = database.Table<PositionOfStop>();
                        return table.ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public AutocorrectionStrings[] GetAutocorrectionStrings()
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<AutocorrectionStrings>();
                        return database.Table<AutocorrectionStrings>().ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public GraphTimetable[] GetTableGraph()
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<GraphTimetable>();
                        var table = database.Table<GraphTimetable>();
                        return table.ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public string[] GetStopNames()
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        var data = database.Table<PositionOfStop>().ToArray();
                        return (from item in data select item.Stop).ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public string[] GetStopRoutes(string stop)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        var data = database.Table<PositionOfStop>().ToArray();
                        var selected = (from item in data where item.Stop == stop select item.Stops).ToArray();
                        return selected[0].Split(',');
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
                catch (Exception ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public int? GetDepartureTime(string line, int hour)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<Departures>();
                        var data = database.Table<Departures>().ToArray();
                        int? check = (from item in data where item.IdOfLine == line select item.DeparturesInArray[hour]).First();
                        if (check == 60)
                            return null;
                        else
                            return check;
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return -1;
                }
                catch (Exception ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return -1;
                }
            }

            public UserData[] GetUserData()
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<UserData>();
                        return database.Table<UserData>().ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }
            public UserData GetUserData(string username, string password)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<UserData>();
                        var ret = from item in database.Table<UserData>()
                                  where item.UserName == username && item.UserPassword == password
                                  select item;
                        if (ret != null && ret.Count() > 0)
                            return ret.First();
                        else
                            return null;
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public string[] GetRoutes(string stop)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        var ret = from item in database.Table<PositionOfStop>().ToArray() 
                                where item.Stop == stop select item.Stops;
                        if (ret != null && ret.Count() > 0)
                            return ret.First().Split(',');
                        else
                            return null;
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }
            public string[] GetStations(string line)
            {
                HashSet<string> stations = new HashSet<string>();
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        var query = from item in database.Table<GraphTimetable>()
                                    where item.IdOfLine == line
                                    select item;
                        foreach (var item in query)
                        {
                            stations.Add(item.PlaceFrom);
                            stations.Add(item.PlaceTo);
                        }
                        return stations.ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }
            public int GetTimeSpan(string fromStation, string toStation)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<GraphTimetable>();
                        var ret = from item in database.Table<GraphTimetable>()
                                  where item.PlaceFrom == fromStation && item.PlaceTo == toStation
                                  select item;
                        if (ret != null && ret.Count() > 0)
                        {
                            int i = ret.First().Time;
                            return ret.First().Time;
                        }
                        else
                            return 0;
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return 0;
                }            
            }

            public string[] GetIdsOfLines()
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<Departures>();
                        var query = (from item in database.Table<Departures>()
                                select item).ToArray();
                        return (from item in query
                               select item.IdOfLine).ToArray();
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                } 
            }

            public void SaveAutocorrectionString(string station)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<AutocorrectionStrings>();
                        if ((from item in database.Table<AutocorrectionStrings>()
                             where item.AutocorrectionString == station
                             select item).Count() == 0)
                        {
                            database.Insert(new AutocorrectionStrings()
                            {
                                AutocorrectionString = station
                            });
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }       
            }

            public void SaveDeparture(Departures departure)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<Departures>();
                        database.Insert(departure);
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }   
            }

            public void SavePositionOfStop(PositionOfStop positionOfStop)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        database.Insert(positionOfStop);
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }   
            }

            public void SaveGraphTimetable(GraphTimetable graphTimetable)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<GraphTimetable>();
                        database.Insert(graphTimetable);
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }   
            }

            public void DeletePositionOfStop(string stop)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.Delete<PositionOfStop>(stop);
                    }
                }
                catch(SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }
            }

            public PositionOfStop GetPositionOfStop(string stop)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<PositionOfStop>();
                        var ret = from item in database.Table<PositionOfStop>()
                                where item.Stop == stop
                                select item;
                        if (ret != null && ret.Count() > 0)
                            return ret.First();
                        else
                            return null;
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return null;
                }
            }

            public void SaveUserData(UserData userData)
            {
                try
                {
                    using (var database = new SQLiteConnection(databaseName))
                    {
                        database.CreateTable<UserData>();
                        database.Insert(userData);
                    }
                }
                catch (SQLiteException ex)
                {
                    Timetable.Code.FileLogger.WriteFile("TimetableErrorMessages.txt", ex.Message + "\n" + ex.HelpLink);
                    return;
                }
            }

            public void DeleteLine(string IdOfLine)
            {
                if (IdOfLine == null || IdOfLine == "")
                    return;
                using (var database = new SQLiteConnection(Path.Combine(
                    Windows.Storage.ApplicationData.Current.LocalFolder.Path, SQLiteLoader.DbName)))
                {
                    var query = from item in database.Table<GraphTimetable>()
                                where item.IdOfLine == IdOfLine
                                select item;

                    HashSet<string> stopsToDelete = new HashSet<string>();
                    List<PositionOfStop> Stops = new List<PositionOfStop>();

                    foreach (var item in query)
                    {
                        stopsToDelete.Add(item.PlaceFrom);
                        stopsToDelete.Add(item.PlaceTo);
                    }

                    SQLiteCommand cmd = new SQLiteCommand(database);
                    cmd.CommandText = "DELETE FROM GraphTimetable WHERE IdOfLine == '" + IdOfLine + "';";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM Departures WHERE IdOfLine == '" + IdOfLine + "';";
                    cmd.ExecuteNonQuery();
                    foreach (var stop in stopsToDelete)
                    {
                        cmd.CommandText = "DELETE FROM PositionOfStop WHERE Stops == '" + IdOfLine + ",';";
                        cmd.ExecuteNonQuery();

                        if ((from item in database.Table<PositionOfStop>()
                             where item.Stop == stop
                             select item).Count() > 0)
                        {
                            var tmp = (from item in database.Table<PositionOfStop>()
                                       where item.Stop == stop
                                       select item).First();

                            if (tmp.Stops.Contains(IdOfLine + ","))
                            {
                                database.Delete(tmp);
                                tmp.Stops = tmp.Stops.Replace(IdOfLine + ",", "");
                                database.CreateTable<PositionOfStop>();
                                database.Insert(tmp);
                            }
                        }
                    }
                }
            }
        }

    /// <summary>
    /// Represent table's Departures head 
    /// </summary>
        [Table("Departures")]
        public class Departures
        {
            [PrimaryKey, Column("IdOfLine")]
            public string IdOfLine { get; set; }

            [Column("Departures")]
            public string DeparturesInString { get; set; }
            [Ignore]
            public int?[] DeparturesInArray
            {
                set
                {
                    DeparturesInArray = value;
                }
                get
                {
                    string[] splittedArray = DeparturesInString.Split(',');
                    List<int?> ToBeReturned = new List<int?>();
                    foreach(string item in splittedArray)
                    {
                        if (item != "")
                        {
                            if (item == "60")
                                ToBeReturned.Add(null);
                            else
                                ToBeReturned.Add(int.Parse(item));
                        }
                    }
                    return ToBeReturned.ToArray();
                }

            }

            public Departures()
            { 
            
            }

            public Departures(int?[] departures)
            { 
                if(departures.Length != 24)
                {
                    throw new ArgumentException("Invalid array");
                }
                string tmp = "";
                foreach (int? item in departures)
                {
                    if(item == null)
                        tmp += string.Format("{0},", 60);
                    else
                        tmp += string.Format("{0},", item.ToString());
                }
                DeparturesInString = tmp;
            }
        }

        /// <summary>
        /// Represent table's PositionOfStop head 
        /// </summary>
        [Table("PositionOfStop")]
        public class PositionOfStop
        {
            [PrimaryKey, Column("Stop")]
            public string Stop { get; set; }

            [Column("Lat")]
            public double Lat { get; set; }

            [Column("Lon")]
            public double Lon { get; set; }

            [Column("Stops")]
            public string Stops { get; set; }
            public PositionOfStop()
            { 
            
            }

            public PositionOfStop(string stop, double lat, double lon, string[] stops)
            {
                Stop = stop;
                Lat = lat;
                Lon = lon;

                if (stops.Length > 0)
                {
                    string tmp = stops[0] + ",";
                    for (int i = 1; i < stops.Length; i++)
                    {
                        if(stops[i] != null && stops[i] != string.Empty)
                            tmp += stops[i] + ",";
                    }
                    Stops = tmp;
                }
                else
                {
                    Stops = null;
                }
            }
        }

        /// <summary>
        /// Represent table's GraphTimetable head 
        /// </summary>
        [Table("GraphTimetable")]
        public class GraphTimetable
        {
            [Column("PlaceFrom")]
            public string PlaceFrom { get; set; }

            [Column("PlaceTo")]
            public string PlaceTo { get; set; }

            [Column("Range")]
            public int Range { get; set; }

            [Column("Time")]
            public int Time { get; set; }

            [Column("IdOfLine")]
            public string IdOfLine { get; set; }

            public GraphTimetable()
            { }

            public GraphTimetable(string placeFrom, string placeTo, int range, int time, string idOfLine)
            {
                PlaceFrom = placeFrom;
                PlaceTo = placeTo;
                Range = range;
                Time = time;
                IdOfLine = idOfLine;
            }            
        }

        /// <summary>
        /// Represent table's Autocorrection head 
        /// </summary>
        [Table("AutocorrectionStrings")]
        public class AutocorrectionStrings
        {
            [PrimaryKey,AutoIncrement, Column("ID")]
            public int ID { get; set; }
            [Column("AutocorrectionStrings")]
            public string AutocorrectionString { get; set; }
        }

        /// <summary>
        /// Represent table's UserData head 
        /// holds all the information needed for user data
        /// </summary>
        [Table("UserData")]
        public class UserData
        {
            [PrimaryKey,AutoIncrement, Column("ID")]
            public int ID { get; set; }

            [Column("UserName")]
            public string UserName { get; set; }

            [Column("Password")]
            public string UserPassword { get; set; }

            [Column("FullName")]
            public string FullName { get; set; }

            public UserData()
            { }

            public UserData(int ID, string UserName, string UserPassword, string FullName)
            {
                this.ID = ID;
                this.UserName = UserName;
                this.UserPassword = UserPassword;
                this.FullName = FullName;
            }

            public static User Create(int ID, string UserName, string UserPassword, string FullName)
            {
                return new User()
                {
                    ID = ID,
                    UserName = UserName,
                    UserPassword = UserPassword,
                    FullName = FullName
                };
            }
        }
}