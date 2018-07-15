using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loT4WebApiSample.Model
{
    public class CommandChannelInfo
    {
        public string userName { get; set; }
        public string channelUri { get; set; }
        public DateTime expirationTime { get; set; }
    }
}
