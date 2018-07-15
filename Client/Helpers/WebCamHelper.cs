using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace WebApiSample.Helpers
{
    public class WebCamHelper
    {
        public MediaCapture mediaCapture;

        private bool initialized = false;

        /// <summary>
        /// 异步初始化网络摄像头
        /// </summary>
        public async Task InitializeCameraAsync()
        {
            if (mediaCapture == null)
            {
                // 尝试发现摄像头
                var cameraDevice = await FindCameraDevice();

                if (cameraDevice == null)
                {
                    // 没有发现摄像头
                    Debug.WriteLine("No camera found!");
                    initialized = false;
                    return;
                }

                // Creates MediaCapture initialization settings with foudnd webcam device
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(settings);
                initialized = true;
            }
        }

        /// <summary>
        /// 异步寻找摄像头，如果没有找到，返回null，否则返回DeviceInfomation
        /// </summary>
        private static async Task<DeviceInformation> FindCameraDevice()
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);


            if (allVideoDevices.Count > 0)
            {
                // 如果发现，返回
                return allVideoDevices[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 开启摄像头预览
        /// </summary>
        public async Task StartCameraPreview()
        {
            try
            {
                await mediaCapture.StartPreviewAsync();
            }
            catch
            {
                initialized = false;
                Debug.WriteLine("Failed to start camera preview stream");
            }
        }

        /// <summary>
        /// 关闭摄像头预览
        /// </summary>
        public async Task StopCameraPreview()
        {
            try
            {
                await mediaCapture.StopPreviewAsync();
            }
            catch
            {
                Debug.WriteLine("Failed to stop camera preview stream");
            }
        }


        /// <summary>
        /// 拍摄照片，返回StorageFile，文件将被存储到临时文件夹
        /// </summary>
        public async Task<StorageFile> CapturePhoto()
        {
            // Create storage file in local app storage
            string fileName = GenerateNewFileName() + ".jpg";
            CreationCollisionOption collisionOption = CreationCollisionOption.GenerateUniqueName;
            StorageFile file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, collisionOption);

            // 拍摄并且存储
            await mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);

            return file;
        }

        /// <summary>
        /// 产生文件名称
        /// </summary>
        private string GenerateNewFileName()
        {
            return " Shaper" + DateTime.UtcNow.ToString("yyyy.MMM.dd HH-mm-ss");
        }

        /// <summary>
        /// 如果摄像头初始化成功，返回true，否则返回false
        /// </summary>
        public bool IsInitialized()
        {
            return initialized;
        }
    }
}
