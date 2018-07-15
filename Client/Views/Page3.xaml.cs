using System;
using WebApiSample.Common;
using WebApiSample.Controls;
using WebApiSample.FaceRecognizatioin;
using WebApiSample.Helpers;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WebApiSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page3 : Page
    {
        string userName = string.Empty;

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public Page3()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UserAccountService userAccount = new UserAccountService();
            PasswordCredential credential = userAccount.GetCredentialFromLocker();
            if (credential != null)
            {
                userName = credential.UserName;
            }

            base.OnNavigatedTo(e);
        }

        private async void btnAddFaceFromFile_Click(object sender, RoutedEventArgs e)
        {
            this.btnAddFaceFromFile.IsEnabled = false;
            this.btnAddFaceFromPhoto.IsEnabled = false;
            this.loading.IsActive = true;
            if (txtMemberName.Text.Length > 0 && txtMemberName.Text.Length < 31)
            {
                StorageFile imgFile = await ImageHelper.PickImageFile();
                if (imgFile != null && userName != string.Empty)
                {
                    FaceApiHelper faceApi = new FaceApiHelper();
                    FaceApiHelper.FaceListStatus status = await faceApi.FaceListAddFace(imgFile,
                        EncriptHelper.ToMd5(userName), txtMemberName.Text);
                    if (status == FaceApiHelper.FaceListStatus.success)
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
                if (imgFile != null && userName != string.Empty)
                {
                    FaceApiHelper faceApi = new FaceApiHelper();
                    FaceApiHelper.FaceListStatus status = await faceApi.FaceListAddFace(imgFile,
                        EncriptHelper.ToMd5(userName), txtMemberName.Text);
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
            if (txtMemberName.Text.Length > 0)
            {
                this.btnAddFaceFromFile.IsEnabled = true;
                this.btnAddFaceFromPhoto.IsEnabled = true;
            }
        }

        private async void btnStartInitialization_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtAuthCode.Text.Length > 0)
            {
                this.loading.IsActive = true;
                this.btnStartInitialization.IsEnabled = false;
                this.hybtnUserAccount.IsEnabled = false;
                InitialDeviceHelper initialDevice = new InitialDeviceHelper();
                await initialDevice.SendAuthCode(userName, txtAuthCode.Text);
                this.loading.IsActive = false;
                this.btnStartInitialization.IsEnabled = true;
                this.hybtnUserAccount.IsEnabled = true;
                btnStartInitialization.IsEnabled = false;
            }
        }

        private void txtAuthCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtAuthCode.Text.Length > 0)
            {
                this.btnStartInitialization.IsEnabled = true;
            }
        }

        private void hybtnUserAccount_Click(object sender, RoutedEventArgs e)
        {
            //NavMenuListView navMenu = new NavMenuListView();//debug:选中4号按钮（用户中心）
            //navMenu.SetSelectItem(4);
            this.Frame.Navigate(typeof(UserAccount));
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
