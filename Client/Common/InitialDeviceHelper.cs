using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiSample.Helpers;
using WebApiSample.Model;
using Windows.Web.Http;

namespace WebApiSample.Common
{
    public class InitialDeviceHelper
    {
        private string DeviceInitializationHost = "http://mywebapidemo.azurewebsites.net/api/InitialDevice";
        public async Task<InitialDeviceStatus> CreateInitialDevice(string userName)
        {
            DeviceInitialInfo deviceInitialization = new DeviceInitialInfo();
            deviceInitialization.userName = userName;
            deviceInitialization.authCode = "1";
            string jsonConent = JsonHelper.ObjectToJson(deviceInitialization);
            HttpService http = new HttpService();
            HttpResponseMessage response = await http.SendPostRequest(DeviceInitializationHost, jsonConent);
            if (response.StatusCode == HttpStatusCode.Created)
                return InitialDeviceStatus.success;
            return InitialDeviceStatus.failed;
        }

        public async Task<InitialDeviceStatus> SendAuthCode(string userName,string authCode)
        {
            DeviceInitialInfo deviceInitialzation = new DeviceInitialInfo();
            deviceInitialzation.userName = userName;
            deviceInitialzation.authCode = authCode;
            string jsonContent = JsonHelper.ObjectToJson(deviceInitialzation);
            HttpService http = new HttpService();
            string queryString = string.Format("?userName={0}", userName);
            HttpResponseMessage response = await http.SendPutRequest(DeviceInitializationHost + queryString, jsonContent);
            if (response.StatusCode == HttpStatusCode.Ok)
                return InitialDeviceStatus.success;
            return InitialDeviceStatus.failed;
        }

        public enum InitialDeviceStatus
        {
            success,failed
        }
    }
}
