using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loT4WebApiSample.Helpers
{
    public class ToastHelper
    {
        const string ToastHost = "http://mywebapidemo.azurewebsites.net/api/Notification";
        public static async Task SendToast(string content,string userName)
        {
            HttpService http = new HttpService();
            string queryString = string.Format("?content={0}&userName={1}",content,userName);
            await http.SendGetRequest(ToastHost + queryString);
        }
    }
}
