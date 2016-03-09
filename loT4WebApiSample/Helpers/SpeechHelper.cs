using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;

namespace loT4WebApiSample.Helpers
{
    /// <summary>
    /// 语音合成，需要结合XAML的SpeechSynthesizer自动播放
    /// </summary>
    public class SpeechHelper
    {
        private MediaElement media;
        public SpeechHelper(MediaElement mediaElement)
        {
            media = mediaElement;
        }

        /// <summary>
        /// 本地语音合成
        /// </summary>
        /// <param name="message">需要合成的字符串</param>
        public async Task PlayTTS(string message)
        {
            try
            {
                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                if (media != null && synthesizer != null)
                {
                    var stream = await synthesizer.SynthesizeTextToStreamAsync(message);
                    media.AutoPlay = true;
                    media.SetSource(stream, stream.ContentType);
                    media.Play();
                }
            }
            catch
            {
                //txtMessage.Text = string.Format("Exception发生错误，错误原因：{0}", ex.Message);
            }
        }
    }
}
