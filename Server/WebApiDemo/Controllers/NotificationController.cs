using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiDemo.Models;
using WebApiDemo.Utils;

namespace WebApiDemo.Controllers
{
    /// <summary>
    /// 这里对应的UWP是单独的一个项目App4Notification
    /// </summary>
    public class NotificationController : ApiController
    {
        const string secret = "CeSkCQQ+z9C7cKgPzup6yO7iy+6Tj2Dx";
        const string sid = "ms-app://s-1-15-2-2030972143-3306208989-117622112-2817077788-2205654758-887429437-499873462";
        const string notificationType = "wns/toast";
        const string contentType = "text/xml";

        private readonly ConfigurationDetails.ConfigurationRepository respository;

        public NotificationController()
        {
            this.respository = new ConfigurationDetails.ConfigurationRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(string userName)
        {
            List<ConfigurationDetails.ConfigurationModel> lstModel = respository.GetModel(userName);
            if(lstModel.Count>0)
            {
                return Request.CreateResponse(HttpStatusCode.Created, lstModel);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpGet]
        public string SendNotification(string content,string userName)
        {
            List<ConfigurationDetails.ConfigurationModel> lstModel = respository.GetModel(userName);
            string channelUri="";
            string strErrorMsg = "";
            if (lstModel.Count>0)
            {
                NotificationHelper notification = new NotificationHelper();
                //关于toast的详细xml写法，
                //参见http://blogs.msdn.com/b/tiles_and_toasts/archive/2015/07/02/adaptive-and-interactive-toast-notifications-for-windows-10.aspx
                string xmlContent = string.Format(
                         "<toast launch=''>" +
                               "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                          "<text>Shaper智能家居</text>"+
                                          "<text>{0}</text>" +
                                          "<image placement='appLogo' src='Assets/toast.jpg'/>" +
                                    "</binding>" +
                               "</visual>" +
                         "</toast>",
                         content
                    );
                foreach(var model in lstModel)
                {
                    channelUri = model.channelUri;
                    strErrorMsg += notification.PostToWns(secret, sid, channelUri, xmlContent, notificationType, contentType);
                }
            }
            else
            {
                return strErrorMsg="获取channelUri出错";
            }
            return strErrorMsg;
        }

        [HttpPost]
        public HttpResponseMessage Post(ConfigurationDetails.ConfigurationModel model)
        {
            if(ModelState.IsValid&&model!=null)
            {
                if(respository.CreateConfiguration(model)>0)
                {
                    var response = Request.CreateResponse(HttpStatusCode.Created,model);
                    return response;
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        public HttpResponseMessage Put(string userName, ConfigurationDetails.ConfigurationModel model)
        {
            if(ModelState.IsValid&&model!=null&&model.userName==userName)
            {
                if(respository.Update(model,userName)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
