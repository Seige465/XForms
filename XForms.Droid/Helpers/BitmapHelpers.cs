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
using Android.Graphics;
using Android.Media;
using Android.Database;
using Android.Provider;

namespace XForms.Droid.Helpers
{
    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height, Context context)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);            

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);


            //int orientation = ((BaseActivity)context).tempRotation;
            ExifInterface exif = new ExifInterface(fileName);
            int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);


            float angle = 0f;
            switch (orientation)
            {
                case 6:
                    angle = 90f;
                    break;
                case 3:
                    angle = 180f;
                    break;
                case 8:
                    angle = 270f;
                    break;
            }
            Matrix matrix = new Matrix();
            matrix.PostRotate(angle);
            resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, matrix, true);
            GC.Collect();
            return resizedBitmap;
        }

        public static int[] GetBitmapSize(this string fileName)
        {
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);
            int[] size = new int[2];
            size[0] = options.OutWidth;
            size[1] = options.OutHeight;
            return size;
        }


        public static Bitmap RotateBitmap(Bitmap source, float angle)
        {
            Matrix matrix = new Matrix();
            matrix.PostRotate(angle);
            Bitmap bm = Bitmap.CreateBitmap(source, 0, 0, source.Width, source.Height, matrix, true);
            source.Recycle();
            return bm;
            
        }
    }
}