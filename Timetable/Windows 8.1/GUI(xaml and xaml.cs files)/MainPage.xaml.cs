using Timetable.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Windows.UI;
using Windows.UI.Popups;


namespace Timetable
{
   
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<string> stations = new List<string>();
        //from clicked = true, to clicked = false
        private bool FromOrToClickedButton;
        //holds info if FromAppBarButton or ToAppButtons are clicked or not
        private bool AppBarButtonClicked = false;
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


        public MainPage()
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
            this.TimeTextBox.Text = DisplayTime();
            if (User.Current != null)
                this.LogInUserTextBlock.Text = User.Current.UserName;

            SQLiteLoader loader = new SQLiteLoader();
            foreach (var item in loader.GetAutocorrectionStrings())
            {
                stations.Add(item.AutocorrectionString);
            }
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

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AppBarButtonClicked)
            {
                this.GetPositionPopup.IsOpen = true;
                AppBarButtonClicked = true;
            }
            else
            {
                this.GetPositionPopup.IsOpen = false;
                AppBarButtonClicked = false;
            }
            Button eventSender = sender as Button;
            if (eventSender.Name == "FromFindStationAppBarButton")
            {
                FromOrToClickedButton = true;
            }
            else
            {
                FromOrToClickedButton = false;
            }
        }


        private void Button_DragOver(object sender, DragEventArgs e)
        {
            (sender as Button).Background = App.Current.Resources["SearchButtonMouseOverBackgroundThemeBrush"] as SolidColorBrush;
        }

        private void FindStationButton_Click(object sender, RoutedEventArgs e)
        {
            double tryParseLat, tryParseLon;
            if (!double.TryParse(this.LatTextBox.Text, out tryParseLat) || tryParseLat < 0 || tryParseLat > 180)
            {
                this.LatTextBox.Text = "Incorrect Format";
                return;
            }
            else if (!double.TryParse(this.LonTextBox.Text, out tryParseLon) || tryParseLon < 0 || tryParseLon > 180)
            {
                this.LonTextBox.Text = "Incorrect Format";
                return;
            }
            else
            {
                Graph graph = new Graph();
                graph.LoadPositionOfStop();
                string closestStop = graph.FindClosestStop(tryParseLat, tryParseLon);

                if (closestStop != null)
                {
                    if (FromOrToClickedButton)
                    {
                        this.FromTextBox.Text = closestStop;
                    }
                    else
                    {
                        this.ToTextBox.Text = closestStop;
                    }
                }
                else
                {
                    if (FromOrToClickedButton)
                    {
                        this.FromTextBox.Text = "Empty database";
                    }
                    else
                    {
                        this.ToTextBox.Text = "Empty database";
                    }
                }
            }
            this.GetPositionPopup.IsOpen = false;
        }

        private void InfoBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Timetable.Pages.InfoPage));
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            if (User.Current == null)
            {
                this.Frame.Navigate(typeof(LoginPage),true);
            }
            else
            {
                this.Frame.Navigate(typeof(SettingsPage));
            }
        }

        private void NavigateToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage),false);
        }

        private void Switch(object sender, RoutedEventArgs e)
        {
            string tmp = this.FromTextBox.Text;
            this.FromTextBox.Text = this.ToTextBox.Text;
            this.ToTextBox.Text = tmp;
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            string[] Splited = TimeTextBox.Text.Split(':');
            int hours = int.Parse(Splited[0]);
            int minutes = int.Parse(Splited[1]);

            if (minutes == 0)
                minutes = 30;
            else
            {
                minutes = 0;
                if (hours >= 0 && hours < 23)
                    hours++;
                else if (hours == 23)
                    hours = 0;
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("{0:00}:{1:00}", hours, minutes);

            TimeTextBox.Text = builder.ToString();
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            string[] Splited = TimeTextBox.Text.Split(':');
            int hours = int.Parse(Splited[0]);
            int minutes = int.Parse(Splited[1]);

            if (minutes == 30)
                minutes = 0;
            else
            {
                minutes = 30;
                if (hours > 0 && hours < 24)
                    hours--;
                else if (hours == 0)
                    hours = 23;
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("{0:00}:{1:00}", hours, minutes);

            TimeTextBox.Text = builder.ToString();
        }
        private string DisplayTime()
        {
            int hours = DateTime.Now.Hour;
            int minutes = DateTime.Now.Minute;

            if (minutes < 30)
                minutes = 0;
            else
                minutes = 30;

            StringBuilder returned = new StringBuilder();
            returned.AppendFormat("{0:00}:{1:00}", hours, minutes);

            return returned.ToString();
        }


        //call the static method execute that collects all the data and returns it
        //if result is not null
        //set in as an app resource
        private void Search(object sender, RoutedEventArgs e)
        {
            ResultsContainer resultsContainer = Timetable.Code.Search.Execute(FromTextBox, ToTextBox, TimeTextBox,
                RangeComboBox, OnlyDirectConnections, DisplayItemsComboBox);

            if (resultsContainer != null)
            {
                App.Current.Resources["Results"] = resultsContainer;
                this.Frame.Navigate(typeof(Timetable.Pages.DeparturesPage));
            }
        }

        private void TextBox_TextChanges(object sender, TextChangedEventArgs e)
        {
            string station = (sender as TextBox).Text;
            foreach(var item in stations)
            {
                if (item == station)
                {
                    (sender as TextBox).Background = new SolidColorBrush(Colors.Green);
                    return;
                }
            }
            (sender as TextBox).Background = new SolidColorBrush(Colors.Red);
        }

        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            this.AdvancedSettingsPopup.IsOpen = AdvancedSettingsPopup.IsOpen ? false : true;
        }
    }
}
