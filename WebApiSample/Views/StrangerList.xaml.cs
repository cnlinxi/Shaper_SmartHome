using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WebApiSample.Controls;
using WebApiSample.Helpers;
using WebApiSample.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiSample.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class StrangerList : Page
    {
        const string storageAccountName = "myblobsample";
        const string storageAccountKey = "JFyiGqG4Av5U7yc0RYIkvyHDW7taz5hl0TPdFLssZaUejbVKdkJvfhUEXNkfVU2usnEPzBc2MdzrCbvURHzrrQ==";
        const string containerName = "mycontainer";

        const string StrangerListHost = "http://mywebapidemo.azurewebsites.net/api/EmergencyPicture";

        private BlobHelper blob;

        StrangerInfo stranger;
        List<StrangerInfo> lstStranger;

        string userName = string.Empty;

        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public StrangerList()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            UserAccountService userAccount = new UserAccountService();
            userName = userAccount.GetUserNameFromLocker();
            if(userName==string.Empty)
            {
                await new MessageBox("你尚未登录！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                //NavMenuListView navMenu = new NavMenuListView();
                //navMenu.SetSelectItem(4);
                this.Frame.Navigate(typeof(UserAccount));
            }
            this.loading.IsActive = true;
            blob = new BlobHelper();
            blob.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
            await GetStrangerList();//获取此用户的所有的被拒访客列表
            if(lstStranger!=null)
                this.listViewStranger.ItemsSource = lstStranger;
            this.loading.IsActive = false;
            base.OnNavigatedTo(e);
        }

        private async Task GetStrangerList()
        {
            try
            {
                lstStranger = new List<StrangerInfo>();
                HttpService http = new HttpService();
                string queryString = string.Format("?userName={0}", userName);
                string response = await http.SendGetRequest(StrangerListHost + queryString);
                if (response.Length <= 0)
                    return;
                JArray jsonArray = JArray.Parse(response);
                foreach(var jObject in jsonArray)
                {
                    StrangerInfo item = new StrangerInfo();
                    item.userName = jObject["userName"].ToString();
                    //远程服务器与本地有时差，落后本地8小时
                    DateTime localAddTime = (DateTime)jObject["addTime"] + new TimeSpan(8, 0, 0);
                    item.addTime = localAddTime.ToString();
                    item.pictureUri = jObject["pictureUri"].ToString();
                    lstStranger.Add(item);
                }
            }
            catch { }
        }

        private async void listViewStranger_ItemClick(object sender, ItemClickEventArgs e)
        {
            tbTip.Visibility = Visibility.Collapsed;
            if (blob == null)
                return;
            this.loading.IsActive = true;
            stranger = e.ClickedItem as StrangerInfo;

            if (!localSettings.Values.ContainsKey(stranger.pictureUri))
            {
                await blob.downloadFileAndStorage(stranger.pictureUri);
            }
            try
            {
                StorageFile file = await localFolder.GetFileAsync(stranger.pictureUri);
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
                    localSettings.Values[stranger.pictureUri] = true;

                    BitmapImage bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(randomStream);
                    this.image.Source = bitmap;
                }
            }
            catch
            {
                //Debug.WriteLine(ex.Message);
            }
            this.loading.IsActive = false;
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton btn = sender as AppBarButton;
            if(btn.Label=="Save")
            {
                SaveStrangerPicture();
            }
            if(btn.Label=="Delete")
            {
                ContentDialogResult dialogResult =
                    await new MessageBox("是否需要从服务器删除此被挡访客的图片？删除后将无法恢复", 
                    MessageBox.NotifyType.CommonMessage).ShowAsync();
                if(dialogResult==ContentDialogResult.Primary)
                {
                    DeleteStrangerPicture();
                }
            }
        }

        private async void SaveStrangerPicture()
        {
            if (stranger == null)
                return;
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Picture", new List<string>() { ".jpg", ".jpeg",".png"  });
            savePicker.SuggestedFileName = "New Picture";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            StorageFile fileToSave = await localFolder.GetFileAsync(stranger.pictureUri);
            if (file != null&&fileToSave !=null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                Stream stream = await fileToSave.OpenStreamForReadAsync();
                int length = Convert.ToInt32(stream.Length);
                byte[] bytes = new byte[length];
                await stream.ReadAsync(bytes, 0, length);
                await FileIO.WriteBytesAsync(file, bytes);
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
            }
        }

        private async void DeleteStrangerPicture()
        {
            if (stranger == null)
                return;
            this.loading.IsActive = true;
            HttpService http = new HttpService();
            string queryString = string.Format("?userName={0}&pictureName={1}", 
                userName, stranger.pictureUri);
            HttpResponseMessage response = await http.SendDeleteRequest(StrangerListHost + queryString);

            if(response.StatusCode==HttpStatusCode.Ok)//删除成功后，刷新列表
            {
                await GetStrangerList();//获取此用户的所有的被拒访客列表
                if (lstStranger != null)
                    this.listViewStranger.ItemsSource = lstStranger;
            }
            this.loading.IsActive = false;
        }
    }
}
