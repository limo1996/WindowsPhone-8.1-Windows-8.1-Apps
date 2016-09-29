/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

//TODO:
//make find departure time more real

namespace Timetable
{
    /// <summary>
    /// Enumeration used for searching shortest way by distance or by time
    /// </summary>
        public enum CRIT : uint
        {
            DISTANCE,
            TIME
        }

    /// <summary>
    /// By time or distance
    /// </summary>
        struct BY {
            //BY() { }
            BY(int time, int distance)
            {
                this.time = time;
                this.distance = distance;
            
            }
            public int time, distance;
        }

    /// <summary>
    /// class that holds operations on the graph represented as matrix.
    /// </summary>
        public sealed class Graph
        {
            /// <summary>
            /// nested class holds Station name => unnecessary class
            /// </summary>
            class Station
            {
                public Station(string name)
                {
                    this.name = name;
                }
                ~Station()
                { }
                public string name;
            }

            /// <summary>
            /// Holds information about line => their stations
            /// In future development I can add string representing line info...
            /// </summary>
            class Line 
            {
                public Line() 
                {
                    stations = new List<string>();
                }
                public Line(string name)
                {
                    this.name = name;
                    stations = new List<string>();
                }

                //add new station to the line
                public void Push(string Station)
                {
                    if (!stations.Contains(Station))
                        stations.Add(Station);
                }

                //get array of strings(Stations)
                public string[] GetStations()
                {
                    return stations.ToArray();
                }

                public string name;
                private List<string> stations;
            }


            /// <summary>
            /// creates a new instance of graph that holds all operations needed in searching graph
            /// </summary>
            /// <param name="filename">database name with all  tables needed in loading graph</param>
            public Graph()
            {
                stringToStation = new SortedDictionary<string,Station>();
                stringToInt = new SortedDictionary<string, int>();
                intToString = new SortedDictionary<int, string>();
                stations = new HashSet<string>();
                stringToLine = new SortedDictionary<string, Line>();
                lines = new HashSet<string>();
            }
            
            ~Graph()
            {             
            }

            /// <summary>
            /// List of private class variables
            /// </summary>
            BY[,] matrix;
            SortedDictionary<string,Station> stringToStation;
            SortedDictionary<string, int> stringToInt;
            SortedDictionary<int, string> intToString;
            SortedDictionary<string, Line> stringToLine;
            HashSet<string> stations;
            HashSet<string> lines;
            Tuple<string, double, double>[] positionOfStation;

            int numOfStations;
            string closedStop;
            double distance;

            /// <summary>
            /// read only variable indicates how many vertexes(stations) graph has
            /// </summary>
            public int NumOfStations
            {
                get { return numOfStations; }
            }

            /// <summary>
            /// This function is needed if you want to use graph methods
            /// Loads private components of the graph
            /// </summary>
            /// <param name="tableName">table where are saved connections and ranges of the graph</param>
            public void LoadGraph()
            { 
                SQLiteLoader myLoader = new SQLiteLoader();
                var table = myLoader.GetTableGraph();
                List<string[]> tmp = new List<string[]>();

                foreach(var item in table)
                {
                    tmp.Add(new string[5]{
                        item.PlaceFrom,
                        item.PlaceTo,                        
                        item.Range.ToString(),
                        item.Time.ToString(),
                        item.IdOfLine,
                    });
                }

                string[][] tableGraph = tmp.ToArray();

                int counter = 0;

                foreach (string[] line in tableGraph)
                {
                    stations.Add(line[0]);
                    stations.Add(line[1]);

                    //
                    lines.Add(line[4]);

                    if (!stringToLine.ContainsKey(line[4]))
                        stringToLine[line[4]] = new Line(line[4]);
                    if (!stringToStation.ContainsKey(line[0]))    
                        stringToStation[line[0]] = new Station(line[0]);
                    if (!stringToStation.ContainsKey(line[1]))   
                        stringToStation[line[1]] = new Station(line[1]);

                    stringToLine[line[4]].Push(line[0]);
                    stringToLine[line[4]].Push(line[1]);
                }
                numOfStations = stations.Count;

                foreach (string station in stations)
                {
                    stringToInt[station] = counter;
                    intToString[counter] = station;
                    counter++;
                }

                CreateMatrix(tableGraph);
            }

