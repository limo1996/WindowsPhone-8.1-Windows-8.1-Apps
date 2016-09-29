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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace MyWindowsBlogReader
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BlogWriterPage_Level1 : Page
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


        public BlogWriterPage_Level1()
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

        //fill text boxes with strings loaded from file type XML, TXT, HTML, JSON, PDF
        private async void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            string path = FilePathTextBox.Text; 
            FileConverter fileConverter = new FileConverter();
            ExportFormat format;
            

            if (path.EndsWith(".txt"))
            {
                format = ExportFormat.TXT;
            }
            else if (path.EndsWith(".html"))
            {
                format = ExportFormat.HTML;
            }
            else if (path.EndsWith(".xml"))
            {
                format = ExportFormat.XML;
            }
            else if (path.EndsWith(".json"))
            {
                format = ExportFormat.JSON;
            }
            else if (path.EndsWith(".pdf"))
            {
                format = ExportFormat.PDF;
            }
            else
            {
                this.FilePathTextBox.Background = new SolidColorBrush(Colors.Red);
                MessageDialog dialog = new MessageDialog("Incorrect file path !");
                var result = dialog.ShowAsync();
                return;
            }

            Tuple<string, string, string, string, string, DateTime> Feed =
                await fileConverter.Convert(path, format);
            
            this.TitleTextBox.GotFocus -= TextBox_GotFocus;
            this.AuthorTextBox.GotFocus -= TextBox_GotFocus;
            this.GuidTextBox.GotFocus -= TextBox_GotFocus;;
            this.LinkTextBox.GotFocus -= TextBox_GotFocus;
            this.DescriptionTextBox.GotFocus -= TextBox_GotFocus;
            this.DateTextblock.GotFocus -= TextBox_GotFocus;

            this.TitleTextBox.Foreground = new SolidColorBrush(Colors.Black);
            this.AuthorTextBox.Foreground = new SolidColorBrush(Colors.Black);
            this.GuidTextBox.Foreground = new SolidColorBrush(Colors.Black);
            this.LinkTextBox.Foreground = new SolidColorBrush(Colors.Black);
            this.DescriptionTextBox.Foreground = new SolidColorBrush(Colors.Black);
            this.DateTextblock.Foreground = new SolidColorBrush(Colors.Black);

            this.TitleTextBox.Text = Feed.Item1;
            this.AuthorTextBox.Text = Feed.Item2;
            this.GuidTextBox.Text = Feed.Item3;
            this.LinkTextBox.Text = Feed.Item4;
            this.DescriptionTextBox.Text = Feed.Item5;
            this.DateTextblock.Text = Feed.Item6.ToString();
        }

        //removes text and change foreground color to black
        private void FilePathTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FilePathTextBox.Text = "";
            FilePathTextBox.Foreground = new SolidColorBrush(Colors.Black);
            FilePathTextBox.GotFocus -= FilePathTextBox_GotFocus;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Foreground = new SolidColorBrush(Colors.Black);
            (sender as TextBox).Text = "";
            (sender as TextBox).GotFocus -= TextBox_GotFocus;
        }

        //saves feed into file asynchronously
        private async void SaveFeed_Click(object sender, RoutedEventArgs e)
        {
            string title = this.TitleTextBox.Text;
            string author = this.AuthorTextBox.Text;
            string guid = this.GuidTextBox.Text;
            string link = this.LinkTextBox.Text;
            string content = this.DescriptionTextBox.Text;

            FileConverter converter = new FileConverter();
            await converter.Save(author, title, link, guid,content);
        }

        //chech correct input file 
        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string path= this.FilePathTextBox.Text;

            if (path.EndsWith(".txt") || path.EndsWith(".html") || path.EndsWith(".xml") ||
                path.EndsWith(".pdf") || path.EndsWith(".json"))
            {
                FilePathTextBox.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                FilePathTextBox.Background = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
