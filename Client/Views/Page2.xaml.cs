using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiSample.Common;
using WebApiSample.Helpers;
using WebApiSample.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace WebApiSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page2 : Page
    {
        string userName = string.Empty;

        const string EmergenceCounterHost = "http://mywebapidemo.azurewebsites.net/api/EmergenceCounter";
        const string TemperatureHost = "http://mywebapidemo.azurewebsites.net/api/Temperature";

        List<EmergenceCounterInfo> lstEmergenceCounter;
        TemperatureInfo temperature;
        public Page2()
        {
            this.InitializeComponent();

            UserAccountService userAccount = new UserAccountService();
            userName = userAccount.GetUserNameFromLocker();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            loading.IsActive = true;

            await InitialTemperature();
            await InitialChart();

            loading.IsActive = false;

            base.OnNavigatedTo(e);
        }

        private async Task InitialTemperature()
        {
            temperature = await GetTemperature();

            if (temperature != null)
            {
                gauge_temperature.Value = Convert.ToDouble(temperature.temperature);
                gauge_huminity.Value = Convert.ToDouble(temperature.humidity);
            }
        }

        private async Task InitialChart()
        {
            lstEmergenceCounter = await GetEmergenceCounter();
            if (lstEmergenceCounter != null)
            {
                bool isZero = true;
                foreach (var obj in lstEmergenceCounter)
                {
                    //现在这样写的原因是IDE变成SB了，不这样我草泥马的编译不过去
                    if (!obj.counter.ToString().Equals("0"))
                    {
                        isZero = false;
                        break;
                    }
                }
                if (isZero)
                {
                    //没有数据，设置图片代替图表
                }
                else
                {
                    SetChart();
                }
            }
        }

        private void SetChart()
        {
            List<NameValueItem> items = new List<NameValueItem>();
            for(int i=0;i<lstEmergenceCounter.Count;++i)
            {
                //这里乘20只是为了好看而已。。。。
                items.Add(new NameValueItem { Name = i.ToString(),
                    Value = 20 * Convert.ToInt32(lstEmergenceCounter[i].counter) });
            }
            AreaSeries series = (AreaSeries)this.Last3DaysDetail.Series[0];
            series.ItemsSource = items;

            series.DependentRangeAxis =
                    new LinearAxis
                    {
                        Minimum = 0,
                        Maximum = 100,
                        Orientation = AxisOrientation.Y,
                        Interval = 20,
                        ShowGridLines = false,
                        Width = 0
                    };
                series.IndependentAxis =
                    new CategoryAxis
                    {
                        Orientation = AxisOrientation.X,
                        Height = 0
                    };
        }

        //private async void toggleLed_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    ToggleSwitch toggleLed = sender as ToggleSwitch;
        //    loading.IsActive = true;

        //    if (toggleLed.IsOn)
        //    {
        //        await SendCommand(CmdOn);
        //    }
        //    else
        //    {
        //        await SendCommand(CmdOff);
        //    }

        //    loading.IsActive = false;
        //}

        //private async Task SendCommand(string commandContent)
        //{
        //    if(commandContent!=string.Empty&&userName!=string.Empty)
        //    {
        //        string queryString =string.Format("?userName={0}",userName);
        //        string url = CommandHost + queryString;
        //        TimingCommandInfo timingCommand = new TimingCommandInfo();
        //        timingCommand.userName = userName;
        //        timingCommand.command = commandContent;
        //        string jsonContent = JsonHelper.ObjectToJson(timingCommand);
        //        HttpService http = new HttpService();
        //        await http.SendPutRequest(url,jsonContent);
        //    }
        //}

        private async void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarButton btn = sender as AppBarButton;
            switch(btn.Label)
            {
                case "Refresh":
                    this.loading.IsActive = true;
                    await InitialTemperature();
                    await InitialChart();
                    this.loading.IsActive = false;
                    break;
                default:
                    break;
            }

        }

        private async Task<List<EmergenceCounterInfo>> GetEmergenceCounter()
        {
            if(userName.Length>0)
            {
                try
                {
                    string queryString = string.Format("?userName={0}", userName);
                    HttpService http = new HttpService();
                    string strResponse = await http.SendGetRequest(EmergenceCounterHost + queryString);
                    List<EmergenceCounterInfo> lstEmergenceCounter = new List<EmergenceCounterInfo>();
                    JArray jsonArray = JArray.Parse(strResponse);
                    foreach(JObject obj in jsonArray)
                    {
                        EmergenceCounterInfo counter = new EmergenceCounterInfo();
                        counter.fireCounter = obj["fireCounter"].ToString();
                        counter.strangerCounter = obj["strangerCounter"].ToString();
                        counter.counter = obj["counter"].ToString();
                        lstEmergenceCounter.Add(counter);
                    }
                    return lstEmergenceCounter;
                }
                catch { }
            }
            return null;
        }

        private async Task<TemperatureInfo> GetTemperature()
        {
            if (userName == string.Empty)
                return null;
            try
            {
                string queryString = string.Format("?userName={0}", userName);
                HttpService http = new HttpService();
                string response = await http.SendGetRequest(TemperatureHost + queryString);
                JObject jsonObject = JObject.Parse(response);
                temperature = new TemperatureInfo();
                temperature.temperature = jsonObject["temperature"].ToString();
                temperature.humidity = jsonObject["humidity"].ToString();
                return temperature;
            }
            catch { }
            return null;
        }

        private void btnShowDetails_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (lstEmergenceCounter == null)
                return;
            FamilyStatusSumInfo familySum = new FamilyStatusSumInfo();
            int sumFire = 0;
            int sumStranger = 0;
            for(int i=0;i<lstEmergenceCounter.Count;++i)
            {
                sumFire +=Convert.ToInt32(lstEmergenceCounter[i].fireCounter);
                sumStranger += Convert.ToInt32(lstEmergenceCounter[i].strangerCounter);
            }
            familySum.FireCounter = sumFire;
            familySum.StrangerCounter = sumStranger;
            this.Frame.Navigate(typeof(FamilyStatusDetails), familySum);
        }
    }
}
