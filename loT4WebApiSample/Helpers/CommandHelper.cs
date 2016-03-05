using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml;

namespace loT4WebApiSample.Helpers
{
    public class CommandHelper
    {
        private const int MOTOR_GPIO_PIN = 5; //Pin 29 in RPI2 - 6th from bottom left
        private const string EMPTY_RESPONSE = "No command in Queque";

        private DispatcherTimer queueMsgchecktimer;

        public CommandHelper()
        {
            try
            {
                sasToken = GetSASToken();
                if (!sasToken.Equals(""))
                {
                    readyToinittimer = true;
                    //LogInfoMsg("Successfully generated sastoken");
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //LogErrorMsg(String.Format("Failed to generate sastoken {0}", ex.Message));
                readyToinittimer = false;
            }

            try
            {
                //InitGPIO();
                readyToinittimer = true;
            }
            catch (Exception ex)
            {
                readyToinittimer = false;
                //LogErrorMsg(String.Format("Failed to Initialize GPIO pins. {0}", ex.Message));
            }

            if (readyToinittimer)
            {
                queueMsgchecktimer = new DispatcherTimer();
                queueMsgchecktimer.Interval = TimeSpan.FromMilliseconds(20000);
                queueMsgchecktimer.Tick += QueueMsgchecktimer_Tick;
                queueMsgchecktimer.Start();
            }
            else
            {
                //LogErrorMsg("Failed to initialize a critical component. Review the details above, fix it and restart the APP.");
            }
        }

        private string _sasToken;
        private string sasToken
        {
            set
            {
                _sasToken = value;
            }
            get
            {
                return _sasToken;
            }
        }
        private string serviceBaseAddress
        {
            get
            {
                //return "https://<<Your Azure Service Namespace>>.servicebus.windows.net/";
                return "Endpoint=sb://mengnan.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=oxxphyduxPkHN4Csuf9WHV9YjjaFESF0M+AkewjunwM=";
            }
        }

        private string fullServicebusaddress
        {
            get
            {
                return serviceBaseAddress + queueTopicName + fullAddresssufix;
            }
        }
        private string fullAddresssufix
        {
            get
            {
                return "/messages/head?timeout=15";
            }
        }

        private bool _readyToinittimer = false;
        private bool readyToinittimer
        {
            get
            {
                return _readyToinittimer;
            }
            set
            {
                _readyToinittimer = value;
            }
        }
        private string queueTopicName
        {
            get
            {
                return "queue topic name that you created";
            }
        }

        private string sasKeyname
        {
            get
            {
                return "RootManageSharedAccessKey";
            }
        }

        private string sasKeyvalue
        {
            get
            {
                return "RootManageSharedAccessKeyFromYourAzurePortal";
            }
        }

        private string _msgFromQuque = string.Empty;
        private string msgFromQueue
        {
            set
            {
                _msgFromQuque = value;
            }
            get
            {
                return _msgFromQuque;
            }
        }

        private async void QueueMsgchecktimer_Tick(object sender, object e)
        {
            try
            {
                //LogInfoMsg("Retrieving Queue msg initiated by QueueMsgchecktimer");
                await ReceiveAndDeleteMessageFromSubscription();
                //LogQueueMsg(msgFromQueue);

                if (!msgFromQueue.Equals(EMPTY_RESPONSE))
                {
                    if (msgFromQueue.Equals("ON"))
                    {
                        //SwitchMotor(true);
                    }
                    else
                    {
                        //SwitchMotor(false);
                    }
                }
            }
            catch (Exception ex)
            {
                //LogErrorMsg(ex.Message);
                if (ex.Message.ToLower().Contains("unauthorized"))
                {
                    sasToken = GetSASToken();
                }
            }
        }

        //private void InitGPIO()
        //{
        //    var gpio = GpioController.GetDefault();

        //    // Show an error if there is no GPIO controller
        //    if (gpio == null)
        //    {
        //        pin = null;
        //        LogErrorMsg("There is no GPIO controller on this device.");
        //        return;
        //    }

        //    pin = gpio.OpenPin(MOTOR_GPIO_PIN);

        //    // Show an error if the pin wasn't initialized properly
        //    if (pin == null)
        //    {
        //        LogErrorMsg("There were problems initializing the GPIO pin.");
        //        return;
        //    }

        //    pin.SetDriveMode(GpioPinDriveMode.Output);

        //    LogInfoMsg("GPIO pin initialized correctly.");
        //}


        private async Task SendMessage(string body)
        {
            string fullAddress = serviceBaseAddress + queueTopicName + "/messages" + "?timeout=60&api-version=2013-08 ";
            await SendViaHttp(body, HttpMethod.Post);
        }


        // Receives and deletes the next message from the given resource (queue, topic, or subscription)
        // using the resourceName and an HTTP DELETE request.
        private async Task<string> ReceiveAndDeleteMessageFromSubscription()
        {
            HttpResponseMessage response = await SendViaHttp(String.Empty, HttpMethod.Delete);

            if (response.IsSuccessStatusCode)
            {
                // we should have retrieved a message
                msgFromQueue = await response.Content.ReadAsStringAsync();
                if (msgFromQueue.Equals(string.Empty))
                {
                    msgFromQueue = EMPTY_RESPONSE;
                }
            }
            return msgFromQueue;
        }

        private async Task<HttpResponseMessage> SendViaHttp(string body, HttpMethod httpMethod)
        {
            HttpClient webClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(fullServicebusaddress),
                Method = httpMethod,
            };

            webClient.DefaultRequestHeaders.Add("Authorization", sasToken);
            request.Content = new StringContent(body);

            HttpResponseMessage response = await webClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = string.Format("{0} : {1}", response.StatusCode, response.ReasonPhrase);
                throw new Exception(error);
            }
            return response;
        }

