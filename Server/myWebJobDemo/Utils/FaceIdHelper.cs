using myWebJobDemo.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace myWebJobDemo.Utils
{
    public class FaceIdHelper
    {
        HttpClient httpClient=new HttpClient();
        const string subscriptionKey = "54fa2361cc0845ed9a970a84d43aa955";
        string uriDetect = "https://api.projectoxford.ai/face/v1.0/detect";

        public async Task<FaceInfo> FaceDetection(string uriFacePicture)
        {
            if (uriFacePicture.Length>0)
            {
                var queryString = string.Format("?returnFaceId={0}&returnFaceLandmarks={1}", true, false);//只返回FaceId
                uriDetect += queryString;

                try
                {
                    //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/octet-stream");
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    FaceUriModel faceUri = new FaceUriModel();
                    faceUri.url = uriFacePicture;
                    string strRequestContent = JsonHelper.ObjectToJson(faceUri);
                    byte[] byteData = Encoding.UTF8.GetBytes(strRequestContent);
                    HttpResponseMessage response;
                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        response = await httpClient.PostAsync(uriDetect, content);
                        string strResponce = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            //解析返回的json,只取图片中第一个人的人脸数据
                            FaceInfo face = new FaceInfo();
                            JArray jsonArray = (JArray)JsonConvert.DeserializeObject(strResponce);
                            face.faceId = jsonArray[0]["faceId"].ToString();/////此处代码维护性差，一旦api改动，此处可能需要修改！！！！
                            face.faceRectangle = jsonArray[0]["faceRectangle"].ToString();
                            return face;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
            return null;
        }
    }
}
