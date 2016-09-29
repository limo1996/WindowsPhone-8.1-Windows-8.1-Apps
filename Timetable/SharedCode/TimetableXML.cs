/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using System.IO;
using System.Net.Http;
using Windows.Networking.Connectivity;

namespace Timetable.Code
{
    /// <summary>
    /// used to save line in xml format and to load one ore more lines from internet via HttpClient
    /// </summary>
    public static class TimetableXML
    {
        //saves line container into xml
        public static async void SaveToXml(LineContainer container)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                    new XElement("Line",new XAttribute("IdOfLine",container.IdOfLine))
                    );
            doc.Root.Add(new XElement("PositionsOfStops"));
            foreach (var item in container.Positions)
            { 
                doc.Root.Element("PositionsOfStops").Add(new XElement("Stop", item.Key,
                    new XAttribute("lat",item.Value.Item1),new XAttribute("lon",item.Value.Item2)));    
            }

            string departures = null;
            foreach(var item  in container.Departures)
            {
                if(item.Value != null)
                    departures += item.Value + ",";
                else
                    departures += ",";
            }
            doc.Root.Add(new XElement("Departures",departures));

            for (int i = 0; i < container.Stations.Count; i++)
            {
                container.Stations[i] = PathBuilder.ChangeDiacritics(container.Stations[i]);
            }

            doc.Root.Add(new XElement("Stations"));
            foreach (var item in container.Stations)
            {
                doc.Root.Element("Stations").Add(new XElement("Station", item));            
            }

            doc.Root.Add(new XElement("Ranges"));

            foreach (var item in container.Ranges)
            {
                doc.Root.Element("Ranges").Add(new XElement("Range",
                    new XAttribute("from", item.Key.Item1), new XAttribute("to", item.Key.Item2),
                    new XAttribute("distance", item.Value.Item1), new XAttribute("time", item.Value.Item2)));
            }

            await SaveStringToLocalFile(container.IdOfLine + ".xml", doc.ToString());
        }

        /// <summary>
        /// save xml into local storage file
        /// </summary>
        /// <param name="filename">filename</param>
        /// <param name="content">content to be saved</param>
        /// <returns>async function returns void</returns>
        private static async Task SaveStringToLocalFile(string filename, string content)
        {
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        /// <summary>
        /// determines wether to load single line as .xml or more lines from .dat where are other links to xmls stored
        /// </summary>
        /// <param name="path">path in format http://example.com/example.xml </param>
        /// <returns></returns>
        public static async Task<List<LineContainer>> Load(string path)
        {
            if (path.EndsWith(".xml"))
            {
                List<LineContainer> returned = new List<LineContainer>();
                if (await LoadFromXML(path) != null)
                {
                    returned.Add(await LoadFromXML(path));
                    return returned;
                }
                else
                {
                    return null;
                }
            }
            else if (path.EndsWith(".dat"))
            {
                HttpClient client = new HttpClient();
                string content = await client.GetStringAsync(path);
                var lines = content.Split('\n');

                List<LineContainer> returned = new List<LineContainer>();

                foreach (string line in lines)
                {
                    LineContainer container = await LoadFromXML(line);
                    if (container != null)
                    {
                        returned.Add(container);
                    }
                }

                return returned;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// loads single line from given uri link using HttpClient and saves it ti LineContainer
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<LineContainer> LoadFromXML(string path)
        {
            Uri testUri = null;

            ConnectionProfile internetConnectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();

            if (internetConnectionProfile != null && internetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess
                && Uri.TryCreate(path,UriKind.RelativeOrAbsolute,out testUri))
            {

                HttpClient client = new HttpClient();
                string XML = await client.GetStringAsync(path);

                XDocument doc = XDocument.Parse(XML);

                LineContainer container = new LineContainer();
                container.IdOfLine = doc.Root.Attribute("IdOfLine").Value;

                Dictionary<string, Tuple<double, double>> positions = new Dictionary<string, Tuple<double, double>>();

                foreach (var item in doc.Root.Element("PositionsOfStops").Elements())
                {
                    positions[item.Value] = new Tuple<double, double>(int.Parse(item.Attribute("lat").Value),
                        int.Parse(item.Attribute("lon").Value));
                }

                Dictionary<int, int?> departures = new Dictionary<int, int?>();
                try
                {
                    var splited = doc.Root.Element("Departures").Value.Split(',');
                    for (int i = 0; i < 24; i++)
                    {
                        int result;
                        if (int.TryParse(splited[i], out result))
                        {
                            departures[i] = result;
                        }
                        else
                        {
                            departures[i] = null;
                        }
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    //log
                }

                List<string> stations = new List<string>();

                foreach (var item in doc.Root.Element("Stations").Elements())
                {
                    stations.Add(item.Value);
                }

                Dictionary<Tuple<string, string>, Tuple<int, int>> distances = new Dictionary<Tuple<string, string>, Tuple<int, int>>();

                foreach (var item in doc.Root.Element("Ranges").Elements())
                {
                    distances[Tuple.Create<string, string>(item.Attribute("from").Value, item.Attribute("to").Value)] =
                        Tuple.Create<int, int>(int.Parse(item.Attribute("distance").Value), int.Parse(item.Attribute("time").Value));
                }

                container.Departures = departures;
                container.Positions = positions;
                container.Ranges = distances;
                container.Stations = stations;

                return container;
            }
            else
                return null;
        }        
    }
}