            /// <summary>
            /// neeeded if you want to find the closest stop
            /// </summary>
            /// <param name="tableOfStops">table in database where are positions of stops located</param>
            public void LoadPositionOfStop()
            { 
                SQLiteLoader myLoader = new SQLiteLoader();

                PositionOfStop[] table = myLoader.GetTablePositionOfStop();

                if (table == null)
                {
                    throw new NullReferenceException("Function SqliteLoader.GetTable returns null reference.\nPlease check table or database name..");
                }
                positionOfStation = new Tuple<string, double, double>[table.GetLength(0)];

                int i = 0;
                foreach(var stop in table)               
                {
                    if (i < table.GetLength(0))
                    {
                        positionOfStation[i] = new Tuple<string, double, double>(stop.Stop,
                            stop.Lat, stop.Lon);
                        i++;
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("Index out of range in Graph.LoadPositionOfStop()");
                    }
                }
            }

            /// <summary>
            /// Checks wether exists direct way between two stations
            /// </summary>
            /// <param name="station1"></param>
            /// <param name="station2"></param>
            /// <returns></returns>
            public bool ExistDirectWay(string station1, string station2)
            {
                foreach (string lineString in lines)
                {
                    if (LayOnLine(lineString, station1) && LayOnLine(lineString, station2))
                        return true;
                }

                return false;
            }

            /// <summary>
            /// Finds shortest way between two stations. Stations must lay on the same line. Use test function
            /// </summary>
            /// <param name="start">start station</param>
            /// <param name="end">end station</param>
            /// <param name="criterium">enum criterium</param>
            /// <returns>tuple: Item1: stations. Item2: range</returns>
            public Tuple<string[],int> FindShortestDirectWay(string start, string end, CRIT criterium)
            {
                string[][] ways = this.GetLinesContainedStations(start, end);
                string[] shortestWay;
                int sum = 0, distance = 0;

                if (ways == null || ways.GetLength(0) == 0)
                    return null;

                if (ways.GetLength(0) == 1)
                    shortestWay = ways[0];
                else 
                {
                    int[] maxIndexes = this.CountMaximumIndexes(this.CountRange(ways, criterium));

                    if (maxIndexes.Length == 1)
                        shortestWay = ways[maxIndexes[0]];
                    else
                    { 
                        shortestWay = ways[this.FindLineWithMinimumOfStops(ways,maxIndexes)];
                    }
                
                }

                for(int i = 0; i < shortestWay.Length - 1; i++)
                {
                    this.GetDistance(shortestWay[i],shortestWay[i+1],criterium,out distance);
                    sum += distance;
                }
                return new Tuple<string[],int>(shortestWay,sum);
            }

            //finds line with minimum of stops
            private int FindLineWithMinimumOfStops(string[][] way, int[] maxIndexes)
            {
                int min= int.MaxValue, minIndex = -1, counter = 0;

                foreach (string[] line in way)
                {
                    if (min > line.Length)
                    {
                        minIndex = counter;
                        min = line.Length;
                    }
                    counter++;
                }
                return minIndex;
            }

            //helper function
            private int[] CountMaximumIndexes(int[] array)
            {
                int max = int.MinValue, counter = 0;
                List<int> maxIndexes = new List<int>();

                foreach (int item in array)
                {
                    if (item >= max)
                    {
                        if (item == max)
                            maxIndexes.Add(counter);
                        else
                        {
                            maxIndexes.Clear();
                            maxIndexes.Add(counter);
                        }
                        max = item;
                    }
                    counter++;
                }
                return maxIndexes.ToArray();
            }

