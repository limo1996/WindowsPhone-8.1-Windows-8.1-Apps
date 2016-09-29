using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using MyWindowsBlogReader.Code;

namespace MyWindowsBlogReader
{
    /// <summary>
    /// Class used to convert and load data into given format
    /// </summary>
    class FileConverter
    {
        private ExportFormat format;
        SQLiteSaver saver;

        public FileConverter()
        {
            saver = new SQLiteSaver(SQLiteSaver.DbName);
            this.format = saver.Settings.ExportFormat;
        }

        //Saves given strings into correct format set in Settings Page
        public async Task Save(string author, string title, string link, string guid, string descripion)
        {
            switch (format)
            { 
                case ExportFormat.HTML :
                    await this.SaveHTML(author, title, link, guid, descripion);
                    break;

                case ExportFormat.JSON :
                    await this.SaveJSON(author, title, link, guid, descripion);
                    break;

                case ExportFormat.PDF :
                    await this.SavePDF(author, title, link, guid, descripion);
                    break;

                case ExportFormat.TXT :
                    await this.SaveTXT(author, title, link, guid, descripion);
                    break;

                case ExportFormat.XML :
                    await this.SaveXML(author, title, link, guid, descripion);
                    break;
                    
                default: return;
            }
        }

        //saves strings into XML files
        private async Task SaveXML(string author, string title, string link, string guid, string descripion)
        {
            string path = saver.Settings.OutputFilename;
            if (!path.EndsWith(".xml"))
            {
                path += ".xml";
            }
                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                       new XElement("item",
                       new XElement("author", author),
                       new XElement("title", title),
                       new XElement("link",link),
                       new XElement("description", descripion),
                       new XElement("pubdate", DateTime.Now.ToString()),
                       new XElement("guid", guid)));

               
                string xml =  doc.ToString();
                await SaveStringToLocalFile(path, xml);
                await Copy(path,saver.Settings.OutputPath);
            
        }

        //saves strings into JSON file
        private async Task SaveJSON(string author, string title, string link, string guid, string descripion)
        {
            string path = saver.Settings.OutputFilename; 
            if (!path.EndsWith(".json"))
            {
                path += ".json";
            } 
            //creating class that will be stored as json
            Article article = new Article(DateTime.Now) { 
                                   Author = author,
                                   Title = title,
                                   Link = link,
                                   Guid = guid,
                                   Content = descripion
            };

            // Serialize our Product class into a string             
            string jsonContent = JsonConvert.SerializeObject(article);

            await SaveStringToLocalFile(path, jsonContent);

        }

        //saves given strings into txt file
        private async Task SaveTXT(string author, string title, string link, string guid, string descripion)
        {
            string path = saver.Settings.OutputFilename;//Path.Combine(saver.Settings.OutputPath, saver.Settings.OutputFilename);
            if (!path.EndsWith(".txt"))
            {
                path += ".txt";
            }

            string file = "\t\t\t\t" + title + "\n\n\n" + DateTime.Now.ToString() + "\t\t\t" + guid +
                "\n\n" + descripion + "\n\n" + link + "\n\n\t" + author;

            await SaveStringToLocalFile(path, file);
        }

  
        private async Task SaveStringToLocalFile(string filename, string content)
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

