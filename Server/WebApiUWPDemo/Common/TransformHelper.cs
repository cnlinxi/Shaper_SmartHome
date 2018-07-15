using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml.Media.Imaging;

namespace WebApiUWPDemo.Common
{
    public static class TransformHelper
    {
        public static async Task<FileUpdateStatus> SaveToJPGImage(WriteableBitmap bitmap, PickerLocationId location, string fileName)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = location
            };
            savePicker.FileTypeChoices.Add("JPG Image", new[] { ".jpg" });
            savePicker.SuggestedFileName = fileName;
            StorageFile sFile = await savePicker.PickSaveFileAsync();
            if (sFile != null)
            {
                CachedFileManager.DeferUpdates(sFile);


                using (var fileStream = await sFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                    Stream pixelStream = bitmap.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                              (uint)bitmap.PixelWidth,
                              (uint)bitmap.PixelHeight,
                              96.0,
                              96.0,
                              pixels);
                    await encoder.FlushAsync();
                }


                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(sFile);
                return status;
            }
            return FileUpdateStatus.Failed;
        }
    }
}
