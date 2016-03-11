using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WebApiSample.Controls;
using WebApiSample.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Popups;
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
    public sealed partial class UserAccount : Page
    {
        private string resourceName = "WebApiSample";
        public UserAccount()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            UserAccountService userAccount = new UserAccountService();
            var loginCredential = userAccount.GetCredentialFromLocker();
            if(loginCredential!=null)
            {
                MessageDialog messageDialog = new MessageDialog("发现已经登录的账户，是否重新登录？这将导致原账号被清除！", "消息");
                messageDialog.Commands.Add(new UICommand("确定", new UICommandInvokedHandler(UICommandHandler)));
                messageDialog.Commands.Add(new UICommand("取消", new UICommandInvokedHandler(UICommandHandler)));
                await messageDialog.ShowAsync();
            }

            base.OnNavigatedTo(e);
        }

        private void UICommandHandler(IUICommand command)
        {
            if(command.Label== "取消")
            {
                //NavMenuListView navMenu = new NavMenuListView();
                //navMenu.SetSelectItem(0);
                this.Frame.Navigate(typeof(Page1));
            }
            if(command.Label=="确定")
            {
                UserAccountService userAccount = new UserAccountService();
                var loginCredential = userAccount.GetCredentialFromLocker();
                if(loginCredential!=null)
                {
                    userAccount.Loginout();
                }
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if(txtAccount.Text.Length>0&&txtPassword.Password.Length>0)
            {
                this.btnLogin.IsEnabled = false;
                this.btnRegister.IsEnabled = false;
                this.loading.IsActive = true;
                UserAccountService userAccount = new UserAccountService();
                bool isSuceess = await userAccount.Login(txtAccount.Text, txtPassword.Password);
                if (isSuceess)
                {
                    await new MessageBox("登录成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                    if((bool)chkSaveUser.IsChecked)
                    {
                        var vault = new PasswordVault();
                        vault.Add(new PasswordCredential(resourceName, txtAccount.Text, txtPassword.Password));
                    }
                }
                else
                {
                    await new MessageBox("登录失败，请重试", MessageBox.NotifyType.CommonMessage).ShowAsync();
                }
                this.btnLogin.IsEnabled = true;
                this.btnRegister.IsEnabled = true;
                this.loading.IsActive = false;
            }
            else
            {
                await new MessageBox("用户名或者密码不可为空！", MessageBox.NotifyType.CommonMessage).ShowAsync();
            }
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
           if(txtAccount_reg.Text.Length>0&&txtPassword_reg.Password.Length>0)
            {
                this.btnLogin.IsEnabled = false;
                this.btnRegister.IsEnabled = false;
                this.loading.IsActive = true;
                UserAccountService userAccount = new UserAccountService();
                UserAccountService.RegisterStaus status = await userAccount.Register(txtAccount_reg.Text, txtPassword_reg.Password);
                if (status == UserAccountService.RegisterStaus.ConflictUserName)
                {
                    await new MessageBox("已占用的用户名，重更换用户名重新注册", MessageBox.NotifyType.CommonMessage).ShowAsync();
                }
                else if (status == UserAccountService.RegisterStaus.Success)
                {
                    await new MessageBox("注册成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                    if((bool)chkSaveUser_reg.IsChecked)
                    {
                        var vault = new PasswordVault();
                        vault.Add(new PasswordCredential(resourceName, txtAccount.Text, txtPassword.Password));
                    }
                }
                else if (status == UserAccountService.RegisterStaus.Failed)
                {
                    await new MessageBox("注册失败！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                }
                this.btnLogin.IsEnabled = true;
                this.btnRegister.IsEnabled = true;
                this.loading.IsActive = false;
            }
           else
            {
                await new MessageBox("用户名或密码不可为空！", MessageBox.NotifyType.CommonMessage).ShowAsync();
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNavToRegister_Click(object sender, RoutedEventArgs e)
        {
            if (this.rootPivot.SelectedIndex > -1)
                this.rootPivot.SelectedIndex = 1;
        }

        private void btnNavToLogin_Click(object sender, RoutedEventArgs e)
        {
            if (this.rootPivot.SelectedIndex > 0)
                this.rootPivot.SelectedIndex = 0;
        }
    }
}
