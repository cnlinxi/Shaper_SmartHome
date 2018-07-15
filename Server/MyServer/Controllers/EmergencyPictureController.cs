using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiDemo.Controllers
{
    public class EmergencyPictureController : ApiController
    {
        private readonly Models.EmergencePictureDetails.EmergencePictureRepository repository;
        public EmergencyPictureController()
        {
            repository = new Models.EmergencePictureDetails.EmergencePictureRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(string userName)
        {
            List<Models.EmergencePictureDetails.EmergencePictureModel> lstPicture = repository.GetModels(userName);
            if(lstPicture.Count>0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, lstPicture);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post(Models.EmergencePictureDetails.EmergencePictureModel model)
        {
            if(ModelState.IsValid&&model!=null)
            {
                if(repository.CreateEmergencePicture(model)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, model);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpDelete]
        public HttpResponseMessage Delete(string userName,string pictureName)
        {
            if(ModelState.IsValid)
            {
                //数据库中存储图片的名称，旧代码中依旧使用pictureUri的变量名，实际上存储的是图片的名称
                if(repository.DeleteEmergencePictureUri(userName,pictureName)>0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, pictureName);
                }
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