        public static async Task<string> ReadStringFromLocalFile(string filename)
        {
            // reads the contents of file 'filename' in the app's local storage folder and returns it as a string

            // access the local folder
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            // open the file 'filename' for reading
            Stream stream = await local.OpenStreamForReadAsync(filename);
            string text;

            // copy the file contents into the string 'text'
            using (StreamReader reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }

        //dont work
        private async Task SavePDF(string author, string title, string link, string guid, string descripion)
        {
            string path = saver.Settings.OutputFilename;
        }

        //saves given strings into HTML file
        private async Task SaveHTML(string author, string title, string link, string guid, string description)
        {
            string path = saver.Settings.OutputFilename;
            if (!path.EndsWith(".html"))
            {
                path += ".html";
            }

            string htmlFile = "<!DOCTYPE html><html><body>";
            htmlFile += "<h1>" + title + "</h1>";
            htmlFile += "<h5>" + DateTime.Now.ToString() + "</h5><i>" + guid + "</i>";
            htmlFile += "<p>" + description + "</p>";
            htmlFile += "<p><a href=\"" + link + "\">Find more information here</a></p>";
            htmlFile += "<p>" + author + "</p>";
            htmlFile += "</body></html>";

            await SaveStringToLocalFile(path, htmlFile);
        }

        //converts string loaded from file into Tuple of 5 strings according to given ExportFormat
        public async Task<Tuple<string, string, string, string, string, DateTime>> Convert(string path, ExportFormat exf)
        {
            switch (exf)
            { 
                case ExportFormat.HTML :
                    return await ConvertHTML(path);

                case ExportFormat.JSON :
                    return await ConvertJSON(path);

                case ExportFormat.NONE :
                    return null;

                case ExportFormat.PDF :
                    return await ConvertPDF(path);

                case ExportFormat.TXT :
                    return await ConvertTXT(path);

                case ExportFormat.XML :
                    return await ConvertXML(path);

                default: return null;
            }
        }

        //converts into string from XML file
        private async Task<Tuple<string, string, string, string, string, DateTime>> ConvertXML(string path)
        {
            XDocument doc = XDocument.Parse(await ReadStringFromLocalFile(path));
                XElement title = doc.Element(XName.Get("item"));
                List<string> nodes = new List<string>();

                foreach (var item in title.Elements())
                {
                    nodes.Add(item.Value);
                }
                return Tuple.Create<string, string, string, string, string, DateTime>(nodes[1], nodes[0], nodes[5],
                    nodes[2], nodes[3], DateTime.Parse(nodes[4]));
            
        }

        // converts into string from JSON file
        private async Task<Tuple<string, string, string, string, string, DateTime>> ConvertJSON(string path)
        { 
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                // Getting JSON from file if it exists, or file not found exception if it does not  
                StorageFile textFile = await localFolder.GetFileAsync(path);
                using (IRandomAccessStream textStream = await textFile.OpenReadAsync())
                {
                    // Read text stream     
                    using (DataReader textReader = new DataReader(textStream))
                    {
                        //get size                       
                        uint textLength = (uint)textStream.Size;
                        await textReader.LoadAsync(textLength);
                        // read it                    
                        string jsonContents = textReader.ReadString(textLength);
                        // deserialize back to our product!  
                        Article article = JsonConvert.DeserializeObject<Article>(jsonContents);

                        return Tuple.Create<string, string, string, string, string, DateTime>(article.Title,
                            article.Author, article.Guid, article.Link, article.Content, DateTime.Parse(article.PubDate));
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                return null;
            }
            
            
        }

        //converts into string from TXT file
        private async Task<Tuple<string, string, string, string, string, DateTime>> ConvertTXT(string path)
        {
            if(!path.EndsWith(".txt"))
            {
                path+=".txt";
            }

            string fileContent = await ReadStringFromLocalFile(path);

            fileContent = fileContent.Replace("\t\t\t\t", ",");
            fileContent = fileContent.Replace("\n\n\n", ",");
            fileContent = fileContent.Replace("\t\t\t", ",");
            fileContent = fileContent.Replace("\n\n\t", ",");
            fileContent = fileContent.Replace("\n\n", ",");

            var parsed = fileContent.Split(',');
            try
            {
                return Tuple.Create<string, string, string, string, string, DateTime>(parsed[1],
                    parsed[6], parsed[3], parsed[5], parsed[4], DateTime.Parse(parsed[2]));
            }
            catch (IndexOutOfRangeException ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                return null;
            }
        }

        //dont work
        private async Task<Tuple<string, string, string, string, string, DateTime>> ConvertPDF(string path)
        { 
            return null; 
        }

        //converts into strings from html
        private async Task<Tuple<string, string, string, string, string, DateTime>> ConvertHTML(string path)
        {
            string fileContent = await ReadStringFromLocalFile(path);

            string title = fileContent.Substring(fileContent.IndexOf("<h1>") + 4,
                fileContent.IndexOf("</h1>") - fileContent.IndexOf("<h1>") - 4);
            string pubDate = fileContent.Substring(fileContent.IndexOf("<h5>") + 4,
                fileContent.IndexOf("</h5>") - fileContent.IndexOf("<h5>") - 4);
            string guid = fileContent.Substring(fileContent.IndexOf("<i>") + 3,
                fileContent.IndexOf("</i>") - fileContent.IndexOf("<i>") - 4);
            string content = fileContent.Substring(fileContent.IndexOf("<p>") + 3,
                fileContent.IndexOf("</p>") - fileContent.IndexOf("<p>") - 3);
            string link = fileContent.Substring(fileContent.IndexOf("<p><a href=\"") + 12,
                fileContent.IndexOf("\">Find") - fileContent.IndexOf("<p><a href=\"") - 12);
            string author = fileContent.Substring(fileContent.IndexOf("</a></p><p>") + 11,
                fileContent.LastIndexOf("</p>") - fileContent.LastIndexOf("</p><p>") - 7);

            return Tuple.Create<string, string, string, string, string, DateTime>(title,
                author, guid, link, content, DateTime.Parse(pubDate));
        }

        private async Task Copy(string from, string to)
        {
            try
            {
                StorageFile newDB = await StorageFile.GetFileFromPathAsync(to);
                StorageFile originalDB = await StorageFile.GetFileFromPathAsync(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, from));
                await newDB.CopyAndReplaceAsync(originalDB);
            }
            catch (Exception ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
            }
        }
    }

    //class tat represents Article and its parts
    public class Article
    {
        public string Title { get; set; }
        public string Author { get;set;}
        public string Content{get;set;}
        public string Guid { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }

        public Article()
        { }

        public Article(DateTime time)
        {
            this.PubDate = time.ToString();
        }
    }
}