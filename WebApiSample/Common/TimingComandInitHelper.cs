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
    public class TimingComandInitHelper
    {
        private string TimingCommandHost = "http://mywebapidemo.azurewebsites.net/api/TimingCommand";

        public async Task<InitialTimingCommandStatus> CreateTimingCommand(string userName)
        {
            TimingCommandInfo timingCommand = new TimingCommandInfo();
            timingCommand.userName = userName;
            timingCommand.command = "Off";
            string jsonConent = JsonHelper.ObjectToJson(timingCommand);
            HttpService http = new HttpService();
            HttpResponseMessage response = await http.SendPostRequest(TimingCommandHost, jsonConent);
            if (response.StatusCode == HttpStatusCode.Created)
                return InitialTimingCommandStatus.success;
            return InitialTimingCommandStatus.failed;
        }

        public enum InitialTimingCommandStatus
        {
            success,failed
        }
    }
}
