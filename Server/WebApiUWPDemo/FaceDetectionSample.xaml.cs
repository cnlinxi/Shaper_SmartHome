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
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiUWPDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FaceDetectionSample : Page
    {
        HttpClient httpClient;
        string uriDetect = "https://api.projectoxford.ai/face/v1.0/detect";
        string uriVerify = "https://api.projectoxford.ai/face/v1.0/verify";
        const string subscriptionKey = "54fa2361cc0845ed9a970a84d43aa955";
        const string FaceId = "6016cc91-3dbd-4a12-acd1-fbd7ce3d7cfc";

        public FaceDetectionSample()
        {
            this.InitializeComponent();

            httpClient = new HttpClient();
        }

        private async void btnFaceDetect_Click(object sender, RoutedEventArgs e)
        {
            this.btnFaceDetect.IsEnabled = false;
            this.loading.IsActive = true;
            StorageFile imgFile =await PickFile();
            await FaceDetection(imgFile);
            this.btnFaceDetect.IsEnabled = true;
            this.loading.IsActive = false;
        }

        private async void btnFaceVerify_Click(object sender, RoutedEventArgs e)
        {
            this.btnFaceVerify.IsEnabled = false;
            await FaceVerify(FaceId);
            this.btnFaceVerify.IsEnabled = true;
        }

        /// <summary>
        /// 人脸识别
        /// </summary>
        /// <param name="imgFile">照片的StorageFile</param>
        /// <returns>人脸模型</returns>
        private async Task<FaceInfo> FaceDetection(StorageFile imgFile)
        {
            if(imgFile!=null)
            {
                var queryString = string.Format("?returnFaceId={0}&returnFaceLandmarks={1}", true, false);//只返回FaceId
                uriDetect += queryString;

                try
                {
                    //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/octet-stream");
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    IRandomAccessStream stream = await imgFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var bytes = new byte[stream.Size];
                    await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
                    HttpResponseMessage response;
                    using (var content = new ByteArrayContent(bytes))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        response = await httpClient.PostAsync(uriDetect, content);
                        string strResponce = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            //解析返回的json,只取图片中第一个人的人脸数据
                            FaceInfo face = new FaceInfo();
                            JArray jsonArray = (JArray)JsonConvert.DeserializeObject(strResponce);
                            face.faceId = jsonArray[0]["faceId"].ToString();/////此处代码维护性差，一旦api改动，此处可能需要修改！！！！
                            face.faceRectangle = jsonArray[0]["faceRectangle"].ToString();
                            this.txtMessage.Text = "请求成功！FaceId:" + face.faceId;
                            return face;
                        }
                        else
                        {
                            txtMessage.Text = "请求失败！失败原因:" + strResponce;
                            return null;
                        }
                    }
                }
                catch(Exception ex)
                {
                    txtMessage.Text = "Exception中请求失败！失败原因:" + ex.Message;
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 人脸比对，此方法首先调用照相机拍摄一张，然后与传入的faceId所对应的照片比对
        /// </summary>
        /// <param name="faceId">需要与将要拍摄的照片进行比对的现有照片的faceId</param>
        /// <returns></returns>
        private async Task FaceVerify(string faceId)
        {
            CameraCaptureUI cameraUi = new CameraCaptureUI();
            cameraUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

            StorageFile photo = await cameraUi.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if(photo!=null)
            {
                IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
                                                                                                                    BitmapPixelFormat.Bgra8,
                                                                                                                    BitmapAlphaMode.Premultiplied);

                SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

                imageFace.Source = bitmapSource;

                this.loading.IsActive = true;
                FaceInfo face = await FaceDetection(photo);
                try
                {
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    FaceVerifyInfo faceVerifyInfo = new FaceVerifyInfo();
                    faceVerifyInfo.faceId1 = faceId;
                    faceVerifyInfo.faceId2 = face.faceId;
                    string json = JsonHelper.ObjectToJson(faceVerifyInfo);

                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    HttpResponseMessage response;
                    using (var content = new ByteArrayContent(bytes))
                    {
                        content.Headers.ContentType = 
                            new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        response = await httpClient.PostAsync(uriVerify, content);
                        if(response.StatusCode==HttpStatusCode.OK)
                        {
                            string strResponse = await response.Content.ReadAsStringAsync();
                            JObject jsonObj = JObject.Parse(strResponse);
                            if ((Boolean)jsonObj["isIdentical"])/////此处代码维护性差，一旦api改动，此处可能需要修改！！！！
                            {
                                txtMessage.Text = "识别成功！这张脸FaceId:" + face.faceId;
                            }
                            else
                            {
                                txtMessage.Text = "识别失败！失败原因：" + strResponse;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    txtMessage.Text = "Exception识别失败！失败原因：" + ex.Message;
                }

                this.loading.IsActive = false;
            }
        }

        /// <summary>
        /// 使用选择器选取文件
        /// </summary>
        /// <returns>文件的StorageFile</returns>
        private async Task<StorageFile> PickFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".JPEG");
            picker.FileTypeFilter.Add(".PNG");
            picker.FileTypeFilter.Add(".BMP");

            var file = await picker.PickSingleFileAsync();
            return file;
        }
    }
}
