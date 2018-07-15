using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using WebApiDemo.HttpClientSample;
using WebApiUWPDemo.Common;
using WebApiUWPDemo.Model;
using Windows.Data.Json;
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

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace WebApiUWPDemo
{   
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        private Uri webUri;
        private string strWebUri = "http://mywebapidemo.azurewebsites.net/api/UserInfo";
        public MainPage()
        {
            this.InitializeComponent();

            Helper.CreateHttpClient(ref httpClient);
            cts = new CancellationTokenSource();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // If the navigation is external to the app do not clean up.
            if (NavigationMode.Forward == e.NavigationMode && e.Uri == null)
                return;

            Dispose();
        }


        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Helper.TryGetUri(strWebUri,out webUri))
                {
                    this.Loading.IsActive = true;
                    UserInfo model = new UserInfo();
                    model.UserName = this.txtUserName.Text;
                    model.Password = this.txtPassword.Text;
                    string json = JsonHelper.ObjectToJson(model);
                    IHttpContent httpContent = new HttpJsonContent(JsonValue.Parse(json));
                    HttpResponseMessage response = await httpClient.PostAsync(webUri,
                        httpContent).AsTask(cts.Token);
                    this.Loading.IsActive = false;
                    if(response.StatusCode==HttpStatusCode.Created)
                    {
                        await new MessageBox("创建成功！", MessageBox.NotifyType.CommonMessage).ShowAsync();
                    }
                }
                else
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                await new MessageBox("创建失败！"+ex.Message, MessageBox.NotifyType.CommonMessage).ShowAsync();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            cts.Dispose();

            //重新创建cts，以备重新发起请求
            cts = new CancellationTokenSource();
        }

        private void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Login));
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DownLoagImage));
        }

        private void btnLeftSample_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ButtonLeftSample));
        }

        private void btnFaceDetection_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FaceDetectionSample));
        }

        private void btnSynthesizeVoice_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SynthesizeVoiceSample));
        }

        private void btnEmotionDetect_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EmotionDetect));
        }

        private void btnSendInformationContent_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SendInformationContent));
        }
    }
}
