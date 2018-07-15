using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiDemo.Utils;

namespace WebApiDemo.Controllers
{
    public class CommandController : ApiController
    {
        const string secret = "9/uziiHzJJ3Ji0Z4qjztczEUT+95h7dN";
        const string sid = "ms-app://s-1-15-2-1087547445-2465971467-2419435991-397243456-4029592136-789690765-2371699257";
        const string notificationType = "wns/raw";
        const string contentType = "application/octet-stream";

        const string CommandTarget = "ShaperloT";

        private readonly Models.CommandDetails.CommandRepository repository;
        public CommandController()
        {
            repository = new Models.CommandDetails.CommandRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(string userName)
        {
            List<Models.CommandDetails.CommandChannelModel> lstModel = repository.GetModel(userName);
            if (lstModel.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.Created, lstModel);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpGet]
        public string SendCommand(string userName,string commandTarget,string commandContent)
        {
            List<Models.CommandDetails.CommandChannelModel> lstCommandModel = repository.GetModel(userName);
            string errorMsg = "";
            if(lstCommandModel.Count>0)
            {
                NotificationHelper command = new NotificationHelper();
                string commandChannelUri;
                foreach(var obj in lstCommandModel)
                {
                    if(commandTarget==CommandTarget)
                    {
                        commandChannelUri = obj.channelUri;
                        errorMsg = command.PostToWns(secret, sid, commandChannelUri, commandContent, notificationType, contentType);
                    }
                }
            }
            else
            {
                errorMsg = "获取channelUri出错";
            }

            return errorMsg;
        }

        [HttpPost]
        public HttpResponseMessage CreateCommandChannel(Models.CommandDetails.CommandChannelModel model)
        {
            if(ModelState.IsValid&&model!=null)
            {
                if(repository.CreateCommandChannel(model)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, model);
                }
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
