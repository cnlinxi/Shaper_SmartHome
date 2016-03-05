using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loT4WebApiSample
{
    public static class Constants
    {
        public static class SpeechConstants
        {
            public const string InitialSpeechMessage = "语音初始化完成";
            public const string GreetingMessage = "你好，正在核实你的身份";

            public const string VisitorNotRecognizedMessage = "对不起，你可能无权入内";
            public const string NoCameraMessage = "对不起，摄像头似乎没有连接";

            public const string FireWariningMessage = "警报！疑似发生火警";

            public static string GeneralGreetigMessage(string visitorName)
            {
                return visitorName+",欢迎回家";
            }
        }

        public static class NotificationConstants
        {
            public const string FireWarining = "警报，家中疑似发生火警";
        } 

        public static class FaceConstants
        {
            public const int FaceRecognizationFailedDuration = 5;
            public const int MaxFaceRecognizationFailed = 5;
        }

        public static class GpioConstants
        {
            //门铃（按钮）关联的Gpio pin
            public const int doorbellPinID = 5;
            //门锁(led灯代替)关联的Gpio pin
            public const int doorlockPinID = 19;
            //门锁打开的时间
            public const int DoorLockOpenDurationSeconds = 10;

            //dht11(温湿度传感器)关联的Gpio pin
            public const int dht11PinID = 12;
            public const int DhtSendValueDuration = 5;

            //远程控制的LED关联的Gpio Pin
            public const int testLedPinID =26;

            //火焰传感器关联的Gpio pin
            public const int fireAlarmPinID = 17;

            //人体传感器关联的Gpio pin
            public const int humanInfrarePinID = 6;
        }

        public static class EmergenceCounter
        {
            public const int SendEmergenceCounterDuration = 1;
        }
        
        public static class TimingCommand
        {
            public const int GetTimingCommandDuration = 5;
        }
    }
}
