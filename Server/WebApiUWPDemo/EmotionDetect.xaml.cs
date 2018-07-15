using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WebApiUWPDemo.Common;
using WebApiUWPDemo.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class EmotionDetect : Page
    {
        HttpClient httpClient;
        string uriEmotionDetect = "https://api.projectoxford.ai/emotion/v1.0/recognize";
        const string subscriptionKey = "d158ee5c690f460da3bf86ab60973fcf";
        public EmotionDetect()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
        }

        private async void btnEmotionDetect_Click(object sender, RoutedEventArgs e)
        {
            this.btnEmotionDetect.IsEnabled = false;
            StorageFile imgFile = await TakePhoto();
            if(imgFile!=null)
            {
                this.loading.IsActive = true;
                await EmotionDetection(imgFile);
            }
            this.btnEmotionDetect.IsEnabled = true;
            this.loading.IsActive = false;
        }

        private async Task<StorageFile> TakePhoto()
        {
            CameraCaptureUI cameraUi = new CameraCaptureUI();
            cameraUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

            StorageFile photo = await cameraUi.CaptureFileAsync(CameraCaptureUIMode.Photo);
            return photo;
        }

        private async Task<string> EmotionDetection(StorageFile imgFile)
        {
            try
            {
                if (imgFile != null)
                {
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    IRandomAccessStream stream = await imgFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var bytes = new byte[stream.Size];
                    await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
                    HttpResponseMessage response;
                    using (var content = new ByteArrayContent(bytes))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        response = await httpClient.PostAsync(uriEmotionDetect, content);
                        string strResponce = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string emotion = GetEmotion(strResponce);
                            this.txtMessage.Text = "请求成功！表情为：" + emotion;
                            return emotion;
                            //解析返回的json,只取图片中第一个人的人脸表情数据
                            //EmotionInfo emotion = new EmotionInfo();
                            //JArray jsonArray = (JArray)JsonConvert.DeserializeObject(strResponce);
                            //string strFirstFaceEmotion = jsonArray[0].ToString();
                            //Object obj = JsonHelper.JsonToObject(strFirstFaceEmotion, emotion);
                            //if(obj is EmotionInfo)
                            //{
                            //    emotion = JsonHelper.JsonToObject(strFirstFaceEmotion, emotion) as EmotionInfo;
                            //}
                            //this.txtMessage.Text = "请求成功！FaceId:" + face.faceId;
                            //return face;
                        }
                        else
                        {
                            txtMessage.Text = "请求失败！失败原因:" + strResponce;
                            return null;
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得照片最有可能的表情
        /// </summary>
        /// <param name="jsonEmotion">表情json(应该为一个json数组)</param>
        /// <returns>表情名</returns>
        private string GetEmotion(string jsonEmotion)
        {
            Dictionary<string, double> emotions = new Dictionary<string, double>();
            JArray jsonArray = (JArray)JsonConvert.DeserializeObject(jsonEmotion);
            //只取第一个人脸
            string jsonFirstEmotion = jsonArray[0]["scores"].ToString();
            JObject jsonObj = JObject.Parse(jsonFirstEmotion);
            emotions.Add("anger", (double)jsonObj["anger"]);
            emotions.Add("contempt", (double)jsonObj["contempt"]);
            emotions.Add("disgust", (double)jsonObj["disgust"]);
            emotions.Add("fear", (double)jsonObj["fear"]);
            emotions.Add("happiness", (double)jsonObj["happiness"]);
            emotions.Add("neutral", (double)jsonObj["neutral"]);
            emotions.Add("sadness", (double)jsonObj["sadness"]);
            emotions.Add("surprise", (double)jsonObj["surprise"]);

            var dicSort = emotions.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, p => p.Value);
            return dicSort.FirstOrDefault().Key;
        }
    }
}