        private string GetSASToken()
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + 720000);
            string stringToSign = WebUtility.UrlEncode(serviceBaseAddress) + "\n" + expiry;
            string hash = HmacSha256(sasKeyvalue, stringToSign);
            string sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
            WebUtility.UrlEncode(serviceBaseAddress), WebUtility.UrlEncode(hash), expiry, sasKeyname);
            return sasToken;
        }

        public string HmacSha256(string secretKey, string value)
        {
            // Move strings to buffers.
            var key = CryptographicBuffer.ConvertStringToBinary(secretKey, BinaryStringEncoding.Utf8);
            var msg = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);
            // Create HMAC.
            var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = objMacProv.CreateHash(key);
            hash.Append(msg);
            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
        }

        private async void btnReadMsg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //LogInfoMsg("Retrieving Queue msg initiated");
                msgFromQueue = await ReceiveAndDeleteMessageFromSubscription();
                //LogQueueMsg(msgFromQueue);
            }
            catch (Exception ex)
            {
                //LogErrorMsg(ex.Message);
            }
        }

        //private void LogQueueMsg(string queuemsg)
        //{
        //    Run myRun1 = new Run();
        //    myRun1.Foreground = bluevioletBrush;
        //    myRun1.Text = String.Format("Retrieved msg: {0} - {1}", queuemsg, DateTime.Now.ToString());

        //    // Create a paragraph and add the Run and Bold to it.
        //    Paragraph myParagraph = new Paragraph();
        //    myParagraph.Inlines.Add(myRun1);

        //    // Add the paragraph to the RichTextBox.
        //    rtbStatusMonitor.Blocks.Add(myParagraph);

        //    scrollrtbStatusMonitor.UpdateLayout();
        //    scrollrtbStatusMonitor.ChangeView(null, scrollrtbStatusMonitor.ScrollableHeight, null);
        //}

        //private void LogErrorMsg(string error)
        //{
        //    Run myRun1 = new Run();
        //    myRun1.Foreground = redBrush;
        //    myRun1.Text = String.Format("Error occurred: {0} - {1}", error, DateTime.Now.ToString());

        //    // Create a paragraph and add the Run and Bold to it.
        //    Paragraph myParagraph = new Paragraph();
        //    myParagraph.Inlines.Add(myRun1);

        //    // Add the paragraph to the RichTextBox.
        //    rtbStatusMonitor.Blocks.Add(myParagraph);

        //    scrollrtbStatusMonitor.UpdateLayout();
        //    scrollrtbStatusMonitor.ChangeView(null, scrollrtbStatusMonitor.ScrollableHeight, null);
        //}

        //private void LogInfoMsg(string info)
        //{
        //    Run myRun1 = new Run();
        //    myRun1.Foreground = blueBrush;
        //    myRun1.Text = String.Format("{0} - {1}", info, DateTime.Now.ToString());

        //    // Create a paragraph and add the Run and Bold to it.
        //    Paragraph myParagraph = new Paragraph();
        //    myParagraph.Inlines.Add(myRun1);

        //    // Add the paragraph to the RichTextBox.
        //    rtbStatusMonitor.Blocks.Add(myParagraph);

        //    scrollrtbStatusMonitor.UpdateLayout();
        //    scrollrtbStatusMonitor.ChangeView(null, scrollrtbStatusMonitor.ScrollableHeight, null);
        //}

        //private void SwitchMotor(bool motorstatuson)
        //{
        //    try
        //    {
        //        if (motorstatuson)
        //        {
        //            if (pin != null)
        //            {
        //                // to turn on the MOTOR, we need to push the pin 'low'
        //                pin.Write(GpioPinValue.Low);
        //                LED.Fill = greenBrush;

        //                GpioStatus.Text = "Relay is ON";
        //                LogInfoMsg("Relay is ON.");
        //            }
        //        }
        //        else
        //        {
        //            if (pin != null)
        //            {
        //                // to turn OFF the MOTOR, we need to push the pin 'HIGH'
        //                pin.Write(GpioPinValue.High);
        //                LED.Fill = redBrush;

        //                GpioStatus.Text = "Relay is OFF";
        //                LogInfoMsg("Relay is OFF.");
        //            }
        //            else
        //            {
        //                throw new Exception("GPIO pin is NULL");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogErrorMsg(ex.Message.ToString());
        //    }
        //}
    }
}
