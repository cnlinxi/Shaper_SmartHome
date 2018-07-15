using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiDemo.Controllers
{
    public class TimingCommandController : ApiController
    {
        private readonly Models.TimingCommandDetails.TimingCommandRepository repository;
        public TimingCommandController()
        {
            repository = new Models.TimingCommandDetails.TimingCommandRepository();
        }

        [HttpGet]
        public HttpResponseMessage GetCommand(string userName)
        {
            Models.TimingCommandDetails.TimingCommandModel model =
                repository.GetModel(userName);
            if(model!=null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPost]
        public HttpResponseMessage CreateCommand(Models.TimingCommandDetails.TimingCommandModel model)
        {
            if(model!=null&&ModelState.IsValid)
            {
                if (repository.CreateCommand(model) > 0)
                    return Request.CreateResponse(HttpStatusCode.Created, model);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPut]
        public HttpResponseMessage UpdateCommand(string userName, Models.TimingCommandDetails.TimingCommandModel model)
        {
            if(model!=null&&ModelState.IsValid&&model.userName==userName)
            {
                if(repository.Update(userName,model)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
