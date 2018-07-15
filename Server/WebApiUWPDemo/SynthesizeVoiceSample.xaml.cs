using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiUWPDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SynthesizeVoiceSample : Page
    {
        //HttpClient httpClient;
        //const string uriSynthesizeVoice = "https://speech.platform.bing.com/synthesize";

        //private AudioGraph graph;
        //private AudioDeviceOutputNode deviceOutput;
        public SynthesizeVoiceSample()
        {
            this.InitializeComponent();
            //httpClient = new HttpClient();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //if(httpClient!=null)
            //{
            //    httpClient.Dispose();
            //}
            //if(graph!=null)
            //{
            //    graph.Dispose();
            //}

            base.OnNavigatedFrom(e);
        }

        private void btnSynthesizeVoice_Click(object sender, RoutedEventArgs e)
        {
            if(txtInput.Text.Length>0)
            {
                PlayTTS(txtInput.Text);
            }
        }

        /// <summary>
        /// 本地语音合成
        /// </summary>
        /// <param name="message">需要合成的字符串</param>
        private async void PlayTTS(string message)
        {
            try
            {
                ResourceContext speechContext = ResourceContext.GetForCurrentView();
                speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };
                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                var voices = SpeechSynthesizer.AllVoices;
                if (voices == null) return;
                VoiceInformation currentVoice = synthesizer.Voice;
                VoiceInformation voice = null;
                foreach (VoiceInformation item in voices.OrderBy(p => p.Language))
                {
                    string tag = item.Language;
                    if (tag.Equals(speechContext.Languages[0]))
                    {
                        voice = item;
                    }
                }
                if (null != voice)
                {
                    synthesizer.Voice = voice;
                    SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(message);

                    media.AutoPlay = true;
                    media.SetSource(synthesisStream, synthesisStream.ContentType);
                    media.Play();
                }
            }
            catch (Exception ex)
            {
                txtMessage.Text = string.Format("Exception发生错误，错误原因：{0}", ex.Message);
            }

        }

        #region 2016年2月15日21:56:28 使用牛津计划网络请求语音合成（未测试）
        //用例：byte[] bytes=await SynthesizeVoice("hello");PlayVoice(bytes);

        /// <summary>
        /// 使用MediaElement播放语音
        /// </summary>
        /// <param name="byteData">字节数组</param>
        private async void PlayVoice(byte[] byteData)
        {
            using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(byteData.AsBuffer());
                stream.Seek(0);//确保流在第0位
                media.AutoPlay = true;
                media.SetSource(stream, "");
                media.Play();
            }
        }

        /// <summary>
        /// 使用牛津计划合成语音
        /// </summary>
        /// <param name="strText">要合成的字符串</param>
        /// <returns>合成后的语音字节数组</returns>
        //private async Task<byte[]> SynthesizeVoice(string strText)
        //{
        //    httpClient.DefaultRequestHeaders.Add("X-Microsoft-OutputFormat", "riff-8khz-8bit-mono-mulaw");

        //    string strContent = 
        //        string.Format("<speak version='1.0' xml:lang='zh-CN'><voice xml:lang='zh-CN' xml:gender='Female' name='Microsoft Server Speech Text to Speech Voice (zh-CN, Yaoyao, Apollo)'>{0}</voice></speak>", strText);

        //    byte[] byteData = System.Text.Encoding.UTF8.GetBytes(strContent);

        //    try
        //    {
        //        using (var content = new ByteArrayContent(byteData))
        //        {
        //            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        //            HttpResponseMessage response = await httpClient.PostAsync(uriSynthesizeVoice, content);
        //            if(response.StatusCode==System.Net.HttpStatusCode.OK)
        //            {
        //                byte[] byteResponse = await response.Content.ReadAsByteArrayAsync();
        //                return byteResponse;
        //            }
        //        }
        //        return null;
        //    }
        //    catch(Exception ex)
        //    {
        //        txtMessage.Text = "Exception请求失败！失败原因：" + ex.Message;
        //        return null;
        //    }
        //}
        #endregion
    }
}
