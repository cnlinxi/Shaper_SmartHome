using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WebApiUWPDemo.Common;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiUWPDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownLoagImage : Page
    {
        private double screenWidth;
        private double controlsWidth;
        private static int imageCounter = 0;
        const int imageCount = 5;

        const string storageAccountName = "myblobsample";
        const string storageAccountKey = "JFyiGqG4Av5U7yc0RYIkvyHDW7taz5hl0TPdFLssZaUejbVKdkJvfhUEXNkfVU2usnEPzBc2MdzrCbvURHzrrQ==";
        const string containerName = "mycontainer";

        const string strImageName = "123.jpg";

        //本地数据容器，重装可同步
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        //多平台同步容器
        private StorageFolder roamdingFolder = ApplicationData.Current.RoamingFolder;
        ApplicationDataContainer roamdingSettings = ApplicationData.Current.RoamingSettings;

        public DownLoagImage()
        {
            this.InitializeComponent();

            screenWidth =this.rootFrame.ActualWidth;
            controlsWidth = (screenWidth - 12) / 5;
            if(controlsWidth>0)
            {
                this.btn.Height = this.btn.Width = controlsWidth;
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            while(imageCounter<imageCount&& controlsWidth>0)
            {
                Button btn = new Button();
                btn.Content = "Click me";
                btn.Margin = new Thickness(10, 5, 0, 0);
                btn.Width = btn.Height = controlsWidth;
                btn.Click += btn_Click;
                rootFrame.Children.Add(btn);
                imageCounter++;
            }
        }

        private async void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    this.Loading.IsActive = true;
                    this.btnUploadImage.IsEnabled = false;
                    BlobMethods blobTools = new BlobMethods();
                    blobTools.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
                    await blobTools.uploadImage(strImageName, file);
                    this.Loading.IsActive = false;
                    this.btnUploadImage.IsEnabled = true;
                }
            }
            catch(Exception ex)
            {

            }
        }

        private async void btnDownloadImage_Click(object sender, RoutedEventArgs e)
        {
            this.btnDownloadImage.IsEnabled = false;
            this.Loading.IsActive = true;
            BitmapImage bitmap = new BitmapImage();
            BlobMethods blobTools = new BlobMethods();
            blobTools.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
            IRandomAccessStream stream = await blobTools.downloadFile(strImageName);
           if(stream==null)
            {
                new MessageBox("网络故障", MessageBox.NotifyType.CommonMessage);
            }
           else
            {
                bitmap.SetSource(stream);
                this.image.Source = bitmap;
                this.btnDownloadImage.IsEnabled = true;
                this.Loading.IsActive = false;
            }
        }

        private async void btnDownloadFile_Click(object sender, RoutedEventArgs e)
        {
            this.btnDownloadFile.IsEnabled = false;
            this.Loading.IsActive = true;
            BlobMethods blobTools = new BlobMethods();
            blobTools.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Picture", new List<string>() { ".png",".jpg",".jpeg" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Picture";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await blobTools.downloadFile(strImageName);
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                //if (status == Windows.Storage.Provider.FileUpdateStatus.Complete) { }
            }
            else
            {
                
            }
            this.btnDownloadFile.IsEnabled = true;
            this.Loading.IsActive = false;
        }

        private async void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            BlobMethods blob = new BlobMethods();
            blob.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
            if (roamdingSettings.Values["isAvatarModify"] != null &&
                Convert.ToBoolean(roamdingSettings.Values["isAvatarModify"]))
            {
                try
                { 
                    await blob.downloadFileAndStorage(strImageName);
                    roamdingSettings.Values["isAvatarModify"] = false;
                }
                catch
                {

                }
            }
            else if ((roamdingSettings.Values["isAvatarModify"] == null))
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation =
                    Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                try
                {
                    StorageFile file = await picker.PickSingleFileAsync();
                    if (file != null)
                    {
                        this.Loading.IsActive = true;
                        this.btnUploadImage.IsEnabled = false;
                        BlobMethods blobTools = new BlobMethods();
                        blobTools.RunatAppStartUp(storageAccountName, storageAccountKey, containerName);
                        await blobTools.uploadImage(strImageName, file);
                        this.Loading.IsActive = false;
                        this.btnUploadImage.IsEnabled = true;
                    }
                }
                catch
                {

                }
            }
            else
            {
                StorageFile file = await localFolder.GetFileAsync(strImageName);
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
                this.image.Source = image;  
            }
        }
    }
}
