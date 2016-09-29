using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace MyWindowsBlogReader
{
    /// <summary>
    /// class that is called On launch of the applicaton
    /// </summary>
    public static class MyOnLaunch
    {
        //needed when calling dialog box multiple times
        private static IAsyncOperation<IUICommand> asyncCommand = null;

        /// <summary>
        /// Asynchronous operation that checks if computer is connected to the internet
        /// if yes that tries to download feeds from the internet
        /// if no than checks if user set the saving of the feeds 
        /// if yes than tries to load it from database 
        /// if no that shows a dialog box with error
        /// </summary>
        /// <returns></returns>
        public static async Task OnLaunch()
        {
            SQLiteSaver saver = new SQLiteSaver(SQLiteSaver.DbName);
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.InternetAccess)
            {
                FeedDataSource feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
                if (feedDataSource != null)
                {
                    if (feedDataSource.Feeds.Count == 0)
                    {
                        await feedDataSource.GetFeedAsync();
                    }
                }
            }
            else if (!saver.Settings.SavingBeforeExiting)
            {
                var dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
                MessageDialog messageDialog = new MessageDialog(@"Please check your internet connection and run your application again.
Error: No internet");

                messageDialog.Commands.Add(new UICommand("Try Again",
                    new UICommandInvokedHandler(CommandTryAgainInvokedMember)));
                messageDialog.Commands.Add(new UICommand("Close",
                    new UICommandInvokedHandler(CommandCloseInvokedMember)));

                messageDialog.Title = "Error !";

                asyncCommand = messageDialog.ShowAsync();


            }
            else
            {
                FeedDataSource feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
                //feedDataSource.Feeds = saver.LoadFeeds().Feeds;
                foreach (var item in saver.LoadFeeds().Feeds)
                {
                    feedDataSource.Feeds.Add(item);
                }
            }
        }

        // if is clicked Try button
        private static void CommandTryAgainInvokedMember(IUICommand command)
        {
            if (asyncCommand != null)
            {
                asyncCommand.Cancel();
            }
            var result = OnLaunch();
        }

        // if in dialog box is click exit button
        private static void CommandCloseInvokedMember(IUICommand command)
        {
            App.Current.Exit();
        }
    }
}