            /// <summary>
            /// counts ranges in given lines by criterium
            /// </summary>
            /// <param name="way"></param>
            /// <param name="criterium"></param>
            /// <returns></returns>
            private int[] CountRange(string[][] way, CRIT criterium)
            {
                int[] ranges = new int[way.GetLength(0)];
                int distance = 0, counter = 0;

                foreach (string[] line in way)
                {
                    int sum = 0;
                    for(int i = 0; i < line.Length - 1; i++)
                    {
                        this.GetDistance(line[i], line[i + 1], criterium, out distance);
                        sum += distance;
                    }
                    ranges[counter] = sum;
                    counter++;
                }
                return ranges;
            }

            /// <summary>
            /// gets stations that contains station1 and station2
            /// </summary>
            /// <param name="station1"></param>
            /// <param name="station2"></param>
            /// <returns></returns>
            private string[][] GetLinesContainedStations(string station1, string station2)
            {
                List<string> possitiveLines = new List<string>();
                List<string[]> returned = new List<string[]>();
                int counter = 0;

                foreach (string lineString in lines)
                {
                    if (LayOnLine(lineString, station1) && LayOnLine(lineString, station2))
                        possitiveLines.Add(lineString);
                }

                foreach (string line in possitiveLines)
                {
                    counter = 0;
                    string[] stations = this.stringToLine[line].GetStations();
                    List<string> possitiveStations = new List<string>();

                    foreach(string station in stations)
                    {
                        if (station == station1 || station == station2)
                        {
                            counter++;
                            if(counter == 2)
                                possitiveStations.Add(station);
                        }
                        if (counter == 1)
                            possitiveStations.Add(station);
                    }
                    returned.Add(possitiveStations.ToArray());
                }
                return returned.ToArray();
            }

            /// <summary>
            /// checks wether station lays on line
            /// </summary>
            /// <param name="line"></param>
            /// <param name="station"></param>
            /// <returns></returns>
            private bool LayOnLine(string line, string station)
            {
                string[] stations = stringToLine[line].GetStations();
                foreach (string stop in stations)
                {
                    if (stop == station)
                        return true;
                }
                return false;
            }

            //create matrix that represents graph, stations(vertexes) and connections(edges)
            private BY[,] CreateMatrix(string[][] loaded)
            {
                matrix = new BY[stations.Count,stations.Count];

                for (int i = 0; i < numOfStations; i++)
                {
                    for (int j = 0; j < numOfStations; j++)
                    {
                        if (i == j)
                        {
                            matrix[i,j].distance = 0;
                            matrix[i, j].time = 0;
                        }
                        else
                        {
                            matrix[i, j].distance = int.MaxValue;
                            matrix[i, j].time = int.MaxValue;
                        }
                    }
                
                }

                    foreach (string[] line in loaded)
                    {
                        matrix[stringToInt[line[0]], stringToInt[line[1]]].distance = int.Parse(line[2]);
                        matrix[stringToInt[line[0]], stringToInt[line[1]]].time = int.Parse(line[3]);

                        matrix[stringToInt[line[1]], stringToInt[line[0]]].distance = int.Parse(line[2]);
                        matrix[stringToInt[line[1]], stringToInt[line[0]]].time = int.Parse(line[3]);
                    }
                return matrix;
            }

            /// <summary>
            /// Method finds closest station to the given coordinates
            /// </summary>
            /// <param name="lat">latitude</param>
            /// <param name="lon">longitude</param>
            /// <returns>name of the closest station</returns>
            public string FindClosestStop(double lat, double lon)
            {
                string closestStop = null;
                double minRange = int.MaxValue;
      
                foreach (Tuple<string,double,double> station in positionOfStation)
                {
                    if (Math.Sqrt((station.Item2 - lat) * (station.Item2 - lat) + (station.Item3 - lon) * (station.Item3 - lon)) < minRange)
                    {
                        minRange = Math.Sqrt((station.Item2 - lat) * (station.Item2 - lat) + (station.Item2 - lon) * (station.Item2 - lon));
                        closestStop = station.Item1;
                    }                
                }
                this.closedStop = closestStop;
                return closestStop;
            }
            
