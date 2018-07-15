using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WebApiSample.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiSample.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FamilyStatusDetails : Page
    {
        public FamilyStatusDetails()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FamilyStatusSumInfo familySum=new FamilyStatusSumInfo();
            if (e.Parameter is FamilyStatusSumInfo)
            {
                 familySum = e.Parameter as FamilyStatusSumInfo;
            }

            if (familySum == null)
                return;

            if(familySum.FireCounter==0&&familySum.StrangerCounter==0)
            {
                this.PieChart.Visibility = Visibility.Collapsed;
                //没有数据，显示图片代替
            }
            else
            {
                List<NameValueItem> items = new List<NameValueItem>();
                NameValueItem itemFire = new NameValueItem();
                itemFire.Name = "发生火警";
                itemFire.Value = familySum.FireCounter;
                items.Add(itemFire);
                NameValueItem itemStranger = new NameValueItem();
                itemStranger.Name = "被挡访客";
                itemStranger.Value = familySum.StrangerCounter;
                items.Add(itemStranger);

                ((PieSeries)this.PieChart.Series[0]).ItemsSource = items;
            }

            base.OnNavigatedTo(e);
        }
    }
}
