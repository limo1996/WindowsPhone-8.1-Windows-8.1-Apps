using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Windows.System;
using Windows.Storage;


namespace MyWindowsBlogReader
{
    //sends an email if user have email sender installed on computer
    //otherwise did nothing
    public class EmailSender
    {
        public async static Task SendMessage(string to, string subject, string message, string Name)
        {
            //"mailto:?to=tickets@msiccdev.uservoice.com&subject=this is the Subject&body=this is the Body"
            await Launcher.LaunchUriAsync(new Uri("mailto:?to="+ to + "&subject=" + subject + "&body=" + "Message from" + Name + " -----------------"+message));
        }
    }    
}
