﻿using System;
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
    public class SpeechHelper:IDisposable
    {
        private MediaElement media;
        private SpeechSynthesizer synthesizer;
        public SpeechHelper(MediaElement mediaElement)
        {
            mediaElement = media;
            synthesizer = new SpeechSynthesizer();
        }

        /// <summary>
        /// 本地语音合成
        /// </summary>
        /// <param name="message">需要合成的字符串</param>
        public async void PlayTTS(string message)
        {
            try
            {
                ResourceContext speechContext = ResourceContext.GetForCurrentView();
                speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };
                synthesizer = new SpeechSynthesizer();
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
            catch
            {
                //txtMessage.Text = string.Format("Exception发生错误，错误原因：{0}", ex.Message);
            }
        }

        public void Dispose()
        {
            synthesizer.Dispose();
        }
    }
}
