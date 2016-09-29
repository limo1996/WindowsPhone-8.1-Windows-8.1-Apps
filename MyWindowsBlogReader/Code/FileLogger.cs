using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyWindowsBlogReader.Code
{
    //log error messages into file
    public static class FileLogger
    {
        public static string Filename = "ErrorLoggerFileMyWinBlogReader.txt";
        public static async void WriteFile(string filename, string content)
        {
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            //Windows.Storage.ApplicationData.Current.LocalFolder.
            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            
            //appends text into logger file
            await FileIO.AppendTextAsync(file, content);
            
        }
    }
}
