using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiSample.Common;
using WebApiSample.FaceRecognizatioin;
using WebApiSample.Model;
using Windows.Security.Credentials;
using Windows.Web.Http;

namespace WebApiSample.Helpers
{
    public class UserAccountService
    {
        private string uriRegister = "http://mywebapidemo.azurewebsites.net/api/UserInfo";
        private string resourceName = "WebApiSample";

        /// <summary>
        /// 登录服务
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>是否登录成功，成功返回true，否则false</returns>
        public async Task<bool> Login(string userName,string password)
        {
            string uriLogin = String.Format("http://mywebapidemo.azurewebsites.net/api/UserInfo?userName={0}", userName);
            HttpService http = new HttpService();
            string response = await http.SendGetRequest(uriLogin);
            if(response!=string.Empty)
            {
                try
                {
                    UserInfo user = new UserInfo();
                    user = JsonHelper.JsonToObject(response, user) as UserInfo;
                    if (user.Password == EncriptHelper.ToMd5(password))
                        return true;
                }
                catch { }
            }
            return false;
        }

        public void Loginout()
        {
            PasswordCredential credential = GetCredentialFromLocker();
            ClearCredentialFromLocker(credential.UserName, credential.Password);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>注册状态</returns>
        public async Task<RegisterStaus> Register(string userName,string password)
        {
            UserInfo user = new UserInfo();
            user.UserName = userName;
            user.Password = password;
            try
            {
                string json = JsonHelper.ObjectToJson(user);
                HttpService http = new HttpService();
                HttpResponseMessage response = await http.SendPostRequest(uriRegister, json);
                if(response!=null)
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        FaceApiHelper faceApi = new FaceApiHelper();
                        string facelistId = EncriptHelper.ToMd5(userName);
                        facelistId = facelistId.ToLower();
                        FaceApiHelper.FaceListStatus status = await faceApi.FaceListCreate(facelistId);
                        if (status == FaceApiHelper.FaceListStatus.success)
                        {
                            return RegisterStaus.Success;
                        }
                        return RegisterStaus.FaceListFailed;
                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        return RegisterStaus.ConflictUserName;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return RegisterStaus.Failed;
                    }
                }
            }
            catch { }
            return RegisterStaus.Failed;
        }

        /// <summary>
        /// 从凭据保险箱获取凭据
        /// </summary>
        /// <returns>无凭据返回空，否则返回PasswordCredential</returns>
        public PasswordCredential GetCredentialFromLocker()
        {
            try
            {
                PasswordCredential credential = null;

                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource(resourceName);
                if (credentialList.Count > 0)
                {
                    credential = credentialList[0];
                }
                return credential;
            }
            catch { return null; }
        }

        public string GetUserNameFromLocker()
        {
            PasswordCredential credential = GetCredentialFromLocker();
            if(credential!=null)
            {
                return credential.UserName;
            }
            return string.Empty;
        }

        public void ClearAllCredentialFromLocker()
        {
            var vault = new PasswordVault();
            var credentialList = vault.FindAllByResource(resourceName);
            if (credentialList.Count > 0)
            {
                foreach(var obj in credentialList)
                {
                    ClearCredentialFromLocker(obj.UserName, obj.Password);
                }
            }
        }

        /// <summary>
        /// 从凭据保险箱删除凭据
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void ClearCredentialFromLocker(string userName,string password)
        {
            var vault = new PasswordVault();
            var credential = new PasswordCredential(resourceName, userName, password);
            vault.Remove(credential);
        }

        public enum RegisterStaus
        {
            Success,ConflictUserName,Failed,FaceListFailed
        }
    }
}