            /// <summary>
            /// finds earliest departure time from the given time of the given line
            /// </summary>
            /// <param name="startTime">time from which you want to find earliest departure time</param>
            /// <param name="station1">first station that lyes on the line</param>
            /// <param name="station2">second station that lyes on the line</param>
            /// <param name="line">line of departure time</param>
            /// <param name="order">is stations in order or not</param>
            /// <returns>Earliest departure time</returns>
            public DateTime FindDepartureTime(DateTime startTime, string station1, string station2, string line, bool order)
            {

                string[] firstAndLastStation = this.FindFirstAndLastStation(stringToLine[line].GetStations());
                PathBuilder builder = new PathBuilder(SQLiteLoader.DbName);

                if (station2 == firstAndLastStation[0] || station2 == firstAndLastStation[1])
                    return builder.GetTime(line,startTime);

               

                DateTime finalTime = startTime;
                DateTime tmpTime = startTime;
                DateTime decrementingTime = startTime;//.Subtract(new TimeSpan(0,10,0));

                while (this.FindDepartureTimePrivate(tmpTime, station1, station2, line, order) > startTime)
                {
                    finalTime = this.FindDepartureTimePrivate(tmpTime, station1, station2, line, order);
                    tmpTime = builder.GetTime(line, decrementingTime);
                    decrementingTime = decrementingTime.Subtract(new TimeSpan(0, 30, 0));
                }

                return finalTime;

                //return this.FindDepartureTimePrivate(startTime, station1, station2, line, order);
            }

            /// <summary>
            /// Method finds departure time from station2 on the line
            ///station1 is needed because of direction of the line... start station -> .. -> station1 -> ... -> station2 -> ...
            /// </summary>
            /// <param name="startTime">time when train or bus leaves initial station</param>
            /// <param name="station1">is first in order on the line</param>
            /// <param name="station2">station from which we want departure time</param>
            /// <param name="line">line of station1 and station2</param>
            /// <returns>DateTime when train or bus leaves station2</returns>
            private DateTime FindDepartureTimePrivate(DateTime startTime, string station1, string station2, string line, bool order)
            {
                string[] lineInOrder = this.GetLineInOrder(line);
                bool reverse = false;

                foreach (string station in lineInOrder)
                {
                    if (station == station1)
                    {
                        reverse = false;
                        break;
                    }
                    if (station == station2)
                    {
                        reverse = true;
                        break;
                    }
                }

                if (!reverse && !order)
                {
                    lineInOrder = lineInOrder.Reverse().ToArray();
                }

                for (int counter = 0; lineInOrder[counter] != station2 && counter < lineInOrder.Length - 1; counter++)
                {
                    int distance = 0;
                    this.GetDistance(lineInOrder[counter], lineInOrder[counter + 1], CRIT.TIME, out distance);
                    startTime = startTime.AddMinutes(distance);
                }
                return startTime;
            }

            /// <summary>
            /// Method finds departure time from "to" station on the line
            /// </summary>
            /// <param name="startTime">time when train or bus leaves "from" station</param>
            /// <param name="line">line of stations from and to</param>
            /// <param name="from">station of the line</param>
            /// <param name="to">station of the line</param>
            /// <returns>DateTime when train or bus arrives at "to" station</returns>
            public DateTime FindArrivalTime(DateTime startTime, string line, string from, string to)
            {
                string[] lineInOrder = this.GetLineInOrder(line);
                bool reverse = false;

                foreach (string station in lineInOrder)
                {
                    if (station == from)
                    {
                        reverse = false;
                        break;
                    }
                    if (station == to)
                    {
                        reverse = true;
                        break;
                    }
                }

                if (reverse)
                {
                    lineInOrder = lineInOrder.Reverse().ToArray();
                }

                int counter = 0;

                foreach (string station in lineInOrder)
                {
                    if (station == from)
                        break;
                    counter++;
                }

                for (; lineInOrder[counter] != to && counter < (lineInOrder.Length - 1); counter++)
                { 
                    int distance = 0;
                    this.GetDistance(lineInOrder[counter], lineInOrder[counter + 1], CRIT.TIME, out distance);
                    startTime = startTime.AddMinutes(distance);
                }
                    return startTime;
            }

