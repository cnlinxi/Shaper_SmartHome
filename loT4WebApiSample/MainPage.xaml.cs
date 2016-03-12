using loT4WebApiSample.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading.Tasks;
using Windows.Storage;
using WebApiSample.Model;
using loT4WebApiSample.Common;
using loT4WebApiSample.Model;
using Windows.Web.Http;
using Sensors.Dht;
using Windows.Networking.PushNotifications;
using Newtonsoft.Json.Linq;
using Windows.Devices.Gpio;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace loT4WebApiSample
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WebCamHelper camera;
        private SpeechHelper speech;
        private GpioHelper gpioHelper;
        private FaceApiHelper faceApi;

        const string storageAccountName = "myblobsample";
        const string storageAccountKey = "JFyiGqG4Av5U7yc0RYIkvyHDW7taz5hl0TPdFLssZaUejbVKdkJvfhUEXNkfVU2usnEPzBc2MdzrCbvURHzrrQ==";
        const string containerName = "mycontainer";
        private BlobHelper blobHelper;

        //本地数据容器，重装可同步
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        //多平台同步容器
        private StorageFolder roamdingFolder = ApplicationData.Current.RoamingFolder;
        ApplicationDataContainer roamdingSettings = ApplicationData.Current.RoamingSettings;

        private DispatcherTimer timer_FaceRecognization;
        private DispatcherTimer timer_DhtSendValue;
        private DispatcherTimer timer_SendEmergenceCounter;
        //private DispatcherTimer timer_Test;
        private DispatcherTimer timer_GetTimingCommand;
        private DispatcherTimer timer_InitialDevice;

        private bool isGpioValuable = false;
        private bool isDoorbellJustPress = false;

        private string userName = string.Empty;
        private const string emergencePictureHost = "http://mywebapidemo.azurewebsites.net/api/EmergencyPicture";
        const string commandHost = "http://mywebapidemo.azurewebsites.net/api/Command";
        const string temperatureHost = "http://mywebapidemo.azurewebsites.net/api/Temperature";
        const string notificationHost = "http://mywebapidemo.azurewebsites.net/api/Notification";
        const string emergenceCounterHost = "http://mywebapidemo.azurewebsites.net/api/EmergenceCounter";
        const string timingCommandHost = "http://mywebapidemo.azurewebsites.net/api/TimingCommand";
        const string initialDeviceHost = "http://mywebapidemo.azurewebsites.net/api/InitialDevice";

        //统计识别失败的次数，每5分钟重置一次
        private int faceRecognizationCount = 0;

        private static int FireCounter = 0;
        private static int StrangerCounter = 0;

        TimeSpan tsThree = new TimeSpan(3, 0, 0);//早晨3点发送EmergenceCounter

        private PushNotificationChannel channel;
        const string CmdOn = "On";
        const string CmdOff = "Off";

        const string ContainerName_UserName = "UserName";
        private string authCode;

        public MainPage()
        {
            this.InitializeComponent();

            faceApi = new FaceApiHelper();
            if(!isGpioValuable)
            {
                InitializeGpio();
            }

            InitializeTimer();
            InitializeBlobStorage();
            //InitialCommand();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(localSettings.Values.ContainsKey(ContainerName_UserName))
            {
                userName = localSettings.Values[ContainerName_UserName].ToString();
                tbMessage.Text = "你好，" + userName;
            }
            else
            {
                InitializeDevice();
            }

            base.OnNavigatedTo(e);
        }

        public void InitializeDevice()
        {
            Random random = new Random();
            authCode = random.Next(10000, 99999).ToString();
            tbMessage.Text = "你的设备尚未初始化！请至PC/Mobile端初始化中心输入以下数字，以完成设备初始化，验证码有效时间30分钟：";
            tbAuthCode.Text = authCode;
            timer_InitialDevice = new DispatcherTimer();
            timer_InitialDevice.Interval = TimeSpan.FromSeconds(17);
            timer_InitialDevice.Tick += Timer_InitialDevice_Tick;
            timer_InitialDevice.Start();
        }

        private async void Timer_InitialDevice_Tick(object sender, object e)
        {
            try
            {
                HttpService http = new HttpService();
                string queryString = string.Format("?authCode={0}", authCode);
                string response = await http.SendGetRequest(initialDeviceHost + queryString);
                if (response.Length > 0)
                {
                    JObject jsonObject = JObject.Parse(response);
                    userName = jsonObject["userName"].ToString();
                    localSettings.Values[ContainerName_UserName] = userName;
                    tbMessage.Text = "你好，" + userName + "。开启你的Shaper智能家居之旅";
                    tbAuthCode.Text="初始化完成";
                    timer_InitialDevice.Stop();
                }
            }
            catch { }
        }

        public void InitializeTimer()
        {
            //人脸识别失败次数重置定时器
            timer_FaceRecognization = new DispatcherTimer();
            timer_FaceRecognization.Interval = TimeSpan.FromMinutes(Constants.FaceConstants.FaceRecognizationFailedDuration);
            timer_FaceRecognization.Tick += Timer_FaceRecognization_Tick;

            //Dht11温湿度传感器发送数据定时器
            timer_DhtSendValue = new DispatcherTimer();
            timer_DhtSendValue.Interval = TimeSpan.FromMinutes(Constants.GpioConstants.DhtSendValueDuration);
            timer_DhtSendValue.Tick += Timer_DhtSendValue_Tick;
            timer_DhtSendValue.Start();

            //每天发生的威胁次数发送数据定时器
            timer_SendEmergenceCounter = new DispatcherTimer();
            timer_SendEmergenceCounter.Interval = TimeSpan.FromHours(Constants.EmergenceCounter.SendEmergenceCounterDuration);
            timer_SendEmergenceCounter.Tick += Timer_SendEmergenceCounter_Tick;
            timer_SendEmergenceCounter.Start();

            //远程控制的小灯（现在没有什么解决方案，每5秒询问一次）
            timer_GetTimingCommand = new DispatcherTimer();
            timer_GetTimingCommand.Interval = TimeSpan.FromSeconds(Constants.TimingCommand.GetTimingCommandDuration);
            timer_GetTimingCommand.Tick += Timer_GetTimingCommand_Tick;
            timer_GetTimingCommand.Start();

            //timer_Test = new DispatcherTimer();
            //timer_Test.Interval = TimeSpan.FromSeconds(30);
            //timer_Test.Tick += Timer_Test_Tick;
            //timer_Test.Start();
        }

        private async void Timer_GetTimingCommand_Tick(object sender, object e)
        {
            if (userName==string.Empty)
                return;

            HttpService http = new HttpService();
            string queryString = string.Format("?userName={0}",userName);
            string response = await http.SendGetRequest(timingCommandHost + queryString);
            try
            {
                if (response.Length <= 0)
                    return;
                TimingCommandInfo timingCommand = new TimingCommandInfo();
                JObject json = JObject.Parse(response);
                timingCommand.userName = json["userName"].ToString();
                timingCommand.id = json["id"].ToString();
                timingCommand.command = json["command"].ToString();
                timingCommand.addTime = json["addTime"].ToString();
                if (timingCommand == null || !isGpioValuable)
                    return;
                if (timingCommand.command == CmdOn)
                    gpioHelper.OnTestLED();
                else
                    gpioHelper.OffTestLED();
            }
            catch { }
        }

        private async void Timer_Test_Tick(object sender, object e)
        {
            
        }

        private async void InitialCommand()
        {
            try
            {
                if (userName != string.Empty)
                {
                    channel = 
                        await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                    channel.PushNotificationReceived += Channel_PushNotificationReceived;

                    if (roamdingSettings.Values["channel"] == null
                        || roamdingSettings.Values["channel"].ToString() != channel.Uri)
                    {
                        CommandChannelInfo channelModel = new CommandChannelInfo();
                        channelModel.userName = userName;
                        channelModel.channelUri = channel.Uri;
                        channelModel.expirationTime = channel.ExpirationTime.DateTime;
                        string strJson = JsonHelper.ObjectToJson(channelModel);
                        HttpService httpService = new HttpService();
                        await httpService.SendPostRequest(commandHost, strJson);
                        roamdingSettings.Values["channel"] = channel.Uri;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            RawNotification rawNotification = args.RawNotification;
            string commandContent = rawNotification.Content;
            if(commandContent==CmdOn&&isGpioValuable)
            {
                gpioHelper.OnTestLED();
            }
            else if(commandContent==CmdOff&&isGpioValuable)
            {
                gpioHelper.OffTestLED();
            }
        }

        private async void Timer_DhtSendValue_Tick(object sender, object e)
        {
            if (!isGpioValuable)
                return;

            if (userName == string.Empty)
                return;

            DhtReading reader = new DhtReading();
            reader = await gpioHelper.GetDht().GetReadingAsync();//开始读取温湿度
            if(reader.IsValid)
            {
                double temperature = Convert.ToDouble(reader.Temperature);//读取温度
                double humidity = Convert.ToDouble(reader.Humidity);//读取湿度
                await SendTemperature(userName, temperature, humidity);
            }
        }

        public async Task SendTemperature(string userName,double temperature,double humidity)
        {
            TemperatureInfo temperatureInfo = new TemperatureInfo();
            temperatureInfo.userName = userName;
            temperatureInfo.temperature = temperature;
            temperatureInfo.humidity = humidity;
            string jsonContent = JsonHelper.ObjectToJson(temperatureInfo);
            HttpService http = new HttpService();
            await http.SendPostRequest(temperatureHost, jsonContent);
        }

        private async void Timer_SendEmergenceCounter_Tick(object sender, object e)
        {
            TimeSpan tsInterval = DateTime.Now.TimeOfDay.Subtract(tsThree);
            double totalMinitus = tsInterval.TotalMinutes;
            if(totalMinitus>=0&&totalMinitus<60)
            {
                await SendEmergenceCounter();
            }
        }

        private async Task SendEmergenceCounter()
        {
            if(userName.Length>0)
            {
                EmergenceCounterInfo counter = new EmergenceCounterInfo();
                counter.userName = userName;
                counter.fireCounter = FireCounter.ToString();
                counter.strangerCounter = StrangerCounter.ToString();
                string jsonContent = JsonHelper.ObjectToJson(counter);
                HttpService http = new HttpService();
                await http.SendPostRequest(emergenceCounterHost, jsonContent);
                FireCounter = StrangerCounter = 0;
            }
        }

        public void InitializeBlobStorage()
        {
            blobHelper = new BlobHelper();
            blobHelper.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
        }

        private void Timer_FaceRecognization_Tick(object sender, object e)
        {
            faceRecognizationCount = 0;
            timer_FaceRecognization.Stop();
        }

        public void InitializeGpio()
        {
            try
            {
                gpioHelper = new GpioHelper();
                isGpioValuable = gpioHelper.Initialize();
            }
            catch
            {
                isGpioValuable = false;
                Debug.WriteLine("GPIO controller不可用");
            }

            if(isGpioValuable)
            {
                //gpioHelper.GetDoorBellPin().ValueChanged += doorbell_ValueChanged;
                gpioHelper.GetFireAlarm().ValueChanged += FireAlarm_ValueChanged;
                gpioHelper.GetHumanInfrare().ValueChanged += HumanInfrare_ValueChanged;
            }
        }

        private async void HumanInfrare_ValueChanged(Windows.Devices.Gpio.GpioPin sender, Windows.Devices.Gpio.GpioPinValueChangedEventArgs args)
        {
            Debug.WriteLine("HumanInfrare传感器：有人形移动！");
            if (userName == string.Empty)
                return;

            if (!isDoorbellJustPress)
            {
                if (args.Edge == GpioPinEdge.RisingEdge)
                {
                    isDoorbellJustPress = true;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                        await BeginFaceRecognization();
                    });
                    isDoorbellJustPress = false;
                }
            }
        }

        private async void FireAlarm_ValueChanged(Windows.Devices.Gpio.GpioPin sender, Windows.Devices.Gpio.GpioPinValueChangedEventArgs args)
        {
            await speech.PlayTTS(Constants.SpeechConstants.FireWariningMessage);
            Debug.WriteLine("FireAlarm传感器：Value发生变化");
            await SendFireAlarm();//向移动端推送火警通知
            ++FireCounter;
        }

        //private async void doorbell_ValueChanged(Windows.Devices.Gpio.GpioPin sender, Windows.Devices.Gpio.GpioPinValueChangedEventArgs args)
        //{
        //    if(!isDoorbellJustPress)
        //    {
        //        if(args.Edge==Windows.Devices.Gpio.GpioPinEdge.FallingEdge)
        //        {
        //            isDoorbellJustPress = true;
        //            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
        //                await BeginFaceRecognization();
        //            });
        //        }
        //    }
        //}

        /// <summary>
        /// 人脸识别
        /// </summary>
        /// <returns></returns>
        private async Task BeginFaceRecognization()
        {
            if (camera!=null&&camera.IsInitialized()&&speech!=null)//检测摄像头是否初始化
            {
                await speech.PlayTTS(Constants.SpeechConstants.GreetingMessage);
                StorageFile imgFile = await camera.CapturePhoto();
                faceApi = new FaceApiHelper();
                FaceInfo faceInfo = await faceApi.FaceDetection(imgFile);
                if (faceInfo == null||userName==string.Empty)
                    return;
                string faceListId = EncriptHelper.ToMd5(userName);
                string memberName = await faceApi.FaceSimilarWithMemberName(faceInfo.faceId,faceListId);
                if(string.Empty!=memberName)//识别成功，有权进入
                {
                    UnlockDoor();
                    await ToastHelper.SendToast(Constants.ToastConstants.MemberComeBackNotification(memberName), userName);
                    await speech.PlayTTS(Constants.SpeechConstants.GeneralGreetigMessage(memberName));
                }
                else
                {
                    if(faceRecognizationCount==0)
                    {
                        timer_FaceRecognization.Start();
                    }
                    ++faceRecognizationCount;
                    if(faceRecognizationCount>=Constants.FaceConstants.MaxFaceRecognizationFailed
                        &&blobHelper!=null)//在5分钟之内识别错误5次，将上传照片
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            string imgName = camera.GenerateUserNameFileName(userName);
                            await blobHelper.uploadImage(imgName, imgFile);//发送图片到Blob Storage
                            await SendEmergencePictureUri(userName, imgName);//发送PictureUri到服务器
                            ++StrangerCounter;
                        });
                    }
                    await ToastHelper.SendToast(Constants.ToastConstants.VisitorNotRecognizedWarning, userName);
                    await speech.PlayTTS(Constants.SpeechConstants.VisitorNotRecognizedMessage);
                }
            }
            else
            {
                await speech.PlayTTS(Constants.SpeechConstants.NoCameraMessage);
            }

            isDoorbellJustPress = false;
        }

        private void UnlockDoor()
        {
            if(isGpioValuable)
            {
                gpioHelper.UnlockDoor();
            }
        }

        /// <summary>
        /// 向移动端推送火警通知
        /// </summary>
        /// <returns></returns>
        private async Task SendFireAlarm()
        {
            if(userName.Length>0)
            {
                //string queryString = string.Format("?content={0}&userName={1}",
                //Constants.NotificationConstants.FireWarining, userName);
                //HttpService http = new HttpService();
                //await http.SendGetRequest(notificationHost + queryString);
                await ToastHelper.SendToast(Constants.ToastConstants.FireWarining,userName);
            }
        }

        /// <summary>
        /// 生成Blob Storage上图片的Uri
        /// </summary>
        /// <param name="imgName">图片名</param>
        /// <returns>Blob storage上图片的Uri</returns>
        //public string GnerateEmergencePictureUri(string imgName)
        //{
        //    return "https://myblobsample.blob.core.windows.net/mycontainer/" + imgName;
        //}

        private async Task SendEmergencePictureUri(string userName,string pictureUri)
        {
            HttpService httpService = new HttpService();
            EmergencePictureInfo emergencePicture = new EmergencePictureInfo();
            emergencePicture.userName = userName;
            emergencePicture.pictureUri = pictureUri;
            string jsonContent = JsonHelper.ObjectToJson(emergencePicture);
            HttpResponseMessage response = await httpService.SendPostRequest(emergencePictureHost, jsonContent);
            if(response.StatusCode!=HttpStatusCode.Created)
            {
                Debug.WriteLine("EmergencePicture发送失败：" + response.Content);
            }
        }

        private async void mediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if(speech==null)
            {
                speech = new SpeechHelper(mediaElement);
                await speech.PlayTTS(Constants.SpeechConstants.InitialSpeechMessage);
            }
            else
            {
                mediaElement.AutoPlay = false;
            }
        }

        private async void cameraElement_Loaded(object sender, RoutedEventArgs e)
        {
            if(camera==null||!camera.IsInitialized())
            {
                camera = new WebCamHelper();
                await camera.InitializeCameraAsync();

                cameraElement.Source = camera.mediaCapture;

                if(cameraElement.Source!=null)
                {
                    await camera.StartCameraPreview();
                }
                else if(camera.IsInitialized())
                {
                    cameraElement.Source = camera.mediaCapture;
                    if(cameraElement.Source!=null)
                    {
                        await camera.StartCameraPreview();
                    }
                }
            }
        }
    }
}
