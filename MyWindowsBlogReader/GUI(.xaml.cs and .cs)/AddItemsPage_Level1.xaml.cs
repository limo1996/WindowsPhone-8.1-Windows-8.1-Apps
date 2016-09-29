/**************************************************************************
*Jakub Lichman, FEI STU BA, project start: 15.08.2015, end: 10.10.2015    *
*                All Rights Reserved. Copying allowed.                    *
***************************************************************************/

using MyWindowsBlogReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Syndication;
using Windows.System;


namespace MyWindowsBlogReader
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class AddItemsPage_Level1 : Page
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


        public AddItemsPage_Level1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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

        //events raised when pointer clicks, enter, left text box
        private void AddButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.AddTextBox.Foreground = new SolidColorBrush(Colors.LightGray);            
        }

        private void AddButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.AddTextBox.Foreground = new SolidColorBrush(Colors.Black);
            this.AddTextBox.Text = "";
        }

        //checks link correctness ongoing
        private void AddButton_TextChanged(object sender, TextChangedEventArgs e)
        {
            counter++;
            Uri uri = null;
            if (!Uri.TryCreate(this.AddTextBox.Text, UriKind.Absolute, out uri))
            {
                this.AddTextBox.Background = new SolidColorBrush(Colors.Red);
                this.HelpButton.IsEnabled = true;
                this.HelpButton.Icon = new SymbolIcon(Symbol.Help);
                return;
            }

          
            SyndicationClient syndicationClient = new SyndicationClient();
            syndicationClient.Timeout = 0;
            var syndicationFeed = syndicationClient.RetrieveFeedAsync(uri);
            while (syndicationFeed.Status == AsyncStatus.Started) { }

            if (syndicationFeed == null || syndicationFeed.Status == AsyncStatus.Error)
            {
                this.AddTextBox.Background = new SolidColorBrush(Colors.Red);
                this.HelpButton.IsEnabled = true;
                this.HelpButton.Icon = new SymbolIcon(Symbol.Help);
                return;
            }
            else 
            {
                this.AddTextBox.Background = new SolidColorBrush(Colors.Green);
                this.HelpButton.IsEnabled = false;
                this.HelpButton.Icon = null;
            }

        }
        private int counter = 0;
        void Response_Completed(IAsyncResult result)
        {

        }

        private void AddTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Text = "";
            textBox.GotFocus -= AddTextBox_GotFocus;
        }

        //shows why error has occured
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if ((this.AddTextBox.Background as SolidColorBrush).Color == Colors.Yellow)
            {
                MessageDialog errorDialog = new MessageDialog("On the given link are no RSS Feeds !");
                var result = errorDialog.ShowAsync();
            }
            else if ((this.AddTextBox.Background as SolidColorBrush).Color == Colors.Red)
            {
                MessageDialog errorDialog = new MessageDialog(@"Incorect link... Please validate it !
Or maybe you dont have internet connection...");
                var result = errorDialog.ShowAsync();
            }
        }

        //adds link into database
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

            if ((this.AddTextBox.Background as SolidColorBrush).Color == Colors.Green)
            {
                SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
                List<string> links = new List<string>(saver.GetLinks());
                links.Add(this.AddTextBox.Text);
                saver.SaveLinks(links.ToArray());
                FeedDataSource feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
                feedDataSource.Feeds.Clear();
                var result = feedDataSource.GetFeedAsync();
                this.Frame.Navigate(typeof(ItemsPage_MainPage));
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Cannot save invalid link !");
                var result = dialog.ShowAsync();
            }           
            
        }

       

    }
}
