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

namespace Timetable
{
    /// <summary>
    /// class that saves LineContainer into Sqlite db located in app local storage
    /// </summary>
    public class LineSaver
    {
        SQLiteLoader sqliteLoader;

        public LineSaver()
        {
            sqliteLoader = new SQLiteLoader();
        }

        /// <summary>
        /// Saves line container into db
        /// </summary>
        /// <param name="container">Correct LineContainer</param>
        public void Save(LineContainer container)
        { 
            //change diacritics !!!
            foreach(var station in container.Stations)
            {
                sqliteLoader.SaveAutocorrectionString(station);
            }
            for (int i = 0; i < container.Stations.Count; i++)
            {
                container.Stations[i] = PathBuilder.ChangeDiacritics(container.Stations[i]);
            }

            List<int?> departuresInt = new List<int?>();
            for(int i = 0; i < container.Departures.Count;i++)
            {
                departuresInt.Add(container.Departures[i]);
            }
            
            Departures departure = new Departures(departuresInt.ToArray());
            List<GraphTimetable> graphTimetable = new List<GraphTimetable>();

            departure.IdOfLine = container.IdOfLine;
            foreach (var item in container.Ranges)
            { 
            graphTimetable.Add(new GraphTimetable(){
                IdOfLine = container.IdOfLine,
                PlaceFrom = item.Key.Item1,
                PlaceTo = item.Key.Item2,
                Range = item.Value.Item1,
                Time = item.Value.Item2
            });
            }

            foreach(var station in container.Positions)
            {
                if (sqliteLoader.GetPositionOfStop(station.Key) == null)
                {
                    sqliteLoader.SavePositionOfStop(new PositionOfStop(station.Key, station.Value.Item1,
                        station.Value.Item2, new string[1]{container.IdOfLine}));
                }
                else
                {
                    var stop = sqliteLoader.GetPositionOfStop(station.Key);
                    List<string> routes = new List<string>(stop.Stops.Split(','));
                    routes.Add(container.IdOfLine);
                    sqliteLoader.DeletePositionOfStop(station.Key);
                    sqliteLoader.SavePositionOfStop(new PositionOfStop(
                        station.Key,
                        stop.Lat,
                        stop.Lon,
                        routes.ToArray()));
                }
            }

            sqliteLoader.SaveDeparture(departure);

            foreach (var item in graphTimetable)
            {
                sqliteLoader.SaveGraphTimetable(item);
            }

        }


    }

   
}
