using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Timetable.Common;
using Timetable.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Timetable
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<string> stations = new List<string>();
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (User.Current != null)
            {
                this.ActualUserTextBox.Text = User.Current.FullName;
            }

            SQLiteLoader loader = new SQLiteLoader();
            foreach (var item in loader.GetAutocorrectionStrings())
            {
                stations.Add(item.AutocorrectionString);
            }
            // TODO: Prepare page for display here.
            if (e.Parameter != null && e.Parameter != "")
            {
                if ((e.Parameter as Tuple<bool, string>).Item1)
                    this.FromTextBox.Text = (e.Parameter as Tuple<bool, string>).Item2;
                else
                    this.ToTextBox.Text = (e.Parameter as Tuple<bool, string>).Item2;
            }
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Foreground = new SolidColorBrush(Colors.Black);
            (sender as TextBox).Text = "";
            (sender as TextBox).GotFocus -= TextBox_GotFocus;
        }

        private void GetPosition(object sender, RoutedEventArgs e)
        {
            if ((sender as AppBarButton).Name == "FromGetPosition")
            {
                this.Frame.Navigate(typeof(PositionPage), true);
            }
            else
            {
                this.Frame.Navigate(typeof(PositionPage), false);
            }
        }

        private void Switch(object sender, RoutedEventArgs e)
        {
            string tmp = this.FromTextBox.Text;
            this.FromTextBox.Text = this.ToTextBox.Text;
            this.ToTextBox.Text = tmp;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SmallSettings));
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            SearchingSetttings ss = App.Current.Resources["SearchingSettings"] as SearchingSetttings;

            if(ss == null)
            {
                ss = new SearchingSetttings()
                {
                    OnlyDirectConnections = false,
                    Criterium = CRIT.TIME,
                    StartingTime = DateTime.Now,
                    ResultsCount = 5
                };
            }
            if (ss.ResultsCount == 0)
                ss.ResultsCount = 5;

            ComboBox rangeBox = new ComboBox();
            rangeBox.Items.Add(ss.Criterium == CRIT.DISTANCE ? "Shortest distance" : "Shortest time");
            rangeBox.SelectedItem = ss.Criterium == CRIT.DISTANCE ? "Shortest distance" : "Shortest time";

            ComboBox resultsCount = new ComboBox();
            ComboBoxItem item = new ComboBoxItem() { Content = ss.ResultsCount, IsSelected = true };
            resultsCount.Items.Add(item);

            ResultsContainer resultsContainer = Timetable.Code.Search.Execute(FromTextBox, ToTextBox,
                new TextBox() { Text = string.Format("{0:00}:{0:00}", ss.StartingTime.Hour, ss.StartingTime.Minute) },
                rangeBox, new CheckBox() { IsChecked = ss.OnlyDirectConnections }, resultsCount);

            if (resultsContainer != null)
            {
                App.Current.Resources["Results"] = resultsContainer;
                this.Frame.Navigate(typeof(ResultPage));
            }            
        }

        private void SearchButton_DragOver(object sender, DragEventArgs e)
        {
            SearchButton.Background = App.Current.Resources["SearchButtonMouseOverBackgroundThemeBrush"] as SolidColorBrush;
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            if (User.Current == null)
            {
                this.Frame.Navigate(typeof(LoginPage), true);
            }
            else
            {
                this.Frame.Navigate(typeof(SettingsPage));
            }
        }

        private void TextBox_TextChanges(object sender, TextChangedEventArgs e)
        {
            string station = (sender as TextBox).Text;
            foreach (var item in stations)
            {
                if (item == station)
                {
                    (sender as TextBox).Background = new SolidColorBrush(Colors.Green);
                    return;
                }
            }
            (sender as TextBox).Background = new SolidColorBrush(Colors.Red);
        }

        private void Navigate_InfoPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Timetable.Pages.InfoPage));
        }

        private void LogIn(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Timetable.Pages.LoginPage),false);
        }
    }
}
