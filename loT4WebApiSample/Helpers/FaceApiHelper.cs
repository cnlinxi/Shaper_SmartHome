using loT4WebApiSample.Common;
using loT4WebApiSample.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using WebApiSample.Model;
using Windows.Storage;
using Windows.Storage.Streams;

namespace loT4WebApiSample.Helpers
{
    public class FaceApiHelper
    {
        HttpClient httpClient;
        string uriDetect = "https://api.projectoxford.ai/face/v1.0/detect";
        string uriVerify = "https://api.projectoxford.ai/face/v1.0/verify";
        string uriFaceListCreation = "https://api.projectoxford.ai/face/v1.0/facelists/";
        string uriFaceListAddFace = "https://api.projectoxford.ai/face/v1.0/facelists/";
        string uriFaceListGetName = "https://api.projectoxford.ai/face/v1.0/facelists/";
        string uriFaceListDeleteFace = "https://api.projectoxford.ai/face/v1.0/facelists/";
        string uriFaceSimilar = "https://api.projectoxford.ai/face/v1.0/findsimilars";

        const string subscriptionKey = "54fa2361cc0845ed9a970a84d43aa955";

        int errorCounter = 0;

        public FaceApiHelper()
        {
            httpClient = new HttpClient();
            errorCounter = 0;
        }

        /// <summary>
        /// 人脸识别
        /// </summary>
        /// <param name="imgFile">照片的StorageFile</param>
        /// <returns>人脸模型</returns>
        public async Task<FaceInfo> FaceDetection(StorageFile imgFile)
        {
            if (imgFile != null)
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
                            return face;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (errorCounter < 2)
                    {
                        errorCounter++;

                        await FaceDetection(imgFile);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 创建人脸列表
        /// </summary>
        /// <param name="faceListId">需要创建的人脸列表Id</param>
        /// <param name="userData">人脸列表的附加数据（不填）</param>
        /// <returns>人脸列表创建状态</returns>
        public async Task<FaceListStatus> FaceListCreate(string faceListId,string userData=null)
        {
            if(faceListId.Length<64)
            {
                string faceListIdLow = faceListId.ToLower();
                try
                {
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    uriFaceListCreation += faceListIdLow;
                    FaceListInfo faceList = new FaceListInfo();
                    faceList.name = faceListIdLow;
                    faceList.userData = userData;
                    string strJson = JsonHelper.ObjectToJson(faceList);
                    byte[] byteData = Encoding.UTF8.GetBytes(strJson);
                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        HttpResponseMessage response = await httpClient.PutAsync(uriFaceListCreation, content);
                        if(response.StatusCode==HttpStatusCode.OK)
                        {
                            return FaceListStatus.success;
                        }
                    }
                }
                catch
                {
                    if(errorCounter<2)
                    {
                        errorCounter++;

                        await FaceListCreate(faceListId, userData);
                    }
                }
                return FaceListStatus.failed;
            }
            else
            {
                return FaceListStatus.namelong;
            }
        }

        public async Task<FaceListStatus>FaceListAddFace(StorageFile imgFile,string faceListId,string userData)
        {
            if (faceListId.Length < 64&&userData.Length<128)
            {
                string faceListIdLow = faceListId.ToLower();
                try
                {
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    string queryString;
                    if (userData == null)
                    {
                        queryString = string.Format("{0}/persistedFaces", faceListIdLow);
                    }
                    else
                    {
                        queryString = string.Format("{0}/persistedFaces?userData={1}", faceListIdLow, userData);
                    }
                    uriFaceListAddFace += queryString;
                    IRandomAccessStream stream = await imgFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var byteData = new byte[stream.Size];
                    await stream.ReadAsync(byteData.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        HttpResponseMessage response = await httpClient.PostAsync(uriFaceListAddFace, content);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return FaceListStatus.success;
                        }
                    }
                }
                catch
                {
                    if (errorCounter < 2)
                    {
                        await FaceListAddFace(imgFile, faceListId, userData);
                        errorCounter++;
                    }
                }
                return FaceListStatus.failed;
            }
            else
            {
                return FaceListStatus.namelong;
            }
        }

        public async Task<List<string>> GetFaceListUserName(string faceListId)
        {
            faceListId = faceListId.ToLower();
            List<string> listName = new List<string>();
            try
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                uriFaceListGetName += faceListId;
                HttpResponseMessage response = await httpClient.GetAsync(uriFaceListGetName);
                if(response.StatusCode==HttpStatusCode.OK)
                {
                    string strResponse = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(strResponse);
                    JArray jsonArray =(JArray) json["persistedFaces"];
                    foreach(JObject obj in jsonArray)
                    {
                        string name = obj["userData"].ToString();
                        listName.Add(name);
                    }
                }
                return listName;
            }
            catch
            {
                if (errorCounter < 2)
                {
                    errorCounter++;

                    await GetFaceListUserName(faceListId);
                }
            }
            return listName;
        }

