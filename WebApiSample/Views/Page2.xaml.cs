using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiSample.Common;
using WebApiSample.Helpers;
using WebApiSample.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WebApiSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page2 : Page
    {
        string userName = string.Empty;
        const string CommandTarget = "ShaperloT";

        const string CommandHost = "http://mywebapidemo.azurewebsites.net/api/Command";
        const string EmergenceCounterHost = "http://mywebapidemo.azurewebsites.net/api/EmergenceCounter";

        const string CmdOn = "On";
        const string CmdOff = "Off";

        List<EmergenceCounterInfo> lstEmergenceCounter;
        public Page2()
        {
            this.InitializeComponent();

            UserAccountService userAccount = new UserAccountService();
            userName = userAccount.GetUserNameFromLocker();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            lstEmergenceCounter = await GetEmergenceCounter();//获取到listEmergenceCounter,需要转化再绑定

            base.OnNavigatedTo(e);
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
                string queryString =string.Format("?userName={0}&commandTarget={1}&commandContent={2}",
                    userName, CommandTarget, commandContent);
                string url = CommandHost + queryString;
                HttpService http = new HttpService();
                await http.SendGetRequest(url);
            }
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private async Task<List<EmergenceCounterInfo>> GetEmergenceCounter()
        {
            if(userName.Length>0)
            {
                string queryString = string.Format("?userName={0}", userName);
                HttpService http = new HttpService();
                string strResponse = await http.SendGetRequest(EmergenceCounterHost + queryString);
                List<EmergenceCounterInfo> lstEmergenceCounter = new List<EmergenceCounterInfo>();
                JsonHelper.JsonToObject(strResponse, lstEmergenceCounter);
                return lstEmergenceCounter;
            }
            return null;
        }
    }
}
