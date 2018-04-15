using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using System;
using Android.Runtime;
using Android.Graphics;
using XForms.XForms;
using Android.Widget;
using XForms.Droid.Helpers;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using static Android.Gms.Maps.GoogleMap;
using Android.Locations;
using Android.Util;
using System.Threading;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Net;
using System.Collections.Generic;
using System.IO;
using Android.Support.Media;
using Android.Graphics.Drawables;

namespace XForms.Droid
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseActivity : Activity, IOnMapReadyCallback, View.IOnTouchListener, 
        GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        ScrollFormActivity _formActivity;

        //camera intent
        const int REQUEST_IMAGE_CAPTURE = 1;
        const int REQUEST_GALLERY_IMAGE = 2;
        public int REQUEST_DRAWING = 3;
        public string filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
        Android.Net.Uri TempUri;
        Java.IO.File _file;
        string imagePath = "";
        Bindings bindingForImage;
        string fileName;
        View imageViewRow;
        Android.Net.Uri photoUri;

        public int tempRotation = 0;

        bool showDialogAfterImageSelect;

        //permission constants
        public const int REQUEST_CAMERA_PERMISSION = 1;
        public const int REQUEST_READ_WRITE_PERMISSION = 2;
        public const int REQUEST_LOCATION_PERMISSION = 3;


        ISharedPreferences prefs;
        ISharedPreferencesEditor editor;

        //for seeing what network state we are in
        NetworkStatusBroadcastReceiver _nsbr;
        NetworkStatusMonitor nsm;

        //keyboard stuff
        public bool keyboardVisible;
        public bool touchEventsSetup = false;


        //map widget stuff
        public EventHandler LatLngChanged;
        private void OnLatLngChanged()
        {
            LatLngChanged?.Invoke(this, EventArgs.Empty);
        }
        public LatLng mapLatLng;
        public LatLng Lat_Lng
        {
            get
            {
                return mapLatLng;
            }
            set
            {
                mapLatLng = value;
                OnLatLngChanged();
            }
        }
        public GoogleMap gmap;
        bool locationCenter = false;
        MarkerOptions mOptions;

        //location listener
        bool _isGooglePlayServicesInstalled;
        public GoogleApiClient apiClient;
        LocationRequest locRequest;
        Location _location;
        LocationManager lm;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            touchEventsSetup = false;

            ThreadPool.SetMaxThreads(10, 10);

            DesignHelper.SetBar(this, Title, Title, 0);


            //MainActivity is our launch activity
            if (!(this is MainActivity))
            {
                ActionBar.SetDisplayHomeAsUpEnabled(true);
            }

            prefs = GetSharedPreferences("settings", FileCreationMode.Private);
            editor = prefs.Edit();

            
            _isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            if (_isGooglePlayServicesInstalled)
            {
                apiClient = new GoogleApiClient.Builder(this, this, this)
                    .AddApi(LocationServices.API).Build();

                locRequest = new LocationRequest();
            }
            else
            {
                Console.WriteLine($"Google Play Services is not installed");
                Toast.MakeText(this, "Google Play Services is not installed", ToastLength.Long).Show();
            }




        }

        #region network monitoring
        private void _nsbr_ConnectionStatusChanged(object sender, EventArgs e)
        {
            nsm.UpdateNetworkStatus();

            switch (nsm.State)
            {
                case NetworkStatusMonitor.NetworkState.Unknown:
                    break;
                case NetworkStatusMonitor.NetworkState.ConnectedWifi:
                    editor.PutString("state", "WiFi");
                    editor.PutBoolean("online", true);
                    break;
                case NetworkStatusMonitor.NetworkState.ConnectedData:
                    editor.PutString("state", "Mobile Data");
                    editor.PutBoolean("online", true);
                    break;
                case NetworkStatusMonitor.NetworkState.Disconnected:
                    editor.PutString("state", "Offline");
                    editor.PutBoolean("online", false);
                    break;
                default:
                    break;

            }
            editor.Apply();
            //redraw menu
            InvalidateOptionsMenu();
        }

        public void KillReceiver()
        {
            if (_nsbr == null)
            {
                throw new InvalidOperationException("Network status monitoring not active");
            }
            try
            {
                UnregisterReceiver(_nsbr);
                _nsbr.ConnectionStatusChanged -= _nsbr_ConnectionStatusChanged;
                _nsbr = null;

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }
        #endregion

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //connection state menu
            MenuInflater.Inflate(Resource.Menu.menu_connection_status, menu);
            //if online
            if (prefs.GetBoolean("online", false))
            {
                menu.GetItem(1).SetVisible(false);
                menu.GetItem(0).SetTitle(prefs.GetString("state", "Online"));
                menu.GetItem(0).SetVisible(true);
            }
            else
            {
                menu.GetItem(0).SetVisible(false);
                menu.GetItem(1).SetVisible(true);
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case REQUEST_CAMERA_PERMISSION:
                    if (grantResults[0] == Permission.Granted)
                    {
                        editor.PutBoolean("camera_permission", true);

                        ContinueToImageSelect();

                        
                    }

                    break;
                case REQUEST_READ_WRITE_PERMISSION:
                    if (grantResults[0] == Permission.Granted)
                    {
                        editor.PutBoolean("read_write_permission", true);                      
                    }

                    break;
                case REQUEST_LOCATION_PERMISSION:
                    if (grantResults[0] == Permission.Granted)
                    {
                        editor.PutBoolean("location_permission", true);


                        apiClient.Connect();
                        if(gmap != null)
                            gmap.MyLocationEnabled = true;
                    }
                    break;
            }
            editor.Apply();
        }

        /**
         * New to Android 7+, requirement based off of link involving new storage stuff, sharing temp URIs across apps
         * https://developer.android.com/training/camera/photobasics.html#TaskScalePhoto
         **/
        public void TakePhoto(string name, Bindings binding, View imageRow, ScrollFormActivity formActivity, bool showDialogAfter)
        {

            fileName = name;
            bindingForImage = binding;
            imageViewRow = imageRow;
            _formActivity = formActivity;

            showDialogAfterImageSelect = showDialogAfter;

            //marshmallow (Android 6) and up require runtime permissions
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                ActivityCompat.RequestPermissions(this,
                    new string[] { Android.Manifest.Permission.Camera, Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.WriteExternalStorage },
                    REQUEST_CAMERA_PERMISSION);
            }
            else
            {
                editor.PutBoolean("camera_permission", true);
                editor.Apply();
                ContinueToImageSelect();
            }

        }

        Java.IO.File photoFile;
        public void ContinueToImageSelect()
        {
            
            try
            {
                photoFile = CreateImageFile();
            }
            catch(IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            if(photoFile != null)
            {
                photoUri = FileProvider.GetUriForFile(this, $"{PackageName}.fileprovider", photoFile);

                
                AlertDialog imagePicker = new AlertDialog.Builder(this).Create();
                imagePicker.SetTitle("Choose Image or Take Photo");
                imagePicker.SetButton("Take Photo", delegate
                {
                    Intent captureIntent = new Intent(MediaStore.ActionImageCapture);
                    captureIntent.PutExtra(MediaStore.ExtraOutput, photoUri);
                    StartActivityForResult(captureIntent, REQUEST_IMAGE_CAPTURE);
                });
                imagePicker.SetButton2("Gallery", delegate
                {
                    /*
                    Intent galleryIntent = new Intent(Intent.ActionGetContent);
                    galleryIntent.SetType("image/*");
                    galleryIntent.PutExtra(MediaStore.ExtraOutput, photoUri);
                    StartActivityForResult(galleryIntent, REQUEST_GALLERY_IMAGE);
                    */
                    List<Intent> galleryIntents = new List<Intent>();
                    Intent galleryIntent = new Intent(Intent.ActionGetContent);
                    galleryIntent.SetType("image/*");
                    IList<ResolveInfo> listGallery = PackageManager.QueryIntentActivities(galleryIntent, 0);


                    foreach (ResolveInfo res in listGallery)
                    {
                        string packageName = res.ActivityInfo.PackageName;
                        Intent intent = new Intent(galleryIntent);
                        intent.SetComponent(new ComponentName(packageName, res.ActivityInfo.Name));
                        intent.SetPackage(packageName);
                        intent.PutExtra(MediaStore.ExtraOutput, photoUri);
                        galleryIntents.Add(intent);
                    }

                    Intent chooserIntent = Intent.CreateChooser(galleryIntent, "Select image from Gallery");
                    //add gallery options
                    chooserIntent.PutExtra(Intent.ExtraInitialIntents, galleryIntents.ToArray());

                    StartActivityForResult(chooserIntent, REQUEST_GALLERY_IMAGE); 

                });

                imagePicker.Show();
                imagePicker.DismissEvent += delegate
                {
                    File.Delete(photoFile.Path);
                };
            }

        }

        string mCurrentPhotoPath;
        private Java.IO.File CreateImageFile()
        {
            string timestamp = DateTime.Now.ToString("DDMMYYYY_hhmmss");
            string imageFileName = fileName + timestamp;

            Java.IO.File storageDir = GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);
            Java.IO.File image = Java.IO.File.CreateTempFile(imageFileName, ".jpg", storageDir);

            mCurrentPhotoPath = image.AbsolutePath;
            return image;

        }

        private void GalleryAddPic()
        {
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Java.IO.File f = new Java.IO.File(mCurrentPhotoPath);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(f);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);
        }

        private void SetPic(ImageView image, Android.Net.Uri uri)
        {
            int targetW = image.Width;
            int targetH = image.Height;

            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true
            };
            BitmapFactory.DecodeFile(mCurrentPhotoPath, options);

            int outWidth = options.OutWidth;
            int outHeight = options.OutHeight;
            int inSampleSize = 1;

            //int scalefactor = Math.Min(photoW / targetW, photoH / targetH);

            int width = 100;
            int height = 100;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            options.InPurgeable = true;

            //get temp photo uri exif
            Dictionary<string,string> tempExifDict = Utilities.CopyExif(uri, this);

            Bitmap full = BitmapFactory.DecodeFile(mCurrentPhotoPath);
            using (var os = new FileStream(mCurrentPhotoPath, FileMode.Create))
            {
                full.Compress(Bitmap.CompressFormat.Jpeg, 70, os);
            }
            //full = null;
            GC.Collect();

            ExifInterface exif = Utilities.SetupNewExif(tempExifDict, mCurrentPhotoPath);
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
            full = Bitmap.CreateBitmap(full, 0, 0, full.Width, full.Height, matrix, true);

            //Bitmap bm = BitmapFactory.DecodeFile(mCurrentPhotoPath, options);
            image.SetImageBitmap(full);
        }

        //handle input of photos, either from gallery or camera
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            string path = "";
            Bitmap pic;
            ImageView iv = null;
            if (imageViewRow != null)
                iv = (ImageView)imageViewRow.FindViewWithTag("image");

            //user cancelled
            if(resultCode == Result.Canceled && requestCode != REQUEST_DRAWING)
            {
                File.Delete(photoFile.Path);
                return;
            }
            //camera
            if(requestCode == REQUEST_IMAGE_CAPTURE)
            {
                //

                GalleryAddPic();


                if (this is DrawingActivity)
                    SetPic(DrawingActivity.BackgroundImage, photoUri);
                else
                    SetPic(iv, photoUri);

                path = mCurrentPhotoPath;
            }

            //gallery
            else if (requestCode == REQUEST_GALLERY_IMAGE)
            {

                pic = MediaStore.Images.Media.GetBitmap(ContentResolver,data.Data);
                using (var os = new FileStream(mCurrentPhotoPath, FileMode.Create))
                {
                    pic.Compress(Bitmap.CompressFormat.Jpeg, 100, os);
                }

                path = mCurrentPhotoPath;

                if (this is DrawingActivity)
                    SetPic(DrawingActivity.BackgroundImage, data.Data);
                else
                    SetPic(iv, data.Data);
                

                //iv.SetImageBitmap(imageBitmap);

            }
            //this only has a value when coming directly from the camera/gallery
            if(bindingForImage != null)
                XForm.SetValue(bindingForImage.nodeset, path);

            //coming back from a canvas
            if (requestCode == REQUEST_DRAWING && data != null)
            {
                iv = (ImageView)WidgetHelper.viewToPass.FindViewWithTag("image");
                iv.SetImageBitmap(BitmapHelpers.LoadAndResizeBitmap(data.Data.Path, 100, 100, this));
                //iv.SetImageBitmap(Bitmap.CreateScaledBitmap(BitmapFactory.DecodeFile(data.Data.Path), 100, 100, true));
            }

            if(this is ScrollFormActivity)
            {
                try
                {             
                    _formActivity.FireFormChangeEvent(imageViewRow);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                if (showDialogAfterImageSelect)
                {
                    iv.PerformClick();
                }

            }

        }


        #region animations
        public void ViewAnimation(View v, bool expand)
        {
            Animation animation;
            if(expand)
                animation = AnimationUtils.LoadAnimation(this, Resource.Animation.AnimateExpand);
            else
                animation = AnimationUtils.LoadAnimation(this, Resource.Animation.AnimateContract);

            
            v.Animation = animation;
            animation.StartOffset = 500;
            animation.Duration = 500;
            animation.Start();

            v.Animation.AnimationStart += delegate
            {
                if (expand)
                {
                    v.Visibility = ViewStates.Visible;
                }
            };

            v.Animation.AnimationEnd += delegate
            {
                if (!expand){
                    v.Visibility = ViewStates.Invisible;
                }
                    
            };

        }
        #endregion



        protected override void OnResume()
        {
            base.OnResume();

            nsm = new NetworkStatusMonitor();
            _nsbr = new NetworkStatusBroadcastReceiver();
            _nsbr.ConnectionStatusChanged += _nsbr_ConnectionStatusChanged;

            RegisterReceiver(_nsbr, new IntentFilter(ConnectivityManager.ConnectivityAction));

            //we have an image to place somewhere :D to the view provided earlier, if this isn't null
            if (!string.IsNullOrEmpty(imagePath))
            {

            }

        }

        protected override async void OnPause()
        {
            base.OnPause();
            KillReceiver();
            if (apiClient.IsConnected)
            {
                await LocationServices.FusedLocationApi.RemoveLocationUpdates(apiClient, this);
                apiClient.Disconnect();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #region Map interface
        public void OnMapReady(GoogleMap googleMap)
        {
            gmap = googleMap;
            googleMap.MapType = MapTypeHybrid;
            locationCenter = false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                ActivityCompat.RequestPermissions(this, 
                    new string[] {
                        Android.Manifest.Permission.AccessCoarseLocation,
                        Android.Manifest.Permission.AccessFineLocation
                    }, 
                    REQUEST_LOCATION_PERMISSION);
            }
            else
            {
                editor.PutBoolean("location_permission", true);
                editor.Apply();
                apiClient.Connect();


                //gmap.SetOnMyLocationChangeListener(this);
                gmap.MyLocationEnabled = true;
            }

            mOptions = new MarkerOptions();
            //mOptions.Anchor(0, 0);

            gmap.MarkerDragEnd += Gmap_MarkerDragEnd;
            gmap.MarkerDragStart += Gmap_MarkerDragStart;


            mOptions.Draggable(true);

            //animate over mbs if we has no permission for location
            if (!prefs.GetBoolean("location_permission", false))
            {
                CameraUpdate cu = CameraUpdateFactory.NewLatLngZoom(new LatLng(-40.226091, 175.566035), 12 );
                googleMap.AnimateCamera(cu);
            }
            
            
            if (Lat_Lng != null)
            {
                mOptions.SetPosition(Lat_Lng);
                mOptions.SetTitle("Placed Marker");
                googleMap.AddMarker(mOptions);
                CameraUpdate cu = CameraUpdateFactory.NewLatLngZoom(new LatLng(mOptions.Position.Latitude, mOptions.Position.Longitude), 17);
                googleMap.AnimateCamera(cu);

            }
            //default marker
            else
            {
                mOptions.SetPosition(new LatLng(-40.226091, 175.566035));
                mOptions.SetTitle("Placed Marker");
                googleMap.AddMarker(mOptions);
                CameraUpdate cu = CameraUpdateFactory.NewLatLngZoom(new LatLng(mOptions.Position.Latitude, mOptions.Position.Longitude), 17);
                googleMap.AnimateCamera(cu);
                Lat_Lng = new LatLng(mOptions.Position.Latitude, mOptions.Position.Longitude);

            }



            googleMap.MapClick += (s, e) =>
            {
                //pre emptively clear map
                googleMap.Clear();
                Lat_Lng = new LatLng(e.Point.Latitude, e.Point.Longitude);
                mOptions.SetPosition(Lat_Lng);
                mOptions.SetTitle("Placed Marker");
                googleMap.AddMarker(mOptions);

                System.Console.WriteLine($"marker placed: {Lat_Lng.Latitude},{Lat_Lng.Longitude}");
            };
        }

        public void MarkWithGPS()
        {
            Lat_Lng = new LatLng(_location.Latitude, _location.Longitude);
            mOptions.SetPosition(Lat_Lng);
            mOptions.SetTitle("Placed Marker");

            gmap.AddMarker(mOptions);
            //CameraUpdate cu = CameraUpdateFactory.NewLatLngZoom(new LatLng(gmap.MyLocation.Latitude, gmap.MyLocation.Longitude), 14);
           // gmap.AnimateCamera(cu);


        }
        private void Gmap_MarkerDragStart(object sender, MarkerDragStartEventArgs e)
        {
            //e.Marker.SetAnchor(0, 0);
            e.Marker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
        }
       
        private void Gmap_MarkerDragEnd(object sender, MarkerDragEndEventArgs e)
        {
            //e.Marker.SetAnchor(0.5f, 1);
            e.Marker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            Lat_Lng = new LatLng(e.Marker.Position.Latitude, e.Marker.Position.Longitude);

        }
        
        #endregion
        

        #region Keyboard dismissal

        public void Iterate(View view)
        {
            if (view is ViewGroup)
                IterateViewChildren(view);
        }

        private void IterateViewChildren(View view)
        {
            if (view is ViewGroup)
            {
                view.SetOnTouchListener(this);
                ViewGroup vGroup = (ViewGroup)view;
                for (int i = 0; i < vGroup.ChildCount; i++)
                {
                    View vChild = vGroup.GetChildAt(i);
                    IterateViewChildren(vChild);
                }
            }

            //tap event to dismiss keyboard
            else if (!(view is EditText))
                view.SetOnTouchListener(this);            

        }

        public void HideKeyboardOnTap(Activity activity)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(activity.CurrentFocus.WindowToken, HideSoftInputFlags.None);
            // keyboardVisible = false;
        }

        public void ShowKeyboard(Activity activity, View view)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            inputMethodManager.ShowSoftInput(view, ShowFlags.Forced);
            keyboardVisible = true;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            View focus = CurrentFocus;
            if (focus != null)
            {
                HideKeyboardOnTap(this);
            }
                       
            return false;
        }
        #endregion

        #region Location Listener API

        public void OnConnected(Bundle connectionHint)
        {
            Console.WriteLine("Connected to client");
            locRequest.SetPriority(100);

            locRequest.SetFastestInterval(500);
            locRequest.SetInterval(1000);

            LocationServices.FusedLocationApi.RequestLocationUpdates(apiClient, locRequest, this);
        }

        public async void OnConnectionSuspended(int cause)
        {
            Console.WriteLine("Suspended from client.");
            if(gmap != null)
                gmap.MyLocationEnabled = false;
            await LocationServices.FusedLocationApi.RemoveLocationUpdates(apiClient, this);

        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Console.WriteLine($"Connection failed, attempting to reach Google Play Services\n\nCode: {result.ErrorCode}\nMessage: {result.ErrorMessage}");
        }

        public void OnLocationChanged(Location location)
        {
            _location = location;
            Lat_Lng = new LatLng(location.Latitude, location.Longitude);
            if (!locationCenter && gmap != null)
            {
                CameraUpdate cu = CameraUpdateFactory.NewLatLngZoom(new LatLng(location.Latitude, location.Longitude), 17);
                gmap.AnimateCamera(cu);
                locationCenter = true;
            }
        }

        private bool IsGooglePlayServicesInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Console.WriteLine("Google Play Services are available");
                return true;
            }
            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Console.WriteLine($"There is a problem with Google Play Services on this device: {errorString}");
            }
            return false;

        }

        public bool IsLocationEnabled()
        {
            lm = (LocationManager)GetSystemService(Context.LocationService);
            try
            {
                return lm.IsProviderEnabled(LocationManager.GpsProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to check location service, {ex.ToString()}");
                return false;
            }
        }

        #endregion
    }


    public static class App
    {
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
    }
}