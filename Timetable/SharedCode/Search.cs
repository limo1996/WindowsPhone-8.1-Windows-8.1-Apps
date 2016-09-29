/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Timetable.Code
{
    /// <summary>
    /// Class that collects all path searching information with given parameters into ResultsContainer
    /// </summary>
    public static class Search
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FromTextBox">box where is start station written</param>
        /// <param name="ToTextBox">box where is end station written</param>
        /// <param name="TimeTextBox">box where is start time written(in string)</param>
        /// <param name="ShortestRange">combo box, where selected item makes sense</param>
        /// <param name="OnlyDirectConnections">check box</param>
        /// <param name="ResultsCount">combo box where selected item determines number of results</param>
        /// <returns>resluts container with number of results equal to selected item is ResultsCount ComboBox</returns>
        public static ResultsContainer Execute(TextBox FromTextBox, TextBox ToTextBox, TextBox TimeTextBox,
            ComboBox ShortestRange, CheckBox OnlyDirectConnections, ComboBox ResultsCount)
        {

            SQLiteLoader loader = new SQLiteLoader(SQLiteLoader.DbName);                    //used in communication with database
            var res = loader.GetAutocorrectionStrings();                                    //gets all stations in database
            if (res == null || res.GetLength(0) < 1)                                        //if there are some stations then OK
            {
                MessageDialog dialog = new MessageDialog("Database is empty !");            //if no throw error box
                var result = dialog.ShowAsync();
                return null;
            }
            else if ((FromTextBox.Background as SolidColorBrush).Color == Colors.Red ||     //if are input stations wrong
                (ToTextBox.Background as SolidColorBrush).Color == Colors.Red)
            {
                MessageDialog dialog = new MessageDialog("Incorrect input stations !");     //throw error
                var result = dialog.ShowAsync();
                return null;
            }
            else                                                                            //else
            { 
               
                string start = FromTextBox.Text;                                            //for better manipulation copy info from
                string end = ToTextBox.Text;                                                //text boxes into strings with shorter names

                PathBuilder pb = new PathBuilder(SQLiteLoader.DbName);                      //create path builder class

                if ((FromTextBox.Background as SolidColorBrush).Color == Colors.Green &&    //if are input stations correct
                    (ToTextBox.Background as SolidColorBrush).Color == Colors.Green &&
                    pb.CheckCorrectInputStations(start, end))                               
                {
                    if (pb.ExistWay(start, end))                                            //and if exist way between this 2 stations
                    {
                        int hour, minute;
                        var splited = TimeTextBox.Text.Split(':');                          //get time info from TextBox

                        try
                        {
                            if (!int.TryParse(TimeTextBox.Text.Split(':')[0], out hour))   //by safely parsing it
                            {
                                MessageDialog dialog = new MessageDialog("Time is not in correct format !");
                                var result = dialog.ShowAsync();                            //else throw error box
                                TimeTextBox.Background = new SolidColorBrush(Colors.Red);
                                return null;
                            }
                            if (!int.TryParse(TimeTextBox.Text.Split(':')[1], out minute))
                            {
                                MessageDialog dialog = new MessageDialog("Time is not in correct format !");
                                var result = dialog.ShowAsync();                            //else throw error box
                                TimeTextBox.Background = new SolidColorBrush(Colors.Red);
                                return null;
                            }
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            hour = DateTime.Now.Hour;
                            minute = DateTime.Now.Minute;
                            //TODO:Write into logger file
                        }

                        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            hour, minute, 0);                                               //get hours and minutes into DataTime
                        bool onlyDirectConnections = OnlyDirectConnections.IsChecked == true;
                        CRIT criterium = ShortestRange.SelectedItem                         //parse selected item into CRIT enum
                            as string == "Shortest distance" ? CRIT.DISTANCE : CRIT.TIME;

                        ResultsContainer resulutsContainer = new ResultsContainer()
                        {
                            Title = start + " > " + end                                     //create ResultsContainer and set its Title
                        };
                                                                                            //repeat following algorithm so many times
                                                                                            //how much user set
                        for (int i = 0; i < (int.Parse((ResultsCount.SelectedItem as ComboBoxItem).Content.ToString())); i++)
                        {
                                                                                            //build path
                            var item = pb.BuildPath(startDate, start, end, criterium, onlyDirectConnections);
                            if(item == null)
                            {
                                MessageDialog dialog = new MessageDialog("The way between " + start + " and " + end + " does not exists...");
                                var result = dialog.ShowAsync();
                                return null;
                            }
                            string title = "";
                                                                                            //set result title to Names of lines that
                                                                                            //are in the final way delimited by comma
                            foreach (var line in item.Item2)
                            {
                                if (!title.Contains(line))
                                    title += line + ", ";
                            }
                            title = title.Remove(title.LastIndexOf(", "));                  //delete last comma

                            string pathInfo = null;
                            for (int j = 0; j < item.Item2.Length; j += 2)
                            {
                                string infoInString = null;
                                                                                            //get info about path
                                var info = pb.GetPathInfo(item.Item2[j], item.Item3[j], item.Item1[j], item.Item1[j + 1]);

                                infoInString += item.Item2[j] + ":" + Environment.NewLine;
                                                                                            //get info into one string
                                for (int k = 0; k < info.Item1.Length; k++)
                                {
                                    infoInString += info.Item1[k] + "  " + info.Item2[k] + Environment.NewLine;
                                }
                                infoInString += Environment.NewLine;
                                pathInfo += infoInString;
                            }
                                                                                            //add result into result container
                            resulutsContainer.Results.Add(new Result()
                            {
                                DepartureAndArrivalTime = item.Item3,                       //set departures and arrival times created by built path method
                                OverallDistance = item.Item4,                               //set overall distance created by built path method
                                OverallTime = (int)item.Item5.TotalMinutes,                 //set overall time from TimeSpan created by built path method
                                Lines = item.Item2,                                         //set lines created by built path method
                                Stations = item.Item1,                                      //set stations created by built path method
                                Title = title,                                              //set title created in this method
                                DetailsOfLine = pathInfo                                    //set path info created in this method
                            }
                                );

                            startDate = item.Item3[0].AddMinutes(1);

                        }
                        return resulutsContainer;
                    }
                    else
                    {
                        MessageDialog dialog = new MessageDialog("The way between " + start + " and " + end + " does not exists...");
                        var result = dialog.ShowAsync();
                        return null;
                    }
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("The database does not contain input stations !");
                    var result = dialog.ShowAsync();
                    return null; 
                }
            }
        }
    }
}
