using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WebApiSample.Common;
using WebApiSample.Controls;
using WebApiSample.FaceRecognizatioin;
using WebApiSample.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiSample.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AddFaceToListGuide : Page
    {
        string UserName = string.Empty;

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public AddFaceToListGuide()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UserAccountService userAccount = new UserAccountService();
            PasswordCredential credential = userAccount.GetCredentialFromLocker();
            if(credential!=null)
            {
                UserName = credential.UserName;
            }

            base.OnNavigatedTo(e);
        }

        private async void btnAddFaceFromFile_Click(object sender, RoutedEventArgs e)
        {
            this.btnAddFaceFromFile.IsEnabled = false;
            this.btnAddFaceFromPhoto.IsEnabled = false;
            this.loading.IsActive = true;
            if(txtMemberName.Text.Length>0&&txtMemberName.Text.Length<31)
            {
                StorageFile imgFile = await ImageHelper.PickImageFile();
                if (imgFile != null&&UserName!=string.Empty)
                {
                    FaceApiHelper faceApi = new FaceApiHelper();
                    FaceApiHelper.FaceListStatus status = await faceApi.FaceListAddFace(imgFile,
                        EncriptHelper.ToMd5(UserName), txtMemberName.Text);
                    if(status==FaceApiHelper.FaceListStatus.success)
                    {
                        await new MessageBox("添加成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                        localSettings.Values[Constants.SettingName.IsUpdateFaceList] = true;
                        //NavMenuListView item = new NavMenuListView();
                        //item.SetSelectItem(0);
                        this.Frame.Navigate(typeof(Page1));
                    }
                    else
                    {
                        await new MessageBox("添加失败", MessageBox.NotifyType.CommonMessage).ShowAsync();
                    }
                }    
            }
            else
            {
                await new MessageBox("添加失败，成员名不可过长或为空", MessageBox.NotifyType.CommonMessage).ShowAsync();
            }
            this.btnAddFaceFromFile.IsEnabled = true;
            this.btnAddFaceFromPhoto.IsEnabled = true;
            this.loading.IsActive = false;
        }

        private async void btnAddFaceFromPhoto_Click(object sender, RoutedEventArgs e)
        {
            this.btnAddFaceFromFile.IsEnabled = false;
            this.btnAddFaceFromPhoto.IsEnabled = false;
            this.loading.IsActive = true;
            if (txtMemberName.Text.Length > 0 && txtMemberName.Text.Length < 31)
            {
                StorageFile imgFile = await ImageHelper.TakePhoto();
                if (imgFile != null && UserName != string.Empty)
                {
                    FaceApiHelper faceApi = new FaceApiHelper();
                    FaceApiHelper.FaceListStatus status = await faceApi.FaceListAddFace(imgFile,
                        EncriptHelper.ToMd5(UserName), txtMemberName.Text);
                    if (status == FaceApiHelper.FaceListStatus.success)
                    {
                        await new MessageBox("添加成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                        //NavMenuListView navMenu = new NavMenuListView();
                        //navMenu.SetSelectItem(0);
                        this.Frame.Navigate(typeof(Page1));
                    }
                    else
                    {
                        await new MessageBox("添加失败", MessageBox.NotifyType.CommonMessage).ShowAsync();
                    }
                }
            }
            else
            {
                await new MessageBox("添加失败，成员名不可过长或为空", MessageBox.NotifyType.CommonMessage).ShowAsync();
            }
            this.btnAddFaceFromFile.IsEnabled = true;
            this.btnAddFaceFromPhoto.IsEnabled = true;
            this.loading.IsActive = false;
        }

        private void txtMemberName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtMemberName.Text.Length>0)
            {
                this.btnAddFaceFromFile.IsEnabled = true;
                this.btnAddFaceFromPhoto.IsEnabled = true;
            }
        }
    }
}
