/**************************************************************************
*Jakub Lichman, FEI STU BA, project start: 15.08.2015, end: 10.10.2015    *
*                All Rights Reserved. Copying allowed.                    *
***************************************************************************/

using MyWindowsBlogReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace MyWindowsBlogReader
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage_MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ItemsPage_MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
            FeedDataSource feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            FeedData feedData = new FeedData();
            feedData.Title = "Add";
            var insert = from i in feedDataSource.Feeds where i.Title == "Add" select i;

            if(insert == null || insert.Count() == 0)
                feedDataSource.Feeds.Insert(0, feedData);

            if (feedDataSource != null)
                this.defaultViewModel["Items"] = feedDataSource.Feeds;
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the splitPage and send title as argument
            if(e.ClickedItem != null)
            {
                string title = (e.ClickedItem as FeedData).Title;
                if (title == "Add")
                    this.Frame.Navigate(typeof(AddItemsPage_Level1), title);
                else
                this.Frame.Navigate(typeof(SplitPage_Level1), title);
            }
            
        }

        /***** Navigation funkcions raised by event Button Click from App bar buttons *****/
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage_Level1));
        }

        private void AddBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddItemsPage_Level1));
        }

        private void InfoBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(InfoPage_Level1));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BlogWriterPage_Level1));
        }
    }
}
