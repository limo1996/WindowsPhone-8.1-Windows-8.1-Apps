using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using Windows.Storage;
using MyWindowsBlogReader.Code;


// TODO: data logger file => visible only for app
//comment heavily

namespace MyWindowsBlogReader
{
    public partial class SQLiteSaver
    {
        public static string DbName = "MyWindowsBlogReaderDatabase.sqlite";

        /// <summary>
        /// Save feeds into SQLite database
        /// </summary>
        /// <param name="feedData">Object that contains all feeds used by application</param>
        /// <returns>boolean whether was saving successful</returns>
        public bool SaveFeeds(FeedDataSource feedData)
        {
            List<FeedDataStruct> listFeedDataSource = new List<FeedDataStruct>();
            //conversion into objects that can be stored in database
            foreach (var feed in feedData.Feeds)
            {
                FeedDataStruct feedDataSource = new FeedDataStruct(feed);
                listFeedDataSource.Add(feedDataSource);
            }
            using (var database = new SQLiteConnection(this.databaseName))
            {
                try
                {
                    //creates tables (if are already created do nothing) and delete all datas in them
                        database.CreateTable<FeedItemStruct>();
                        database.CreateTable<FeedDataStruct>();

                        database.DeleteAll<FeedItemStruct>();
                        database.DeleteAll<FeedDataStruct>();
                    
                    //saving objects
                    foreach (var item in feedData.Feeds)
                    {
                        database.Insert(new FeedDataStruct(item));
                        foreach (var item2 in item.FeedItemStruct)
                        {
                            database.Insert(item2);
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Method loads datas from SQLite database into FeedDataSource object
        /// </summary>
        /// <returns>whole datas loaded into FeedDataSource object</returns>
        public FeedDataSource LoadFeeds()
        {
            FeedDataSource feedDataSource = new FeedDataSource();

            //creating database for loading datas
            using (var database = new SQLiteConnection(this.databaseName))
            {
                //in case that tables are not created
                database.CreateTable<FeedDataStruct>();
                database.CreateTable<FeedItemStruct>();

                //creating new instance of generics list of FeedDataStruct objects that will be filled 
                //with datas from sqlite table
                List<FeedDataStruct> feedDataStruct = new List<FeedDataStruct>();

                var feedDatas = database.Table<FeedDataStruct>();
                foreach (var feedDataLocal in feedDatas)
                {
                    feedDataStruct.Add(feedDataLocal);
                }

                //in this foreach loop will be filled FeedData struct
                foreach (var item in feedDataStruct)
                {
                    //List of FeedItems fill be filled with datas from database but from FeedItemStruct table
                    //FeedItems will be picked through their IDs taht are saved in FeedData object
                    List<FeedItem> listOfFeedItems = new List<FeedItem>();
                    foreach (int i in this.MakeIntsFromString(item.IDs))
                    {
                        var selected = database.Table<FeedItemStruct>().Where(o => o.ID == i);//.First();
                        if(selected.Count() > 0)
                            listOfFeedItems.Add(new FeedItem(selected.First()));
                    }
                    //FeedDataSruct is transformed through constructor into List of 
                    //FeedData objects
                    FeedData feedData = new FeedData(item,listOfFeedItems);
                    feedDataSource.Feeds.Add(feedData);
                }
            }
            return feedDataSource;
        }

        //function used to save array of ints into SQLite table
        //this function make from array {1,2,3,6,4,6} string "1,2,3,6,4,6"
        private int[] MakeIntsFromString(string array)
        {
            if (array == null || array.Length == 0)
                return new int[] { };

            string[] tokens = array.Split(',');

            List<int> ints = new List<int>();
            foreach (string token in tokens)
                ints.Add(int.Parse(token));

            return ints.ToArray();
        }

        /// <summary>
        /// Deletes all information in all tables in current database
        /// </summary>
        public void ClearAllTables()
        {
            try
            {
                using (var database = new SQLiteConnection(this.DatabaseName))
                {
                    database.DeleteAll<FeedItemStruct>();
                    database.DeleteAll<FeedDataStruct>();
                    database.DeleteAll<LinkTable>();
                    database.DeleteAll<Settings>();
                }
            }
            catch (SQLiteException ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
            
            }
        }
    }

    /// <summary>
    /// Holds two static variables used to set IDs in FeedItemStruct and FeedDataStruct class
    /// </summary>
    public static class Keys
    {
        public static int FeedItemId = 0;
        public static int FeedDataId = 0;
    }

    /// <summary>
    /// very similar to FeedItem but this class holds structure of data that will be saved into SQLite database
    /// </summary>
    [Table("FeedItem")]
    public class FeedItemStruct
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }

        /// <summary>
        /// constructor that creates new instance of FeedItemStruct from feedItem and their ID
        /// </summary>
        /// <param name="feedItem">FeedItem to be transform</param>
        /// <param name="ID">ID of the FeedItemStruct object</param>
        public FeedItemStruct(FeedItem feedItem, int ID)
        {
            this.ID = ID;
            this.Author = feedItem.Author;
            this.Content = feedItem.Content;
            this.Link = feedItem.Link.AbsoluteUri;
            this.Title = feedItem.Title;
            this.PubDate = feedItem.PubDate.ToString();
        }

        //default constructor needed when creating table
        public FeedItemStruct()
        {

        }
    }

    /// <summary>
    /// very similar to FeedData but this class holds structure of data that will be saved into SQLite database
    /// </summary>
    [Table("FeedData")]
    public class FeedDataStruct
    {
        [PrimaryKey]
        public int ID { get; set; }
        
        public string Title { get; set; }
        public string PubDate { get; set; }
        public string Description { get; set; }
        public string IDs { get; set; }

        /// <summary>
        /// construtor used to transform FeedData into FeedDataStruct
        /// </summary>
        /// <param name="feedData">instance of FeedData to be transform</param>
        public FeedDataStruct(FeedData feedData)
        {
            this.Title = feedData.Title;
            this.Description = feedData.Description;
            this.PubDate = feedData.PubDate.ToString();
            this.ID = Keys.FeedDataId;
            Keys.FeedDataId++;

            List<int> IDs = new List<int>();
            int count = feedData.Items.Count;
            int topBorder = count + Keys.FeedItemId;

            for (int i = Keys.FeedItemId; i < topBorder; i++)
            {
                IDs.Add(i);
                Keys.FeedItemId++;
            }

            try
            {
                for (int i = 0; i < count; i++)
                {
                    FeedItemStruct feedItemStruct = new FeedItemStruct(feedData.Items[i], IDs[i]);
                    feedData.FeedItemStruct.Add(feedItemStruct);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
            }

            this.IDs = this.MakeStringFromArray(IDs.ToArray());
        }

        //default constructor needed when creating table
        public FeedDataStruct()
        { 
        
        }

        //back conversion from string to array of ints
        private string MakeStringFromArray(int[] array)
        {
            if (array.Length < 1)
                return null;
            string Arraystring = array[0].ToString();

            for (int i = 1; i < array.Length; i++)
            {
                Arraystring += "," + array[i].ToString();
            }
            return Arraystring;
        }
    }
}
