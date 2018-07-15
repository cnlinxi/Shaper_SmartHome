using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiDemo.Models;

namespace WebApiDemo.Controllers
{
    public class UserInfoController : ApiController
    {
        private readonly Models.UserDetails.UserRepository repository;

        public UserInfoController()
        {
            this.repository = new Models.UserDetails.UserRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(string userName)
        {
            Models.UserDetails.UserModel model = repository.GetModel(userName);
            if(model!=null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(UserDetails.UserModel model)
        {
            if(ModelState.IsValid&&model!=null)
            {
                int effectRows = this.repository.CreateUser(model);
                if (effectRows>0)
                {
                    var response = Request.CreateResponse(HttpStatusCode.Created, model);
                    return response;
                }
                else if(effectRows==-1)
                {
                    var response = Request.CreateResponse(HttpStatusCode.Conflict);
                    return response;
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPut]
        public HttpResponseMessage Put(UserDetails.UserModel model, string userName)
        {
            if (this.ModelState.IsValid && model != null&&model.UserName.Equals(userName))
            {
                if(this.repository.Update(model, userName)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
