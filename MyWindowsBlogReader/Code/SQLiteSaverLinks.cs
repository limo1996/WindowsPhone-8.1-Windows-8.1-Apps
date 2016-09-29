using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using System.Xml.Serialization;
using Windows.Storage;
using System.Collections.ObjectModel;
using MyWindowsBlogReader.Code;



namespace MyWindowsBlogReader
{
    /// <summary>
    /// Class that do all the job needed when inserting, deleting and selecting data from database
    /// </summary>
    public partial class SQLiteSaver
    {
        private string databaseName;
        
        private Settings settings = null;

        public Settings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                this.settings = value;
            }
        }

        //property that returns and sets database name
        public string DatabaseName
        {
            get
            {
                return databaseName;
            }
            set
            {
                //combine database name with location of the applicaton on the disc
                databaseName = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, value);
            }
        }

        //save given array of links into database
        public void SaveLinks(string[] links)
        {
            if (!CheckLinks(links))
            {
                throw new ArgumentException("One or more input strings are invalid!");
            }
            else
            {
                foreach (string link in links)
                {
                    this.SaveLink(link);
                }
            
            }
        }

        //save link into databes
        private void SaveLink(string link)
        {
            try
            {
                using (var database = new SQLiteConnection(this.databaseName))
                {
                    database.CreateTable<LinkTable>(CreateFlags.None);

                    if (database.Find<LinkTable>(link) == null || database.Find<LinkTable>(link).Link == null)
                    {
                        database.Insert(new LinkTable(link, 0));
                    }
                }
            }
            catch (SQLiteException ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
            }
        }

        //return links taht are saved in database
        public string[] GetLinks()
        {
            try
            {
                using (var database = new SQLiteConnection(this.databaseName))
                {
                    database.CreateTable<LinkTable>();
                    var data = database.Table<LinkTable>().ToArray();

                    var returned = from i in data select i.Link;
                    return returned.ToArray();
                }
            }
            catch (SQLiteException ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                return null;
            }
        }

        //chech if all links are correct
        private static bool CheckLinks(string[] links)
        {
            Uri uri = null;
            foreach (string link in links)
            {
                if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
                {
                    return false;
                }
                uri = null;
            }
            return true;
        }

        // sets database name and load settings from database into public Getter and Setter
        public SQLiteSaver(string databaseName)
        {
            
            this.DatabaseName = databaseName;
            this.settings = this.LoadSettings();
           
        }

        //sets database name
        public SQLiteSaver()
        {
            this.settings = this.LoadSettings();
        }

        //save settings that are a class variable
        public void SaveSettings()
        {
            using (var database = new SQLiteConnection(this.databaseName))
            {
                try
                {
                    database.CreateTable<Settings>();
                    database.DeleteAll<Settings>();
                    database.Insert(this.settings);
                }
                catch (SQLiteException ex)
                {
                    FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                }
            }
        }

        //overloaded method that saves settings into database passed as parameter
        public void SaveSettings(Settings set)
        {
            using (var database = new SQLiteConnection(this.databaseName))
            {
                try
                {
                    database.CreateTable<Settings>();
                    database.DeleteAll<Settings>();
                    database.Insert(set);
                }
                catch (SQLiteException ex)
                {
                    FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                }
            }
        }

        //load settings from database
        private Settings LoadSettings()
        {
            using (var database = new SQLiteConnection(this.databaseName))
            {
                try
                {
                    database.CreateTable<Settings>();
                    var returned = database.Table<Settings>();
                    if (returned != null && returned.Count() > 0)
                    {
                        return returned.First();
                    }
                    else
                    {
                        return new Settings(SQLiteSaver.DbName, false);
                    }
                }
                catch (SQLiteException ex)
                {
                    FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                    return new Settings(SQLiteSaver.DbName, false);
                }
                catch (Exception ex)
                {
                    FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                    return new Settings(SQLiteSaver.DbName, false);
                }

            }
        }
        
    }

    /// <summary>
    /// class that holds info about application settings
    /// </summary>
    public class Settings
    {
        public string DatabaseName { get; set; }
        public bool SavingBeforeExiting { get; set; }
        public ExportFormat ExportFormat { get; set; }
        public string OutputPath { get; set; }
        public string OutputFilename { get; set; }
        public string Filename { get; set; }
        public Settings()
        {
            
        }

        //contructor with parameters
        public Settings(string dbName, bool save)
        {
            this.DatabaseName = dbName;
            this.SavingBeforeExiting = save;
        }

        //returns enum representation of the string value
        public static ExportFormat FromStringEnum(string enumInString)
        {
            switch (enumInString)
            {
                case "XML": return ExportFormat.XML;
                    //break;
                case "JSON": return ExportFormat.JSON;
                    //break;
                case "PDF": return ExportFormat.PDF;
                    //break;
                case "HTML": return ExportFormat.HTML;
                    //break;
                case "TXT": return ExportFormat.TXT;
                    //break;
                default: return ExportFormat.NONE;
                    
            }
        }

        //returns string representation of the ExportFormat enum
        public static string FromEnumString(ExportFormat exportFormat)
        {

            switch (exportFormat)
            {
                case ExportFormat.HTML: return "HTML";
                    //
                case ExportFormat.JSON: return "JSON";
                    //
                case ExportFormat.PDF: return "PDF";
                    //
                case ExportFormat.TXT: return "TXT";
                    //
                case ExportFormat.XML: return "XML";
                    //
                default: return "NONE";            
            }
        }
    }

    /// <summary>
    /// class that contains link that is saved in database
    /// </summary>
    public class LinkTable
    {
        [PrimaryKey]
        public string Link { get; set; }
        [Ignore]
        public int Counter { get; set; }

        public LinkTable(string link, int ID)
        {
            this.Link = link;
            this.Counter = ID;
        }

        public LinkTable()
        {
            this.Link = null;
        }
    }

    /// <summary>
    /// Enum that represent output format of the user`s blog
    /// </summary>
    public enum ExportFormat : int
    { 
        XML = 0,
        JSON = 1,
        TXT = 2,
        HTML = 3,
        PDF = 4,
        NONE = 5,
    }
}
