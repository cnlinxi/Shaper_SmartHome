using Sensors.Dht;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace loT4WebApiSample.Helpers
{
    public class GpioHelper
    {
        private GpioController gpioController;
        private GpioPin doorbellPin;
        private GpioPin doorlockPin;
        private GpioPin dht11Pin;
        private GpioPin testLedPin;
        private GpioPin fireAlarmPin;
        private GpioPin humanInfrarePin;

        private IDht dht;

        /// <summary>
        /// 初始化Gpio
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            gpioController = GpioController.GetDefault();
            if (gpioController == null)
                return false;

            //门锁及门铃初始化
            //doorbellPin = gpioController.OpenPin(Constants.GpioConstants.doorbellPinID);
            //if(doorbellPin==null)
            //{
            //    return false;
            //}
            //doorbellPin.DebounceTimeout = TimeSpan.FromSeconds(25);
            //if(doorbellPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            //{
            //    doorbellPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            //}

            doorlockPin = gpioController.OpenPin(Constants.GpioConstants.doorlockPinID);
            if (doorlockPin == null)
                return false;
            doorlockPin.SetDriveMode(GpioPinDriveMode.Output);
            doorlockPin.Write(GpioPinValue.High);//输入高电压，关闭门锁

            //dht11（温湿度传感器）初始化
            dht11Pin = gpioController.OpenPin(Constants.GpioConstants.dht11PinID, GpioSharingMode.Exclusive);
            if (dht11Pin == null)
                return false;
            dht = new Dht11(dht11Pin, GpioPinDriveMode.Input);
            if (dht == null)
                return false;

            //远程控制示例的LED灯初始化
            testLedPin = gpioController.OpenPin(Constants.GpioConstants.testLedPinID);
            if (testLedPin == null)
                return false;
            testLedPin.SetDriveMode(GpioPinDriveMode.Output);
            testLedPin.Write(GpioPinValue.High);//输入高电压，初始关闭状态

            //火焰传感器初始化
            fireAlarmPin = gpioController.OpenPin(Constants.GpioConstants.fireAlarmPinID);
            if (fireAlarmPin == null)
                return false;
            if(fireAlarmPin.IsDriveModeSupported(GpioPinDriveMode.InputPullDown))
            {
                fireAlarmPin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            }

            //人体传感器初始化
            humanInfrarePin = gpioController.OpenPin(Constants.GpioConstants.humanInfrarePinID);
            if (humanInfrarePin == null)
                return false;
            //humanInfrarePin.DebounceTimeout = TimeSpan.FromSeconds(25);
            if(humanInfrarePin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                humanInfrarePin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            }
            
            return true;
        }

        public GpioPin GetDoorBellPin()
        {
            return doorbellPin;
        }

        public GpioPin GetFireAlarm()
        {
            return fireAlarmPin;
        }

        public GpioPin GetHumanInfrare()
        {
            return humanInfrarePin;
        }

        public IDht GetDht()
        {
            return dht;
        }

        /// <summary>
        /// 打开门锁，并保持一段时间
        /// </summary>
        public async void UnlockDoor()
        {
            //写入低电压，以打开门锁
            doorlockPin.Write(GpioPinValue.Low);
            await Task.Delay(TimeSpan.FromSeconds(Constants.GpioConstants.DoorLockOpenDurationSeconds));
            doorlockPin.Write(GpioPinValue.High);
        }

        /// <summary>
        /// 打开用于远程控制的LED小灯
        /// </summary>
        public void OnTestLED()
        {
            testLedPin.Write(GpioPinValue.Low);
        }

        /// <summary>
        /// 关闭用于远程控制的LED小灯
        /// </summary>
        public void OffTestLED()
        {
            testLedPin.Write(GpioPinValue.High);
        }
    }
}