        public async Task<string> GetFaceListFaceId(string faceListId,string userName)
        {
            faceListId = faceListId.ToLower();
            try
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                uriFaceListGetName += faceListId;
                HttpResponseMessage response = await httpClient.GetAsync(uriFaceListGetName);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string strResponse = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(strResponse);
                    JArray jsonArray = (JArray)json["persistedFaces"];
                    foreach (JObject obj in jsonArray)
                    {
                        string name = obj["userData"].ToString();
                        if(name==userName)
                        {
                            return obj["persistedFaceId"].ToString();
                        }
                    }
                }
                return string.Empty;
            }
            catch
            {
                if (errorCounter < 2)
                {
                    errorCounter++;

                    await GetFaceListFaceId(faceListId,userName);
                }
            }
            return string.Empty;
        }

        public async Task<bool> FaceListDeleteFace(string faceListId,string userName)
        {
            faceListId = faceListId.ToLower();
            try
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string persistedFaceId =await GetFaceListFaceId(faceListId, userName);
                if(persistedFaceId!=string.Empty)
                {
                    string queryString = string.Format("{0}/persistedFaces/{1}", faceListId, persistedFaceId);
                    uriFaceListDeleteFace += queryString;
                    HttpResponseMessage response = await httpClient.DeleteAsync(uriFaceListDeleteFace);
                    if (response.StatusCode == HttpStatusCode.OK)
                        return true;
                }
            }
            catch
            {
                if(errorCounter<2)
                {
                    errorCounter++;

                    await FaceListDeleteFace(faceListId, userName);
                }
            }
            return false;
        }

        /// <summary>
        /// 从FaceList中查找FaceId对应的人脸，返回置信度（0-1）
        /// </summary>
        /// <param name="faceId">人脸FaceId</param>
        /// <param name="faceListId">人脸FaceListId</param>
        /// <returns>浮点型置信度（0-1）</returns>
        public async Task<double> FaceSimilar(string faceId,string faceListId)
        {
           try
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                FaceSimilarInfo faceSimilar = new FaceSimilarInfo();
                faceSimilar.faceId = faceId;
                faceSimilar.faceListId = faceListId;
                faceSimilar.maxNumOfCandidatesReturned = 3;
                string json = JsonHelper.ObjectToJson(faceSimilar);
                byte[] byteData = Encoding.UTF8.GetBytes(json);
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response =await httpClient.PostAsync(uriFaceSimilar, content);
                    if(response.StatusCode==HttpStatusCode.OK)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JArray jsonArray = JArray.Parse(jsonResponse);
                        if(jsonArray.Count>0)
                        {
                            return (double)jsonArray[0]["confidence"];
                        }
                    }
                }
            }
            catch
            {
                if(errorCounter<2)
                {
                    errorCounter++;

                    await FaceSimilar(faceId, faceListId);
                }
            }
            return -1;
        }

        /// <summary>
        /// 在FaceList中查找faceId对应的人脸，若找到返回成员名
        /// </summary>
        /// <param name="faceId">人脸Faceld</param>
        /// <param name="faceListId">人脸FaceIdList</param>
        /// <returns>若找到，返回成员名，否则返回空字符串</returns>
        public async Task<string> FaceSimilarWithMemberName(string faceId, string faceListId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                FaceSimilarInfo faceSimilar = new FaceSimilarInfo();
                faceSimilar.faceId = faceId;
                faceSimilar.faceListId = faceListId.ToLower();
                faceSimilar.maxNumOfCandidatesReturned = 3;
                string json = JsonHelper.ObjectToJson(faceSimilar);
                byte[] byteData = Encoding.UTF8.GetBytes(json);
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(uriFaceSimilar, content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JArray jsonArray = JArray.Parse(jsonResponse);
                        if (jsonArray.Count > 0)
                        {
                            uriFaceListGetName += faceListId.ToLower();
                            response = await httpClient.GetAsync(uriFaceListGetName);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string strResponse = await response.Content.ReadAsStringAsync();
                                JObject jsonObject = JObject.Parse(strResponse);
                                JArray jsonarray = (JArray)jsonObject["persistedFaces"];
                                foreach (JObject obj in jsonarray)//这里的obj是getFacelist 的Object
                                {
                                    if (obj["persistedFaceId"].ToString() == jsonArray[0]["persistedFaceId"].ToString())
                                    {
                                        return obj["userData"].ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                if (errorCounter < 2)
                {
                    errorCounter++;

                    await FaceSimilarWithMemberName(faceId, faceListId);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 人脸比对
        /// </summary>
        /// <param name="faceId">两个需要比对的人脸faceid1，faceid2</param>
        /// <returns>人脸比对的结果（faceverifystatus枚举类型）</returns>
        public async Task<FaceVerifyStatus> FaceVerify(string faceId1, string faceId2)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                FaceVerifyInfo faceVerifyInfo = new FaceVerifyInfo();
                faceVerifyInfo.faceId1 = faceId1;
                faceVerifyInfo.faceId2 = faceId2;
                string json = JsonHelper.ObjectToJson(faceVerifyInfo);

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                HttpResponseMessage response;
                using (var content = new ByteArrayContent(bytes))
                {
                    content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    response = await httpClient.PostAsync(uriVerify, content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string strResponse = await response.Content.ReadAsStringAsync();
                        JObject jsonObj = JObject.Parse(strResponse);
                        if ((Boolean)jsonObj["isIdentical"])/////此处代码维护性差，一旦api改动，此处可能需要修改！！！！
                        {
                            return FaceVerifyStatus.similar;
                        }
                        else
                        {
                            return FaceVerifyStatus.unlike;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (errorCounter < 2)
                {
                    errorCounter++;

                    await FaceVerify(faceId1, faceId2);
                }
            }
            return FaceVerifyStatus.failed;
        }

        public enum FaceVerifyStatus
        {
            similar, unlike, failed
        }
        public enum FaceListStatus
        {
            success,failed,namelong
        }
    }
}
