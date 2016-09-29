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
using Windows.UI;
using Windows.UI.Popups;
using SQLite;
using System.Xml.Linq;
using Timetable.Code;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Timetable
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        //programmer defined variables
        private LineContainer lineContainer = new LineContainer();
        private int distanceValue = 0;
        private int timeValue = 0;

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


        public SettingsPage()
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
            this.DeparturesComboBox2.Items.Add("--");
            for (int i = 0; i < 60; i++)
            {
                if (i < 24)
                {
                    this.DeparturesComboBox1.Items.Add(i.ToString());
                }
                this.DeparturesComboBox2.Items.Add(i.ToString());
            }
            if(User.Current != null)
                this.LogInUserTextBlock.Text = User.Current.UserName;

            SQLiteLoader loader = new SQLiteLoader();
            foreach (var item in loader.GetIdsOfLines())
                this.IdsOfLineCombobox.Items.Add(item);
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

        private void NavigateToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage),true);
        }

        private void HelpAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Timetable.Pages.InfoPage));
        }

        private void StationsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.StationsTextBox.Text != null)
            {
                string[] stations = this.StationsTextBox.Text.Split(',');
                if (stations[0] != "")
                {
                    this.StationFromComboBox.Items.Clear();
                    this.StationComboBox.Items.Clear();
                    this.StationToComboBox.Items.Clear();

                    foreach (var station in stations)
                    {
                        this.StationFromComboBox.Items.Add(station);
                        this.StationComboBox.Items.Add(station);
                        this.StationToComboBox.Items.Add(station);
                    }
                }
                this.XStationsDetected.Text = string.Format("{0} Stations detected", stations.GetLength(0).ToString());
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = "";
            (sender as TextBox).Foreground = new SolidColorBrush(Colors.Black);
            (sender as TextBox).GotFocus -= TextBox_GotFocus;
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;
            if (int.TryParse(ValueTextBox.Text, out value))
            {
                if (value > 0)
                {
                    (sender as TextBox).Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    (sender as TextBox).Background = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                (sender as TextBox).Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void AddRange_Click(object sender, RoutedEventArgs e)
        {
            if ((ValueTextBox.Background as SolidColorBrush).Color == Colors.Green)
            {
                if ((this.RangeComboBox.SelectedItem as ComboBoxItem).Content.ToString() == "Distance")
                {
                    this.distanceValue = int.Parse(this.ValueTextBox.Text);
                }
                else
                {
                    this.timeValue = int.Parse(this.ValueTextBox.Text);
                }

                if (this.timeValue != 0 && this.distanceValue != 0)
                {
                    string fromString = this.StationFromComboBox.SelectedItem.ToString();
                    string toString = this.StationToComboBox.SelectedItem.ToString();

                    this.lineContainer.Ranges[Tuple.Create<string, string>(fromString, toString)]
                         = Tuple.Create<int, int>(distanceValue, timeValue);

                    this.distanceValue = 0;
                    this.timeValue = 0;
                    this.ValueTextBox.Text = "";
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Error: Invalid value in 'Value (int)' box !");
                var result = dialog.ShowAsync();
            }
        }

        private void AddPosition_Click(object sender, RoutedEventArgs e)
        {
            if((LatTextBox.Background as SolidColorBrush).Color == Colors.Green && (LonTextBox.Background as SolidColorBrush).Color == Colors.Green)
            {
                string station = this.StationComboBox.SelectedItem.ToString();
                double lat = double.Parse(LatTextBox.Text);
                double lon = double.Parse(LonTextBox.Text);
                this.lineContainer.Positions[station] = Tuple.Create<double, double>(lat, lon);
                LatTextBox.Text = "";
                LonTextBox.Text = "";
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Latitude or longitude is not in correct format !");
                var result = dialog.ShowAsync();
            }
        }

        private void LatLonTextChanged(object sender, TextChangedEventArgs e)
        {
            double value;
            if (double.TryParse((sender as TextBox).Text, out value))
            {
                if (value > 0)
                {
                    (sender as TextBox).Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    (sender as TextBox).Background = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                (sender as TextBox).Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void AddDeparture_Click(object sender, RoutedEventArgs e)
        {
            int minute;
            int? finalMinute;
            int hour = int.Parse(this.DeparturesComboBox1.SelectedItem as string);
            if (!int.TryParse(this.DeparturesComboBox2.SelectedItem as string, out minute))
            {
                finalMinute = null;
            }
            else
            {
                finalMinute = minute;
            }
            this.lineContainer.Departures[hour] = finalMinute;
        }

        private void AddLine(object sender, RoutedEventArgs e)
        {
            this.lineContainer.IdOfLine = IdOfLineTextBox.Text;
            this.lineContainer.Stations = new List<string>(StationsTextBox.Text.Split(','));
            this.lineContainer = HH();//delete
            if (lineContainer.IsCorrect())
            {
                LineSaver lineSaver = new LineSaver();
                lineSaver.Save(this.lineContainer);
                SQLiteLoader loader = new SQLiteLoader();

                this.IdsOfLineCombobox.Items.Clear();

                foreach (var item in loader.GetIdsOfLines())
                    this.IdsOfLineCombobox.Items.Add(item);

                MessageDialog dialog = new MessageDialog("Line successfully added !");
                var result = dialog.ShowAsync();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {           
            SQLiteLoader loader = new SQLiteLoader(SQLiteLoader.DbName);
            loader.Reset();

            this.IdsOfLineCombobox.Items.Clear();
        }
        //delete
        private LineContainer HH()
        {
            LineContainer ll = new LineContainer();
            for (int i = 0; i < 24; i++)
                ll.Departures.Add(i, 25);
            ll.IdOfLine = "ZA-FL-02";
            ll.Stations = new List<string>(new string[] { "Zilina", "BanskaBystrica", "Filakovo" });
            ll.Positions.Add("Zilina", Tuple.Create<double, double>(48, 56));
            ll.Positions.Add("BanskaBystrica", Tuple.Create<double, double>(48, 56));
            ll.Positions.Add("Filakovo", Tuple.Create<double, double>(48, 56));
            ll.Ranges.Add(Tuple.Create<string, string>("Zilina", "BanskaBystrica"), Tuple.Create<int, int>(100, 100));
            ll.Ranges.Add(Tuple.Create<string, string>("BanskaBystrica", "Filakovo"), Tuple.Create<int, int>(100, 100));
            return ll;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            int parsed;
            if (int.TryParse(this.ID.Text, out parsed))
            {
                SQLiteLoader loader = new SQLiteLoader(SQLiteLoader.DbName);
                loader.SaveUserData(new UserData()
                {
                    FullName = this.Fullname.Text,
                    UserPassword = this.Password.Text,
                    UserName = this.Username.Text,
                    ID = parsed
                }
                    );
            }
            else
            {
                MessageDialog dialog = new MessageDialog("ID of user must be integer !");
                var result = dialog.ShowAsync();
            }
        }

        private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteLoader loader = new SQLiteLoader();
            loader.DeleteLine(this.IdsOfLineCombobox.SelectedItem as string);
                        
            this.IdsOfLineCombobox.Items.Clear();
            foreach (var item in loader.GetIdsOfLines())
                this.IdsOfLineCombobox.Items.Add(item);
        }

        private void SaveLineToXmlButton_Click(object sender, RoutedEventArgs e)
        {
            this.lineContainer.IdOfLine = IdOfLineTextBox.Text;
            this.lineContainer.Stations = new List<string>(StationsTextBox.Text.Split(','));
            this.lineContainer = HH();//delete
                TimetableXML.SaveToXml(this.lineContainer);
                
        }
    }
}
