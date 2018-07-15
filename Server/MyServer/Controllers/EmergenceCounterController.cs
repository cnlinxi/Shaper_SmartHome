using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiDemo.Controllers
{
    public class EmergenceCounterController : ApiController
    {
        private readonly Models.EmergenceCounterDetails.EmergenceCounterRepository repository;

        public EmergenceCounterController()
        {
            this.repository = new Models.EmergenceCounterDetails.EmergenceCounterRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(string userName)
        {
            List<Models.EmergenceCounterDetails.EmergenceCounterModel> lstCounter =
                repository.GetModels(userName);
            if (lstCounter.Count > 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, lstCounter);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(Models.EmergenceCounterDetails.EmergenceCounterModel model)
        {
            if (ModelState.IsValid && model != null)
            {
                if (repository.CreateEmergenceCounter(model) > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, model);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
