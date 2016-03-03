using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WebApiSample.Common;
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

        const string notificationHost = "http://mywebapidemo.azurewebsites.net/api/Notification";

        //本地数据容器，重装可同步
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
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
            FaceListInit();
            AvatorInit();
            NotificationInit();

            base.OnNavigatedTo(e);
        }

        private async void FaceListInit()
        {
            if (userName != string.Empty)
            {
                FaceApiHelper faceApi = new FaceApiHelper();
                List<string> lstMemberName =
                    await faceApi.GetFaceListUserName(EncriptHelper.ToMd5(userName));
                List<FaceListNameInfo> lstfaceListName = new List<FaceListNameInfo>();
                foreach (var obj in lstMemberName)
                {
                    FaceListNameInfo faceName = new FaceListNameInfo();
                    faceName.Name = obj;
                    lstfaceListName.Add(faceName);
                }
                lvFaceListName.ItemsSource = lstfaceListName;
            }
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
                    if (roamdingSettings.Values["channel"]==null
                        || roamdingSettings.Values["channel"].ToString() != channel.Uri)
                    {
                        NotificationInfo channelModel = new NotificationInfo();
                        channelModel.userName = userName;
                        channelModel.channelUri = channel.Uri;
                        channelModel.expirationTime = channel.ExpirationTime.DateTime;
                        string strJson = JsonHelper.ObjectToJson(channelModel);
                        HttpService httpService = new HttpService();
                        await httpService.SendPostRequest(notificationHost, strJson);
                        roamdingSettings.Values["channel"] = channel.Uri;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarButton btn = sender as AppBarButton;
            switch(btn.Label)
            {
                case "Add":
                    this.Frame.Navigate(typeof(AddFaceToListGuide));
                    break;
                case "menu1":
                    UserAccountService user = new UserAccountService();
                    user.ClearAllCredentialFromLocker();
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
    }
}
