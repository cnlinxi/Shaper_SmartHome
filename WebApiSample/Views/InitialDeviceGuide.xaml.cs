using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WebApiSample.Common;
using WebApiSample.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
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
    public sealed partial class InitialDeviceGuide : Page
    {
        string userName;
        public InitialDeviceGuide()
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

        private void txtAuthCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtAuthCode.Text.Length>0)
            {
                this.btnStartInitialization.IsEnabled = true;
            }
        }

        private void hybtnUserAccount_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserAccount));
        }

        private async void btnStartInitialization_Click(object sender, RoutedEventArgs e)
        {
            if(this.txtAuthCode.Text.Length>0)
            {
                this.loading.IsActive = true;
                this.btnStartInitialization.IsEnabled = false;
                this.hybtnUserAccount.IsEnabled = false;
                InitialDeviceHelper initialDevice = new InitialDeviceHelper();
                await initialDevice.SendAuthCode(userName, txtAuthCode.Text);
                this.loading.IsActive = false;
                this.btnStartInitialization.IsEnabled = true;
                this.hybtnUserAccount.IsEnabled = true;
            }
        }
    }
}
