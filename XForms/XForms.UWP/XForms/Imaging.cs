using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;

namespace XForms.UWP.XForms
{
    public static class Imaging
    {
        public async static Task<StorageFile> ShowImageLoadDialog()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
                return null;
            file = await file.CopyAsync(storageFolder);
            await file.RenameAsync(Guid.NewGuid().ToString() + file.FileType);
            return file;
        }
        public async static Task<StorageFile> ShowCameraDialog()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (photo == null)
                return null;            
            await photo.MoveAsync(storageFolder);
            return photo;
        }
    }
}
