using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApiSample.Common;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace WebApiSample.Helpers
{
    public class HttpService:IDisposable
    {
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        private Uri uri;

        private int errorCounter;
        public HttpService()
        {
            CreateHttpClient(ref httpClient);
            cts = new CancellationTokenSource();
            errorCounter = 0;
        }
        /// <summary>
        /// 创建HttpClient对象
        /// </summary>
        /// <param name="httpClient">要实例化的httpClient对象</param>
        internal static void CreateHttpClient(ref HttpClient httpClient)
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }
            //添加过滤器
            IHttpFilter filter = new HttpBaseProtocolFilter();
            filter = new PlugInFilter(filter);
            httpClient = new HttpClient(filter);

            //添加User-Agent
            httpClient.DefaultRequestHeaders.UserAgent.Add(new Windows.Web.Http.Headers.HttpProductInfoHeaderValue("mySample", "v1"));
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

        /// <summary>
        /// 发送Get请求(Windows.Web)
        /// </summary>
        /// <param name="url">server地址</param>
        /// <returns>请求返回的字符串</returns>
        public async Task<string> SendGetRequest(string url)
        {
            if(TryGetUri(url,out uri))
            {
                try
                {
                    string response = await httpClient.GetStringAsync(uri).AsTask(cts.Token);
                    return response;
                }
                catch(OperationCanceledException e)
                {

                }
                catch(Exception ex)
                {
                    if(errorCounter<2)
                    {
                        errorCounter++;

                        await SendGetRequest(url);
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送Post请求（Windows.Web），请求体为json字符串
        /// </summary>
        /// <param name="url">server地址</param>
        /// <param name="jsonBody">json字符串</param>
        /// <returns>HttpResponseMessage类型的response</returns>
        public async Task<HttpResponseMessage> SendPostRequest(string url,string jsonBody)
        {
            if(TryGetUri(url, out uri))
            {
                try
                {
                    IHttpContent httpContent = new HttpJsonContent(JsonValue.Parse(jsonBody));
                    HttpResponseMessage response = await httpClient.PostAsync(uri, 
                        httpContent).AsTask(cts.Token);
                    return response;
                }
                catch(OperationCanceledException e)
                { }
                catch(Exception ex)
                {
                    if(errorCounter<2)
                    {
                        errorCounter++;
                        await SendPostRequest(url, jsonBody);
                    }
                }
            }
            return null;
        }

        public void Dispose()
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
    }
}
