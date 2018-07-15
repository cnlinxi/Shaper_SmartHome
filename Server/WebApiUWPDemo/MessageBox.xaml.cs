using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace WebApiUWPDemo
{
    public sealed partial class MessageBox : ContentDialog
    {
        public MessageBox(string content,NotifyType type)
        {
            this.InitializeComponent();

            if(type==NotifyType.CommonMessage)
            {
                TextBlock tb = new TextBlock();
                tb.Text = content;
                this.rootFrame.Children.Add(tb);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public  enum NotifyType
        {
            CommonMessage
        }
    }
}
