using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiDemo.Controllers
{
    public class InitialDeviceController : ApiController
    {
        private readonly Models.DeviceInitializationDetails.DeviceInitializationRepositoty repository;
        public InitialDeviceController()
        {
            repository = new Models.DeviceInitializationDetails.DeviceInitializationRepositoty();
        }

        [HttpGet]
        public HttpResponseMessage Get(string authCode)
        {
            Models.DeviceInitializationDetails.DeviceInitializationModel model =
                repository.GetModel(authCode);
            if(model!=null&&model.userName.Length>0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPost]
        public HttpResponseMessage Post(Models.DeviceInitializationDetails.DeviceInitializationModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                if (repository.CreateDeviceInitialization(model) > 0)
                    return Request.CreateResponse(HttpStatusCode.Created, model);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPut]
        public HttpResponseMessage Update(string userName, Models.DeviceInitializationDetails.DeviceInitializationModel model)
        {
            if (model != null && ModelState.IsValid && model.userName == userName)
            {
                if (repository.Update(userName, model) > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
