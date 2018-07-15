using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiDemo.Controllers
{
    public class TemperatureController : ApiController
    {
        private readonly Models.TemperatureDetails.TemptureRepository repository;
        public TemperatureController()
        {
            repository = new Models.TemperatureDetails.TemptureRepository();
        }

        [HttpGet]
        public HttpResponseMessage GetTemprature(string userName)
        {
            Models.TemperatureDetails.TemperatureModel model = repository.GetTempture(userName);
            if(model!=null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPost]
        public HttpResponseMessage CreateTemprature(Models.TemperatureDetails.TemperatureModel model)
        {
            if(ModelState.IsValid&&model!=null)
            {
                if(repository.CreateTempture(model)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, model);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