            /// <summary>
            /// gets line in order
            /// </summary>
            /// <param name="line">line which have to be ordered</param>
            /// <returns>ordered line. e.g. starting from initial stop and ends with last stop</returns>
            private string[] GetLineInOrder(string line)
            {
                if (!lines.Contains(line))
                    return null;
                List<string> lineInOrder = new List<string>();
                string[] stations = stringToLine[line].GetStations();
                string[] tmp = this.FindFirstAndLastStation(stations);
                int distance = 0;
                string actualStop = tmp[0];

                lineInOrder.Add(tmp[0]);//start

                while (lineInOrder.Count != stations.Length)
                {
                    foreach (string station in stations)
                    { 
                    if(this.GetDistance(actualStop,station,CRIT.DISTANCE,out distance) && !lineInOrder.Contains(station))
                    {
                        lineInOrder.Add(station);
                        actualStop = station;
                    }
                    }
                }

                return lineInOrder.ToArray();
            }

            //finds first and last station in stations
            private string[] FindFirstAndLastStation(string[] stations)
            {
                int distance = 0;
                int counter = 0;
                List<string> returnedStations = new List<string>();

                foreach (string station1 in stations)
                {
                    foreach (string station2 in stations)
                    {
                        if (this.GetDistance(station1, station2, CRIT.DISTANCE, out distance))
                            counter++;
                    }
                    if (counter == 1)
                        returnedStations.Add(station1);
                    counter = 0;
                }
                if (returnedStations.Count != 2)
                {
                    // write into logger @"In function FindFirstAndLastStation has error occured...\n
                              //number of returned stations is not equal to 2"
                }
                return returnedStations.ToArray();
            }

            /// <summary>
            /// Determinates wether the connection exist or not a set distance
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <param name="FACTOR">time or distance accessibility</param>
            /// <param name="distance">if connection exists in distance is saved distance betwwn them</param>
            /// <returns>boolean wether exists direct connection between 2 stations</returns>
            public bool GetDistance(string from, string to, CRIT FACTOR, out int distance)
            {
                distance = -1;
                if (stringToStation.ContainsKey(from) && stringToStation.ContainsKey(to))
                {
                    if (matrix[stringToInt[from],stringToInt[to]].distance != int.MaxValue &&
                        matrix[stringToInt[from],stringToInt[to]].distance != 0)
                    {
                        if (FACTOR == CRIT.DISTANCE)
                        {
                            distance = matrix[stringToInt[from], stringToInt[to]].distance;
                            return true;
                        }
                        if (FACTOR == CRIT.TIME)
                        {
                            distance = matrix[stringToInt[from], stringToInt[to]].time;
                            return true;
                        }

                        return false;
                    }
                    else
                        return false;      
     
                }
                else
                    return false;
            
            }

            /// <summary>
            /// Determinates wether exists route between start and end station
            /// </summary>
            /// <param name="start">start station</param>
            /// <param name="end">end station</param>
            /// <returns>boolean variable represents wether route exists or not</returns>
            public bool ExistsWay(string start, string end)
            {
                Queue<int> myQueue = new Queue<int>();
                bool[] visited = new bool[this.numOfStations];
                for (int i = 0; i < this.numOfStations; i++)
                    visited[i] = false;

                myQueue.Enqueue(stringToInt[start]);
                visited[stringToInt[start]] = true;

                while (myQueue.Count != 0)
                {
                    for (int i = 0; i < this.numOfStations; i++)
                    {
                        if (this.matrix[myQueue.Peek(), i].distance != int.MaxValue && this.matrix[myQueue.Peek(), i].distance != 0
                            && visited[i] == false)
                        {
                            myQueue.Enqueue(i);
                            visited[i] = true;
                        }
                    }
                    myQueue.Dequeue();
                }
                return visited[stringToInt[end]];
            }

