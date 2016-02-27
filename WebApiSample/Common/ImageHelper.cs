using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WebApiSample.Common
{
    public class ImageHelper
    {
        public static async Task<StorageFile> TakePhoto()
        {
            CameraCaptureUI cameraUi = new CameraCaptureUI();
            cameraUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

            StorageFile photo = await cameraUi.CaptureFileAsync(CameraCaptureUIMode.Photo);
            return photo;
            //if (photo != null)
            //{
            //    IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            //    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            //    SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
            //                                                                                                        BitmapPixelFormat.Bgra8,
            // SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            //await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
            //imageFace.Source = bitmapSource;
            //}
        }

        public static async Task<StorageFile> PickImageFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                return file;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static async Task<Dictionary<string,StorageFile>> PickImageFileAndType()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                Dictionary<string, StorageFile> result = new Dictionary<string, StorageFile>();
                result.Add(file.FileType, file);
                return result;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
