using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using myWebJobDemo.Utils;
using myWebJobDemo.Model;

namespace myWebJobDemo
{
    public class Functions
    {
        const string userName = "user";
        const string pictureUri = "http://a0.att.hudong.com/68/18/16300001051406137869184233456.jpg";

        // This function will be triggered based on the schedule you have set for this WebJob
        // This function will enqueue a message on an Azure Queue called queue
        //[NoAutomaticTrigger]
        //public static void ManualTrigger(TextWriter log, int value, [Queue("queue")] out string message)
        //{
        //    log.WriteLine("Function is invoked with value={0}", value);
        //    message = value.ToString();
        //    log.WriteLine("Following message will be written on the Queue={0}", message);
        //}

        [NoAutomaticTrigger]
        public static async void UpdateFaceId(TextWriter log)
        {
            FaceIdHelper faceHelper = new FaceIdHelper();
            FaceInfo faceInfo = await faceHelper.FaceDetection(pictureUri);
            if(faceInfo!=null)
            {
                UserFaceIdDetails userFace = new UserFaceIdDetails();
                userFace.UserName = userName;
                userFace.FaceId = faceInfo.faceId;
                UserFaceIdRepository repository = new UserFaceIdRepository();
                int i = repository.UpdateFaceId(userFace, userName);
                if (i > 0)
                {
                    log.WriteLine("更新成功，更新的UserName:{0},更新的FaceId:{1}", userFace.UserName, userFace.FaceId);
                }
                else
                {
                    log.WriteLine("更新期间发生异常！");
                }
            }
        }
    }
}
