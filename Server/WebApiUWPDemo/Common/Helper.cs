using SDKSample.HttpClientSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace WebApiUWPDemo
{
    internal static class Helper
    {
        /// <summary>
        /// 创建HttpClient对象
        /// </summary>
        /// <param name="httpClient">要实例化的httpClient对象</param>
        internal static void CreateHttpClient(ref HttpClient httpClient)
        {
            if(httpClient!=null)
            {
                httpClient.Dispose();
            }
            //添加过滤器
            IHttpFilter filter = new HttpBaseProtocolFilter();
            filter = new PlugInFilter(filter);
            httpClient = new HttpClient(filter);

            //添加User-Agent
            httpClient.DefaultRequestHeaders.UserAgent.Add(new Windows.Web.Http.Headers.HttpProductInfoHeaderValue("mySample","v1"));
        }

        /// <summary>
        /// 从字符串获取Uri
        /// </summary>
        /// <param name="uriString">Uri字符串</param>
        /// <param name="uri">Uri</param>
        /// <returns>成功返回true,否则返回false</returns>
        internal static bool TryGetUri(string uriString, out Uri uri)
        {
            // Note that this app has both "Internet (Client)" and "Home and Work Networking" capabilities set,
            // since the user may provide URIs for servers located on the internet or intranet. If apps only
            // communicate with servers on the internet, only the "Internet (Client)" capability should be set.
            // Similarly if an app is only intended to communicate on the intranet, only the "Home and Work
            // Networking" capability should be set.
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out uri))
            {
                return false;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return false;
            }

            return true;
        }
    }
}
