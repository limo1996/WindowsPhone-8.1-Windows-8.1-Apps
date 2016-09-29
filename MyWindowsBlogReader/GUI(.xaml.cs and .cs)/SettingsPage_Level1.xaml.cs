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
    public sealed partial class SettingsPage_Level1 : Page
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


        public SettingsPage_Level1()
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
            //fill the text and combo boxes with data saved
            SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
            this.SaveCheckBox.IsChecked = saver.Settings.SavingBeforeExiting;
            if (saver.Settings.OutputPath != null)
            {
                this.OutputPathTextBox.Text = saver.Settings.OutputPath;
            }
            if(saver.Settings.Filename != null && saver.Settings.Filename != "")
            {
                this.OutputFilenameTextBox.Text = saver.Settings.Filename;
            }
            this.ComboBoxExportFormat.SelectedItem = (from item in ComboBoxExportFormat.Items
                                                      where (item as ComboBoxItem).Content.ToString() == Settings.FromEnumString(saver.Settings.ExportFormat)
                                                      select item).First();
                //ComboBoxExportFormat.FindName(Settings.FromEnumString(saver.Settings.ExportFormat));
            
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

        //sends an message via local email client
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string Name = this.NameTextBox.Text;
            string Subject = this.SubjectTextBox.Text;
            string Message = this.MessageTextBox.Text;

            var result = MyWindowsBlogReader.EmailSender.SendMessage("lichman.jakub@gmail.com",Subject,Message,Name);
        }

        //deletes text in text box a change font color
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Text = "";
            textBox.Foreground = new SolidColorBrush(Colors.Black);
            textBox.GotFocus -= TextBox_GotFocus;
        }

        //saves all current feed datas into local database
        private void SaveFeed_Click(object sender, RoutedEventArgs e)
        {
            SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
            FeedDataSource feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            saver.SaveFeeds(feedDataSource);         
        }

        //save all changes being made
        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            bool Checked = false;
            if(this.SaveCheckBox.IsChecked == true)
                Checked = true;
            SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
            saver.Settings = new Settings(SQLiteSaver.DbName,Checked);
            saver.Settings.Filename = this.OutputFilenameTextBox.Text;
            saver.Settings.OutputPath = this.OutputPathTextBox.Text;
            saver.Settings.OutputFilename = this.OutputFilenameTextBox.Text;
            saver.Settings.ExportFormat = Settings.FromStringEnum(
                (this.ComboBoxExportFormat.SelectedItem as ComboBoxItem).Content.ToString());
            saver.SaveSettings();
        }

        //reset -> deletes all information from tables in database
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Are you sure ?");
            dialog.Commands.Add(new UICommand("Yes",new UICommandInvokedHandler(DeleteAllSettings)));
            dialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(CancelCommand)));
            var result = dialog.ShowAsync();
        }

        private void DeleteAllSettings(IUICommand command)
        {
            SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
            saver.ClearAllTables();
            saver.Settings = new Settings(SQLiteSaver.DbName, false);
            saver.Settings.OutputPath = "";
            saver.Settings.Filename = "MyBlog";
            saver.Settings.ExportFormat = ExportFormat.XML;
            FeedDataSource feedDataSource = new FeedDataSource();
            var result = feedDataSource.GetFeedAsync();
            App.Current.Resources["feedDataSource"] = feedDataSource;            
            
            this.Frame.Navigate(typeof(ItemsPage_MainPage));
        }

        private void CancelCommand(IUICommand command)
        {
            this.Frame.Navigate(typeof(SettingsPage_Level1));
        }
    }
}