            /// <summary>
            /// Finds shortest way between two stations using Dijikstra's algorithm
            /// </summary>
            /// <param name="start">start station</param>
            /// <param name="end">end station</param>
            /// <param name="VALUE">criterium distance or time accessability</param>
            /// <returns>Tuple of string array(stations on the shortes route) and int(shortest range between start and end station)</returns>
            public Tuple<string[], int> FindShortestWay(string start, string end, CRIT VALUE)
            {


                bool[] closed = new bool[NumOfStations];
                int[] DISTANCES = new int[NumOfStations];
                List<string> predecessors = new List<string>();


                List<int> myList = new List<int>();

                myList.Add(stringToInt[start]);

                for (int i = 0; i < NumOfStations; i++)
                {
                    DISTANCES[i] = int.MaxValue;
                    closed[i] = false;
                }

                DISTANCES[stringToInt[start]] = 0;

                if (VALUE == CRIT.DISTANCE)
                {
                    while (myList.Count != 0)
                    {
                        double minDistance = int.MaxValue;
                        int node = -1;
                        for (int counter = 0; counter < myList.Count; counter++)
                            if (DISTANCES[myList[counter]] < minDistance)
                            {
                                minDistance = DISTANCES[myList[counter]];
                                node = myList[counter];
                            }

                        myList.Remove(node);
                        closed[node] = true;

                        for (int i = 0; i < NumOfStations; i++)
                            if (matrix[node, i].distance != int.MaxValue)
                                if (!closed[i])
                                    if (DISTANCES[node] + matrix[node, i].distance < DISTANCES[i])
                                    {
                                        DISTANCES[i] = DISTANCES[node] + matrix[node, i].distance;
                                        myList.Add(i);
                                    }
                    }

                    distance = DISTANCES[stringToInt[end]];
                    int tmp = stringToInt[end];
                    int j = 0;

                    predecessors.Add(end);
                    while (DISTANCES[tmp] != 0)
                    {
                        j = 0;
                        while (j < NumOfStations)
                        {
                            if (DISTANCES[tmp] - DISTANCES[j] == matrix[tmp, j].distance)
                            {
                                predecessors.Add(intToString[j]);
                                tmp = j;
                            }
                            j++;
                        }
                    }
                    predecessors.Reverse();

                    return new Tuple<string[], int>(predecessors.ToArray(), DISTANCES[stringToInt[end]]);
                }

                else
                {
                    while (myList.Count != 0)
                    {
                        double minDistance = int.MaxValue;
                        int node = -1;
                        for (int counter = 0; counter < myList.Count; counter++)
                            if (DISTANCES[(int)myList[counter]] < minDistance)
                            {
                                minDistance = DISTANCES[(int)myList[counter]];
                                node = (int)myList[counter];
                            }

                        myList.Remove(node);
                        closed[node] = true;

                        for (int i = 0; i < NumOfStations; i++)
                            if (matrix[node, i].time != int.MaxValue)
                                if (!closed[i])
                                    if (DISTANCES[node] + matrix[node, i].time < DISTANCES[i])
                                    {
                                        DISTANCES[i] = DISTANCES[node] + matrix[node, i].time;
                                        myList.Add(i);
                                    }
                    }
                    distance = DISTANCES[stringToInt[end]];
                    int tmp = stringToInt[end];
                    int j = 0;

                    predecessors.Add(end);
                    while (DISTANCES[tmp] != 0)
                    {
                        j = 0;
                        while (j < NumOfStations)
                        {
                            if (DISTANCES[tmp] - DISTANCES[j] == matrix[tmp, j].time)
                            {
                                predecessors.Add(intToString[j]);
                                tmp = j;
                            }
                            j++;
                        }
                    }
                    predecessors.Reverse();

                    return new Tuple<string[], int>(predecessors.ToArray(), DISTANCES[stringToInt[end]]);
                }
            }
        }
}