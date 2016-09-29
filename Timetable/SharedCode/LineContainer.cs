/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;

namespace Timetable
{
    /// <summary>
    /// class that holds all information given by user. This information is than saved into sqlite db or xml
    /// </summary>
    public class LineContainer
    {
        public List<string> Stations
        {
            get
            { return stations; }
            set
            { stations = value; }
        }
        /// <summary>
        /// string - Name of station, Tuple => Item1 - Lat, => Item2 - Lon
        /// </summary>
        public Dictionary<string, Tuple<double, double>> Positions
        {
            get
            { return positions; }
            set
            { positions = value; }
        }

        /// <summary>
        /// Tuple1 : From(string), To(string); Tuple2: Distance(int), Time(int)
        /// </summary>
        public Dictionary<Tuple<string, string>, Tuple<int, int>> Ranges
        {
            get
            { return ranges; }
            set
            { ranges = value; }
        }

        //key : hour, value : minute
        public Dictionary<int, int?> Departures
        {
            get
            { return departures; }
            set
            { departures = value; }
        }

        public string IdOfLine { get; set; }

        private List<string> stations = new List<string>();
        private Dictionary<string, Tuple<double, double>> positions = new Dictionary<string, Tuple<double, double>>();
        private Dictionary<Tuple<string, string>, Tuple<int, int>> ranges = new Dictionary<Tuple<string, string>, Tuple<int, int>>();
        private Dictionary<int, int?> departures = new Dictionary<int, int?>();

        /// <summary>
        /// checks wether line is in correct format, if all values are filled, if is ID unique and so on
        /// </summary>
        /// <returns></returns>
        public bool IsCorrect()
        {
            foreach (var station in Stations)
            {
                if (!Positions.ContainsKey(station))
                {
                    MessageDialog dialog = new MessageDialog("Stations can not be deleted before adding line");
                    var result = dialog.ShowAsync();
                    return false;
                }

                if (station == "")
                {
                    MessageDialog dialog = new MessageDialog("Station cannot be empty string !");
                    var result = dialog.ShowAsync();
                    return false;
                }
            }
            for (int i = 0; i < 24; i++)
            {
                if (!Departures.ContainsKey(i))
                {
                    MessageDialog dialog = new MessageDialog("Departure at " + i +"aclock is not filled");
                    var result = dialog.ShowAsync();
                    return false;
                }
            }
            if (IdOfLine == "" || IdOfLine == null || IdOfLine.Length < 4)
            {
                MessageDialog dialog = new MessageDialog("ID format is incorrect !");
                var result = dialog.ShowAsync();
                return false;
            }

            if (Stations.Count <= 1)
                return false;
            SQLiteLoader loader = new SQLiteLoader(SQLiteLoader.DbName);
            foreach (var id in loader.GetIdsOfLines())
            {
                if (id == IdOfLine)
                {
                    MessageDialog dialog = new MessageDialog("ID already exists !");
                    var result = dialog.ShowAsync();
                    return false;
                }
            }
            return true;
        }
    }
}
