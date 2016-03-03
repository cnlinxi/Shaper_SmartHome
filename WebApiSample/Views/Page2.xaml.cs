using System;
using System.Threading.Tasks;
using WebApiSample.Common;
using WebApiSample.Helpers;
using WebApiSample.Model;
using Windows.UI.Xaml.Controls;

namespace WebApiSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page2 : Page
    {
        string userName = string.Empty;
        const string CommandTarget = "ShaperloT";
        const string CommandHost = "http://mywebapidemo.azurewebsites.net";
        const string CmdOn = "On";
        const string CmdOff = "Off";
        public Page2()
        {
            this.InitializeComponent();

            UserAccountService userAccount = new UserAccountService();
            userName = userAccount.GetUserNameFromLocker();
        }

        private async void toggleLed_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSwitch toggleLed = sender as ToggleSwitch;
            loading.IsActive = true;

            if (toggleLed.IsOn)
            {
                await SendCommand(CmdOn);
            }
            else
            {
                await SendCommand(CmdOff);
            }

            loading.IsActive = false;
        }

        private async Task SendCommand(string commandContent)
        {
            if(commandContent!=string.Empty&&userName!=string.Empty)
            {
                string queryString =
                    string.Format("/api/Command?userName={0}&commandTarget={1}&commandContent={2}",
                    userName, CommandTarget, commandContent);
                string url = CommandHost + queryString;
                HttpService http = new HttpService();
                await http.SendGetRequest(url);
            }
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
