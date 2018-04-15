using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Android.Support.V4.Content;

namespace XForms.Droid.Helpers
{
    public class UriUtil
    {
        public static string tempImageFileLocation;

        public static File CreateFileForPic(Context context)
        {
            String fileName = "IMG_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            File storageDir = context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);
            return new File(storageDir, fileName);
        }

        public static File CreateTempFileForPic(Context context)
        {
            String fileName = "IMG_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            File storageDir = context.GetExternalFilesDir("");
            File image = File.CreateTempFile(fileName, ".jpg", storageDir);
            tempImageFileLocation = image.AbsolutePath;
            return image;

        }

        public static Android.Net.Uri FromFile(Context context, File file)
        {
            string authority = $"{context.PackageName}.fileprovider";
            if (context == null || file == null)
                return null;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                return FileProvider.GetUriForFile(context, authority, file);
            else
                return Android.Net.Uri.FromFile(file);
        }

    }
}