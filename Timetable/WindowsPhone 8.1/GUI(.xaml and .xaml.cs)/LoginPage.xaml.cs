using Timetable.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Timetable.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private bool navigateToSettingsPage;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public LoginPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
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
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (User.Current != null)
            {
                this.ActualUserTextBox.Text = User.Current.FullName;
            }
            this.navigateToSettingsPage = (bool)e.NavigationParameter;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void LoginButton_DragOver(object sender, DragEventArgs e)
        {
            (sender as Button).Background = App.Current.Resources["LogInButtonDragOverBackgroundBrush"] as SolidColorBrush;
        }

        private void LogIn(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Timetable.Pages.LoginPage), this.navigateToSettingsPage);
        }

        private void Navigate_InfoPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(InfoPage));
        }

        private void Navigate_Settings(object sender, RoutedEventArgs e)
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = "";
            (sender as TextBox).Foreground = new SolidColorBrush(Colors.Black);
            (sender as TextBox).GotFocus -= TextBox_GotFocus;
        }

        private void PasswordBox_TextBox(object sender, RoutedEventArgs e)
        {
            (sender as PasswordBox).Password= "";
            (sender as PasswordBox).Foreground = new SolidColorBrush(Colors.Black);
            //(sender as TextBox).GotFocus -= PasswordTextBox_GotFocus;
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            PathBuilder pb = new PathBuilder(SQLiteLoader.DbName);
            if (this.UsernameTextBox.Text == "admin" && this.PasswordTextBox.Password == "admin")
            {
                User.Current = UserData.Create(1, "admin", "admin", "Jakub Lichman");
                if (navigateToSettingsPage)
                    this.Frame.Navigate(typeof(SettingsPage));
                else
                    this.Frame.Navigate(typeof(MainPage));
            }
            else if (pb.CheckLogIn(this.UsernameTextBox.Text, this.PasswordTextBox.Password))
            {
                User.Current = pb.GetUser(this.UsernameTextBox.Text, this.PasswordTextBox.Password);
                if (navigateToSettingsPage)
                    this.Frame.Navigate(typeof(SettingsPage));
                else
                    this.Frame.Navigate(typeof(MainPage));
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Incorrect password or username");
                var result = dialog.ShowAsync();
            }
        }
    }
}
