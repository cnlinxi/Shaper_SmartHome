using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WebApiSample.FaceRecognizatioin;
using WebApiSample.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace WebApiSample
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EmotionDetect : Page
    {
        private WebCamHelper camera;
        private SpeechHelper speech;

        const int sumEmotion = 10;

        string[] chEmotions = { "怒", "蔑视", "厌恶", "恐惧", "乐", "没表情", "哀","惊讶" };
        string[] enEmotions = { "anger", "contempt", "disgust", "fear", "happiness", "neutral", "sadness", "surprise"};

        private int count = 0;
        double score = 0;

        DispatcherTimer timer;

        public class SpeechContent
        {
            public const string Gretting = "准备好了没？现在要开始了";
            public const string PreStart = "预备";
            public const string Start = "开始";
            public const string Stop = "游戏结束";
            public static string SpeechGameSuccess(int score)
            {
                return "主人，你好棒，竟然得了" + score + "分，表情帝";
            }
            public static string SpeechGameFailure(int score)
            {
                return "主人，你只得到了" + score + "分";
            }
        }
        public EmotionDetect()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void mediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (speech == null)
            {
                speech = new SpeechHelper(mediaElement);
                await speech.PlayTTS(SpeechContent.Gretting);
            }
            else
            {
                mediaElement.AutoPlay = false;
            }
        }

        private async void cameraElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (camera == null || !camera.IsInitialized())
            {
                camera = new WebCamHelper();
                await camera.InitializeCameraAsync();

                cameraElement.Source = camera.mediaCapture;

                if (cameraElement.Source != null)
                {
                    await camera.StartCameraPreview();
                }
                else if (camera.IsInitialized())
                {
                    cameraElement.Source = camera.mediaCapture;
                    if (cameraElement.Source != null)
                    {
                        await camera.StartCameraPreview();
                    }
                }
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (camera == null || !camera.IsInitialized() || speech == null)
                return;
            await speech.PlayTTS(SpeechContent.PreStart);
            await Task.Delay(1000);

            btnStart.Visibility = Visibility.Collapsed;
            tbEmotionTip.Visibility = Visibility.Visible;

            Random random = new Random();
            StorageFile file;
            int index = 0;
            FaceApiHelper faceApi = new FaceApiHelper();
            KeyValuePair<string, double> detectedEmotion;
            for(int i=0;i<sumEmotion;++i)
            {
                index = random.Next(0, 7);
                tbEmotionTip.Text = chEmotions[index];
                await speech.PlayTTS(chEmotions[index]);
                await Task.Delay(1000);
                file = await camera.CapturePhoto();
                detectedEmotion = await faceApi.EmotionDetection(file);
                if (detectedEmotion.Key.Equals(enEmotions[index]))
                {
                    score += detectedEmotion.Value * 10;
                }
                else if (detectedEmotion.Key.Equals("neutral"))
                {
                    score += 5;
                }
            }

            tbSocre.Text = "本次得分：" + ((int)score + 1).ToString();
            await speech.PlayTTS(SpeechContent.Stop);
            if (score >= 60)
            {
                await speech.PlayTTS(SpeechContent.SpeechGameSuccess((int)score + 1));
            }
            else
            {
                await speech.PlayTTS(SpeechContent.SpeechGameFailure((int)score + 1));
            }

            btnStart.Visibility = Visibility.Visible;
            tbEmotionTip.Visibility = Visibility.Collapsed;
        }

        private async void Timer_Tick(object sender, object e)
        {
            //Thread thread = new Thread();
            if(count==0)
            {
                btnStart.Visibility = Visibility.Collapsed;
                tbEmotionTip.Visibility = Visibility.Visible;
            }
            Random random = new Random();
            StorageFile file;
            int index = 0;
            FaceApiHelper faceApi = new FaceApiHelper();
            KeyValuePair<string, double> detectedEmotion;
            index = random.Next(0, 7);
            tbEmotionTip.Text = chEmotions[index];
            await speech.PlayTTS(chEmotions[index]);
            file = await camera.CapturePhoto();
            detectedEmotion = await faceApi.EmotionDetection(file);
            if (detectedEmotion.Key.Equals(enEmotions[index]))
            {
                score += detectedEmotion.Value * 10;
            }
            else if(detectedEmotion.Key.Equals("neutral"))
            {
                score += 5;
            }

            if (++count >= sumEmotion && timer != null)
            {
                timer.Stop();

                tbSocre.Text = "本次得分：" + ((int)score + 1).ToString();
                await speech.PlayTTS(SpeechContent.Stop);
                if (score >= 60)
                {
                    await speech.PlayTTS(SpeechContent.SpeechGameSuccess((int)score + 1));
                }
                else
                {
                    await speech.PlayTTS(SpeechContent.SpeechGameFailure((int)score + 1));
                }

                btnStart.Visibility = Visibility.Visible;
                tbEmotionTip.Visibility = Visibility.Collapsed;
            }
        }
    }
}
