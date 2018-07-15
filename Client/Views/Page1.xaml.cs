using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using WebApiSample.Common;
using WebApiSample.Controls;
using WebApiSample.FaceRecognizatioin;
using WebApiSample.Helpers;
using WebApiSample.Model;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace WebApiSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page1 : Page
    {
        string userName = string.Empty;

        const string storageAccountName = "myblobsample";
        const string storageAccountKey = "JFyiGqG4Av5U7yc0RYIkvyHDW7taz5hl0TPdFLssZaUejbVKdkJvfhUEXNkfVU2usnEPzBc2MdzrCbvURHzrrQ==";
        const string containerName = "mycontainer";

        const string CommandHost = "http://mywebapidemo.azurewebsites.net/api/TimingCommand";
        const string NotificationHost = "http://mywebapidemo.azurewebsites.net/api/Notification";
        const string TimingCommandHost = "http://mywebapidemo.azurewebsites.net/api/TimingCommand";

        const string CmdOn = "On";
        //本地数据容器，重装可同步
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        //多平台同步容器
        private StorageFolder roamdingFolder = ApplicationData.Current.RoamingFolder;
        ApplicationDataContainer roamdingSettings = ApplicationData.Current.RoamingSettings;

        public Page1()
        {
            this.InitializeComponent();

            UserAccountService userAccount = new UserAccountService();
            userName = userAccount.GetUserNameFromLocker();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.loading.IsActive=true;
            FaceListInit();
            AvatorInit();
            NotificationInit();
            this.loading.IsActive = false;

            base.OnNavigatedTo(e);
        }

        private async void FaceListInit()
        {
            //if (localSettings.Values.ContainsKey(Constants.SettingName.IsUpdateFaceList)
            //    && (!Convert.ToBoolean(localSettings.Values[Constants.SettingName.IsUpdateFaceList])))
            //{
            //    List<FaceListNameInfo> lstfaceListName = 
            //        (List<FaceListNameInfo>)localSettings.Values[Constants.SettingName.FaceList];
            //    return;
            //}
            if (userName != string.Empty)
            {
                FaceApiHelper faceApi = new FaceApiHelper();
                List<FaceListNameInfo> lstMemberName =
                    await faceApi.GetFaceListUserName(EncriptHelper.ToMd5(userName));
                //List<FaceListNameInfo> lstfaceListName = new List<FaceListNameInfo>();
                //foreach (var obj in lstMemberName)
                //{
                //    FaceListNameInfo faceName = new FaceListNameInfo();
                //    faceName.Name = obj.Name;
                //    lstfaceListName.Add(faceName);
                //}
                lvFaceListName.ItemsSource = lstMemberName;
                //localSettings.Values[Constants.SettingName.FaceList] = lstfaceListName;
                //localSettings.Values[Constants.SettingName.IsUpdateFaceList] = false;
            }
        }

        private async void toggleOpenLEDInit()
        {
            try
            {
                HttpService http = new HttpService();
                string queryString = string.Format("?userName={0}", userName);
                string response = await http.SendGetRequest(TimingCommandHost + queryString);
                if (response.Length <= 0)
                    return;
                JObject jsonObject = JObject.Parse(response);
                if (jsonObject["command"].ToString() == CmdOn)
                    toggleLed.IsOn = true;
            }
            catch { }
        }

        private async void AvatorInit()
        {
            if(userName!=string.Empty)
            {
                string imgName = EncriptHelper.ToMd5(userName) + ".jpg";
                BitmapImage bitmap = await GetAvator(imgName);
                if(bitmap!=null)
                {
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = bitmap;
                    elAvator.Fill = imgBrush;
                }
            }
            else
            {
                ImageBrush defaultAvatorImg = new ImageBrush();
                BitmapImage defaultAvatorBitmap = new BitmapImage(new Uri("ms-appx:///Assets/userAvatorDefault.png"));
                defaultAvatorImg.ImageSource = defaultAvatorBitmap;
                elAvator.Fill = defaultAvatorImg;
            }
        }

        private async void NotificationInit()
        {
            try
            {
                if(userName!=string.Empty)
                {
                    PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                    if (roamdingSettings.Values["channel"]==null)
                    {
                        NotificationInfo channelModel = new NotificationInfo();
                        channelModel.userName = userName;
                        channelModel.channelUri = channel.Uri;
                        channelModel.expirationTime = channel.ExpirationTime.DateTime;
                        string strJson = JsonHelper.ObjectToJson(channelModel);
                        HttpService httpService = new HttpService();
                        await httpService.SendPostRequest(NotificationHost, strJson);
                        roamdingSettings.Values["channel"] = channel.Uri;
                    }
                    else if (roamdingSettings.Values["channel"].ToString() != channel.Uri)
                    {
                        NotificationInfo channelModel = new NotificationInfo();
                        channelModel.userName = userName;
                        channelModel.channelUri = channel.Uri;
                        channelModel.expirationTime = channel.ExpirationTime.DateTime;
                        string strJson = JsonHelper.ObjectToJson(channelModel);
                        HttpService http = new HttpService();
                        string queryString = string.Format("?userName={0}", userName);
                        await http.SendPutRequest(NotificationHost + queryString, strJson);
                        roamdingSettings.Values["channel"] = channel.Uri;
                    }
                    channel.PushNotificationReceived += Channel_PushNotificationReceived;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        {
            String notificationContent = String.Empty;

            switch (e.NotificationType)
            {
                case PushNotificationType.Badge:
                    notificationContent = e.BadgeNotification.Content.GetXml();
                    break;

                case PushNotificationType.Tile:
                    notificationContent = e.TileNotification.Content.GetXml();
                    break;

                case PushNotificationType.Toast:
                    notificationContent = e.ToastNotification.Content.GetXml();
                    UpdateUIFromToast(notificationContent);
                    break;

                case PushNotificationType.Raw:
                    notificationContent = e.RawNotification.Content;
                    break;
            }
        }

        private async void UpdateUIFromToast(string notificationContent)
        {
            string content = ToastHelper.ParseXml(notificationContent);
            if (content == string.Empty)
                return;
            if(content==Constants.ToastConstants.FireAlarm)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.btnHomeStatus.Content = "异常";
                    this.btnHomeStatus.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                });
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
             {
                 tbLastActivity.Text = content;
             });
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarButton btn = sender as AppBarButton;
            switch(btn.Label)
            {
                case "Add":
                    this.Frame.Navigate(typeof(AddFaceToListGuide));
                    break;
                case "Settings":
                    this.Frame.Navigate(typeof(SettingsPage));
                    break;
                case "初始化设备":
                    this.Frame.Navigate(typeof(InitialDeviceGuide));
                    break;
                case "控制中心":
                    this.Frame.Navigate(typeof(Page2));
                    break;
                default:
                    break;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            const string ChangeAvator = "ChangeAvator";
            const string Logout = "Logout";
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            if(item.Tag.ToString()==ChangeAvator)
            {
                if(userName!=string.Empty)
                {
                    this.loading.IsActive = true;
                    this.btnChangeAccount.IsEnabled = false;
                    string imgName = EncriptHelper.ToMd5(userName) + ".jpg";
                    await UploadAvator(imgName);
                    BitmapImage bitmap = await GetAvator(imgName);
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = bitmap;
                    elAvator.Fill = imgBrush;
                    this.loading.IsActive = false;
                    this.btnChangeAccount.IsEnabled = true;
                }
                else
                {
                        await new MessageBox("请先登录", MessageBox.NotifyType.CommonMessage).ShowAsync();
                }
            }else if(item.Tag.ToString()==Logout)
            {
                UserAccountService userAccount = new UserAccountService();
                userAccount.Loginout();
                AvatorInit();
            }
        }

        public async Task UploadAvator(string strImageName)
        {
           StorageFile img =  await ImageHelper.PickImageFile();
            if (img == null)
                return;
            BlobHelper blob = new BlobHelper();
            blob.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
            await blob.uploadImage(strImageName, img);
            roamdingSettings.Values["isAvatarModify"] = true;
        }

        public async Task<BitmapImage> GetAvator(string strImageName)
        {
            if (roamdingSettings.Values["isAvatarModify"]!=null &&
                Convert.ToBoolean(roamdingSettings.Values["isAvatarModify"]))
            {
                BlobHelper blob = new BlobHelper();
                blob.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
                try
                {
                    await blob.downloadFileAndStorage(strImageName);
                    roamdingSettings.Values["isAvatarModify"] = false;

                    StorageFile file = await localFolder.GetFileAsync(strImageName);
                    if (file != null)
                    {
                        Stream stream = await file.OpenStreamForReadAsync();
                        int length = Convert.ToInt32(stream.Length);
                        byte[] bytes = new byte[length];
                        await stream.ReadAsync(bytes, 0, length);
                        IRandomAccessStream randomStream = new InMemoryRandomAccessStream();
                        DataWriter dataWriter = new DataWriter(randomStream.GetOutputStreamAt(0));
                        dataWriter.WriteBytes(bytes);
                        await dataWriter.StoreAsync();

                        BitmapImage image = new BitmapImage();
                        await image.SetSourceAsync(randomStream);
                        return image;
                    }
                }
                catch
                {

                }
            }
            else if(roamdingSettings.Values["isAvatarModify"] != null)
            {
                StorageFile file = await localFolder.GetFileAsync(strImageName);
                if (file != null)
                {
                    Stream stream = await file.OpenStreamForReadAsync();
                    int length = Convert.ToInt32(stream.Length);
                    byte[] bytes = new byte[length];
                    await stream.ReadAsync(bytes, 0, length);
                    IRandomAccessStream randomStream = new InMemoryRandomAccessStream();
                    DataWriter dataWriter = new DataWriter(randomStream.GetOutputStreamAt(0));
                    dataWriter.WriteBytes(bytes);
                    await dataWriter.StoreAsync();

                    BitmapImage image = new BitmapImage();
                    await image.SetSourceAsync(randomStream);
                    return image;
                }
            }

            return null;
        }

        enum AccoutStatus
        {
            notLogin,success,failed
        }

        private void FamilyStatue_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

        }

        private void btnHomeStatus_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //NavMenuListView navMenu = new NavMenuListView();
            //navMenu.SetSelectItem(1);
            this.Frame.Navigate(typeof(Page2));
        }

        private async void toggleLed_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSwitch toggleLed = sender as ToggleSwitch;
            loading.IsActive = true;

            if (toggleLed.IsOn)
            {
                await SendCommand(Constants.Command.CmdOn);
            }
            else
            {
                await SendCommand(Constants.Command.CmdOff);
            }

            loading.IsActive = false;
        }

        private async Task SendCommand(string commandContent)
        {
            if (commandContent != string.Empty && userName != string.Empty)
            {
                string queryString = string.Format("?userName={0}", userName);
                string url = CommandHost + queryString;
                TimingCommandInfo timingCommand = new TimingCommandInfo();
                timingCommand.userName = userName;
                timingCommand.command = commandContent;
                string jsonContent = JsonHelper.ObjectToJson(timingCommand);
                HttpService http = new HttpService();
                await http.SendPutRequest(url, jsonContent);
            }
        }

        private async void borderAddMember_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ContentDialogResult dialogResult =
                await new MessageBox("是否添加新的成员？", MessageBox.NotifyType.CommonMessage).ShowAsync();
            if(dialogResult==ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(Page3));
            }
        }

        private async void lvFaceListName_ItemClick(object sender, ItemClickEventArgs e)
        {
            FaceListNameInfo nameInfo = e.ClickedItem as FaceListNameInfo;
            ContentDialogResult dialogResult =
                await new MessageBox("是否删除以下的成员？删除后将失去入内的权限", 
                MessageBox.NotifyType.DeleteFaceFromListMessage, nameInfo.Name).ShowAsync();
            if(dialogResult==ContentDialogResult.Primary
                &&userName!=string.Empty)
            {
                FaceApiHelper faceApi = new FaceApiHelper();
                await faceApi.DeleteFaceFromFaceList(EncriptHelper.ToMd5(userName), nameInfo.FaceId);
                FaceListInit();//更新成员列表
            }
        }

        //private async void lvFaceListName_ItemClick_1(object sender, ItemClickEventArgs e)
        //{
        //    FaceListNameInfo nameInfo = e.ClickedItem as FaceListNameInfo;
        //    ContentDialogResult dialogResult =
        //        await new MessageBox("是否删除以下的成员？删除后将失去入内的权限",
        //        MessageBox.NotifyType.DeleteFaceFromListMessage, nameInfo.Name).ShowAsync();
        //    if (dialogResult == ContentDialogResult.Primary
        //        && userName != string.Empty)
        //    {
        //        FaceApiHelper faceApi = new FaceApiHelper();
        //        await faceApi.DeleteFaceFromFaceList(EncriptHelper.ToMd5(userName), nameInfo.FaceId);
        //    }
        //}
    }
}
