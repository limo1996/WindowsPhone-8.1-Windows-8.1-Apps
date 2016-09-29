/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Timetable
{
    /// <summary>
    /// holds all datas in List of Results and Title of searching
    /// </summary>
    public class ResultsContainer
    { 
        public string Title{ get; set; }
        public List<Result> Results 
        {
            get
            {
                return _results;
            }
        }
        private List<Result> _results = new List<Result>();

    }

    /// <summary>
    /// Holds all the data for a single searching way. This result is binded in results pages
    /// </summary>
    public class Result
    {
        public string Title { get; set; }
        public string DetailsOfLine { get; set; }
        private int overallTime;
        public int OverallTime
        {
            get
            {
                return overallTime;
            }
            set
            {
                overallTime = value;
                SmallInfo += "Overall time " + value + "minutes. " + Environment.NewLine;
            }
        }
        private int overallDistance;
        public int OverallDistance 
        { 
            get
            {
                return overallDistance;
            }
            set 
            {
                overallDistance = value;
                SmallInfo += "Overall distance " + value + "kilometers. " + Environment.NewLine;
            }
        }
        public string[] Stations { get; set; }
        public string[] Lines { get; set; }
        private DateTime[] departureAndArrivalTime;
        public DateTime[] DepartureAndArrivalTime
        {
            get
            {
                return departureAndArrivalTime;
            }
            set
            {
                departureAndArrivalTime = value;
                StartTime = value[0].ToString();
            }
        }
        public string StartTime { get; set; }
        public string SmallInfo { get; set; }
    }
}
