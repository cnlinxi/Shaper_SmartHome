using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiUWPDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowPicture : Page
    {
        string imageName = "";
        Image image=null;
        public ShowPicture()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is Dictionary<string,Image>)
            {
                Dictionary<string, Image> data = e.Parameter as Dictionary<string, Image>;
                KeyValuePair<string, Image> value = data.FirstOrDefault();
                imageName = value.Key;
                image = value.Value;
                this.picture.Source = image.Source;
                this.tbMessage.Text ="图片名称："+ imageName;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {


            base.OnNavigatedFrom(e);
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("是否取消上传？", "消息");
            dialog.Commands.Add(new UICommand("确定", 
                new UICommandInvokedHandler(CommandInvokedHandler)));
            dialog.Commands.Add(new UICommand("取消",
                new UICommandInvokedHandler(CommandInvokedHandler)));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            //switch(command.Label)
            //{
            //    case "确定":
            //        Dictionary<string, Image> data = new Dictionary<string, Image>();
            //        data.Add(imageName,image);
            //        this.Frame.Navigate(typeof(ButtonLeftSample), data);
            //        break;
            //    case "取消":
            //        break;
            //    default:
            //        break;
            //}
            if(command.Label.Equals("确定"))
            {
                Dictionary<string, Image> data = new Dictionary<string, Image>();
                data.Add(imageName, image);
                this.Frame.Navigate(typeof(ButtonLeftSample), data);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ButtonLeftSample),null);
        }
    }
}
