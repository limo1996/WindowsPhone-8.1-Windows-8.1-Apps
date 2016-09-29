using MyWindowsBlogReader.Code;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Web.Syndication;

namespace MyWindowsBlogReader
{

    //holds into for a single blog feed, including a list of blog posts (FeedItem)
    public class FeedData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PubDate { get; set; }

        private List<FeedItem> _Items = new List<FeedItem>();
        public List<FeedItem> Items
        {
            get 
            {
                return _Items;
            }
        }

        private List<FeedItemStruct> feedItemStruct = new List<FeedItemStruct>();

        public List<FeedItemStruct> FeedItemStruct {
            get {
                return feedItemStruct;
            }
        }

        public FeedData()
        {
        }

        public FeedData(FeedDataStruct feedDataStruct)
        {
            this.Description = feedDataStruct.Description;
            DateTime tmp;

            if (DateTime.TryParse(feedDataStruct.PubDate, out tmp))
            {
                this.PubDate = tmp;
            }
            else
            {
                this.PubDate = DateTime.Now;
            }

            this.Title = feedDataStruct.Title;
        }

        public FeedData(FeedDataStruct feedDataStruct, List<FeedItem> listOfFeedItems)
            :this(feedDataStruct)
        {
            this._Items = new List<FeedItem>(listOfFeedItems);
        }

    }

    //Holds info for a single blog post
    public class FeedItem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime PubDate { get; set; }
        public Uri Link { get; set; }

        public FeedItem()
        { 
        }

        public FeedItem(FeedItemStruct feedItemStruct)
        {
            this.Title = feedItemStruct.Title;
            this.Author = feedItemStruct.Author;
            this.Content = feedItemStruct.Content;
            DateTime tmp;

            if(DateTime.TryParse(feedItemStruct.PubDate, out tmp))
            {
                this.PubDate = tmp;
            }
            else
            {
                this.PubDate = DateTime.Now;
            }
            this.Link = new Uri(feedItemStruct.Link);
        }
    }

    //Holds a collection of blog feeds (FeedData), and contains methods needed to retrieve the feeds.
    public class FeedDataSource
    {
        private ObservableCollection<FeedData> _Feeds = new ObservableCollection<FeedData>();
        public ObservableCollection<FeedData> Feeds
        {
            get
            {
                return _Feeds;
            }
        }

        //load feeds asynchronously 
        //links loads from database
        public async Task GetFeedAsync()
        {
            
            List<Task<FeedData>> feedList = new List<Task<FeedData>>();
            SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
            string[] links = saver.GetLinks();

            foreach (var link in links)
            {
                feedList.Add(GetFeedAsync(link));
            }
            foreach (var feed in feedList)
            {
                this._Feeds.Add(await feed);
            }
        }

        private async Task<FeedData> GetFeedAsync(string feedUriString)
        {
            SyndicationClient client = new SyndicationClient();
            Uri feedUri = new Uri(feedUriString);
            
            try
            {
                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);
                FeedData feedData = new FeedData();

                if (feed.Title != null && feed.Title.Text != null)
                {
                    feedData.Title = feed.Title.Text;
                }
                if (feed.Subtitle != null && feed.Subtitle.Text != null)
                {
                    feedData.Description = feed.Subtitle.Text;
                }

                if (feed.Items != null && feed.Items.Count > 0)
                { 
                    // Use the date of the latest post as the last pdated date.
                    feedData.PubDate = feed.Items[0].PublishedDate.DateTime;

                    foreach (var item in feed.Items)
                    {
                        FeedItem feedItem = new FeedItem();
                        if (item.Title != null && item.Title.Text != null)
                        {
                            feedItem.Title = item.Title.Text;
                        }
                        if (item.PublishedDate != null)
                        {
                            feedItem.PubDate = item.PublishedDate.DateTime;
                        }
                        if (item.Authors != null && item.Authors.Count > 0)
                        {
                            feedItem.Author = item.Authors[0].Name;
                        }

                        if (feed.SourceFormat == SyndicationFormat.Atom10)
                        {
                            if (item.Content != null && item.Content.Text != null)
                            {
                                feedItem.Content = item.Content.Text;
                            }
                            if (item.Id != null)
                            {
                                feedItem.Link = item.BaseUri;
                            }
                        }
                        else if (feed.SourceFormat == SyndicationFormat.Rss20)
                        {
                            if (item.Summary != null && item.Summary.Text != null)
                            {
                                feedItem.Content = item.Summary.Text;
                            }
                            if (item.Links != null && item.Links.Count > 0)
                            {
                                feedItem.Link = item.Links[0].Uri;
                            }
                        }
                        feedData.Items.Add(feedItem);
                    }
                }
                return feedData;
            }
            catch (Exception ex)
            {
                FileLogger.WriteFile(FileLogger.Filename, ex.Message + "\nSource: " + ex.Source);
                MessageDialog dialog = new MessageDialog(ex.Message);
                var result = dialog.ShowAsync();
                return null;
            }
        }

        //return the feed that has the specified title.
        public static FeedData GetFeed(string title)
        {
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;

            if (_feedDataSource == null)
                return null;

            var matches = _feedDataSource.Feeds.Where((feed) => feed.Title.Equals(title));
            if (matches.Count() == 1)
            {
                return matches.First();
            }
            return null;
        }

        public static FeedItem GetItem(string uniqueID)
        {
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            /*var matches = from item in _feedDataSource.Feeds where item.Title == uniqueID select item;
                _feedDataSource.Feeds.SelectMany(group => group.Items).
            Where((item) => item.Title.Equals(uniqueID));*/

            List<FeedItem> result = new List<FeedItem>();

            foreach (FeedData data in _feedDataSource.Feeds)
            {
                var matches = from item in data.Items where item.Title == uniqueID select item;

                if(matches != null)
                {
                    foreach (FeedItem feed in matches)
                        result.Add(feed);
                }
            }

            if (result.Count > 0)
                return result[0];

            return null;
        }
    } 
}
