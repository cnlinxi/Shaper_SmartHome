using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Xml;
using WebApiUWPDemo.Common;
using WebApiUWPDemo.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiUWPDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Login : Page
    {
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        private Uri uri;
        public Login()
        {
            this.InitializeComponent();

            Helper.CreateHttpClient(ref httpClient);
            cts = new CancellationTokenSource();
        }



        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string userName = this.txtUserName.Text;
            string webUri = String.Format("http://mywebapidemo.azurewebsites.net/api/UserInfo?userName={0}", userName);
            if(!Helper.TryGetUri(webUri,out uri))
            {
                return;
            }
            try
            {
                string response = await httpClient.GetStringAsync(uri).AsTask(cts.Token);
                UserInfo model = new UserInfo();
                model= JsonHelper.JsonToObject(response, model) as UserInfo;
                
                if (model.Password== EncriptHelper.ToMd5(this.txtPassword.Text))
                {
                    await new MessageBox("登录成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                }
                else
                {
                    await new MessageBox("无效的用户名或密码", MessageBox.NotifyType.CommonMessage).ShowAsync();
                }
            }
            catch(OperationCanceledException cancelEx)
            {

            }
            catch(Exception ex)
            {

            }
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(response);
            //XmlNode user = doc.ChildNodes[1];
            //if(user.Name== "UserDetails.UserModel")
            //{
            //    UserInfo model = new UserInfo();
            //    foreach(XmlNode node in user.ChildNodes)
            //    {
            //        if(node.Name=="Password")
            //        {
            //            string innerText = node.InnerText;
            //            string password = EncriptHelper.ToMd5(this.txtPassword.Text);
            //            if(innerText==password)
            //            {
            //                await new MessageBox("登录成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    await new MessageBox("无效的用户名或密码", MessageBox.NotifyType.CommonMessage).ShowAsync();
            //}

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
