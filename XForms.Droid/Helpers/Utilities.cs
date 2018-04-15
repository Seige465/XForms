using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Widget;
using XForms.XForms;
using Android.Text;
using Android.Text.Util;
using Android.Text.Method;
using Android.Database;
using Android.Provider;
using Android.Support.Media;
using System.Collections.Generic;
using Android.Views;

namespace XForms.Droid.Helpers
{
    public static class Utilities
    {

        public static String[] attributes = new String[]
        {
            ExifInterface.TagDatetime,
            ExifInterface.TagDatetimeDigitized,
            ExifInterface.TagExposureTime,
            ExifInterface.TagFlash,
            ExifInterface.TagFocalLength,
            ExifInterface.TagGpsAltitude,
            ExifInterface.TagGpsAltitudeRef,
            ExifInterface.TagGpsDatestamp,
            ExifInterface.TagGpsLatitude,
            ExifInterface.TagGpsLatitudeRef,
            ExifInterface.TagGpsLongitude,
            ExifInterface.TagGpsLongitudeRef,
            ExifInterface.TagGpsProcessingMethod,
            ExifInterface.TagGpsTimestamp,
            ExifInterface.TagImageLength,
            ExifInterface.TagImageWidth,
            ExifInterface.TagMake,
            ExifInterface.TagModel,
            ExifInterface.TagOrientation,
            ExifInterface.TagSubsecTime,
            ExifInterface.TagSubsecTimeDigitized,
            ExifInterface.TagSubsecTimeOriginal,
            ExifInterface.TagWhiteBalance
        };

        public static bool HasConnection()
        {
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
                return true;
            return false;
        }

        public static void SetupHint(Activity context, string message)
        {
            context.RunOnUiThread(() => 
            {
                AlertDialog hintDialog = new AlertDialog.Builder(context).Create();
                hintDialog.SetTitle("Hint");

                TextView tv = new TextView(context);
                SpannableString s;
                EventHandler ev;


                var lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                lp.SetMargins(20, 20, 20, 20);
                tv.SetTextAppearance(Android.Resource.Style.TextAppearanceMaterialMedium);
                tv.MovementMethod = LinkMovementMethod.Instance;
                tv.SetPadding(50, 10, 50, 10);



                //Linkify.AddLinks(message, MatchOptions.WebUrls);
                tv.SetText(Html.FromHtml((string)message, FromHtmlOptions.ModeLegacy), TextView.BufferType.Spannable);
                tv.LayoutParameters = lp;
                hintDialog.SetView(tv);

                hintDialog.SetButton("OK", delegate { return; });
                hintDialog.Show();

            });
           
        }

        public static string GetRealPathFromURI(Android.Net.Uri contentURI, Context context)
        {
            string result;
            ICursor cursor = context.ContentResolver.Query(contentURI, null, null, null, null);
            if (cursor == null)
                result = contentURI.Path;
            else
            {
                cursor.MoveToFirst();
                int idx = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                result = cursor.GetString(idx);
                cursor.Close();
            }
            return result;

        }

        /// <summary>
        /// Obtains the EXIF data of the given URI.
        /// </summary>
        /// <param name="oldUri">The URI you wish to get the EXIF data</param>
        /// <param name="context">Your current context (Activity most likely)</param>
        /// <returns></returns>
        public static Dictionary<string,string> CopyExif(Android.Net.Uri oldUri, Context context)
        {
            //somewhere around here we get the exif data
            //photofile is the temp image

            Dictionary<string, string> ExifDict = new Dictionary<string, string>();

            var stream = context.ContentResolver.OpenInputStream(oldUri);

            ExifInterface oldExif = new ExifInterface(stream);

            foreach (string s in attributes)
            {
                string value = oldExif.GetAttribute(s);
                if (value != null)
                    ExifDict.Add(s, value);
                
            }
            return ExifDict;

        }

        public static ExifInterface SetupNewExif(Dictionary<string,string> exifDict, string path)
        {
            ExifInterface newExif = new ExifInterface(path);
            foreach (var item in exifDict)
            {
                newExif.SetAttribute(item.Key, item.Value);
            }
            newExif.SaveAttributes();
            return newExif;
        }


    }

    public class NetworkStatusMonitor
    {
        private NetworkState _state;

        public NetworkStatusMonitor()
        {
        }

        public NetworkState State
        {
            get
            {
                UpdateNetworkStatus();

                return _state;
            }
        }

        public void UpdateNetworkStatus()
        {
            _state = NetworkState.Unknown;

            try
            {
                // Retrieve the connectivity manager service
                var connectivityManager = (ConnectivityManager)
                    Application.Context.GetSystemService(
                        Context.ConnectivityService);

                // Check if the network is connected or connecting.
                // This means that it will be available,
                // or become available in a few seconds.
                var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                if (activeNetworkInfo == null)
                    _state = NetworkState.Disconnected;

                else if (activeNetworkInfo.IsConnectedOrConnecting)
                {
                    // Now that we know it's connected, determine if we're on WiFi or something else.
                    _state = activeNetworkInfo.Type == ConnectivityType.Wifi ?
                        NetworkState.ConnectedWifi : NetworkState.ConnectedData;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unable to get connection status.\n\n{ex.ToString()}");
            }
        }

        public enum NetworkState
        {
            Unknown,
            ConnectedWifi,
            ConnectedData,
            Disconnected
        }





    }

    [BroadcastReceiver()]
    [IntentFilter(new string[] { "android.net.conn.CONNECTIVITY_CHANGE" })]
    public class NetworkStatusBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler ConnectionStatusChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}