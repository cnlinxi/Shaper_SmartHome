using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiDemo.Models;

namespace WebApiDemo.Controllers
{
    public class InformationController : ApiController
    {
        private readonly InformationDetails.ConfigurationRepository repository;

        public InformationController()
        {
            repository = new InformationDetails.ConfigurationRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(string contentId)
        {
            InformationDetails.InformationModel model = repository.GetModel(contentId);
            if(model!=null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, model.Content);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(InformationDetails.InformationModel model)
        {
            if (ModelState.IsValid && model != null)
            {
                if (repository.CreateInformation(model) > 0)
                {
                    var response = Request.CreateResponse(HttpStatusCode.Created, model);
                    return response;
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}