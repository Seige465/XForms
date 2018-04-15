using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Media;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using XForms.Droid.Helpers;
using XForms.XForms;

namespace XForms.Droid
{
    [Activity(Label = "Drawing", ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden )]
    public class DrawingActivity : BaseActivity
    {
        LinearLayout ColorLayout;
        LinearLayout WidthLayout;
        EditText EditExif;
        RelativeLayout MainCanvas;
        public static ImageView BackgroundImage;
        string path;

        Switch Highlight;
        SeekBar BrushSize;
        TextView WidthText;

        Bindings Binding;
        Controls Control;
        View v;
        bool newPhoto = false;

        int savedWidth;
        int savedHeight;

        static int strokeWidth = 4;
        static bool IsHighlighted = false;

        //this is used to keep between orientation changes
        public static Bitmap TempBitmap;
        //this bitmap is on the canvas item
        Bitmap canvasBitmap;
        DrawingView dv;

        BackgroundWorker bw;
        ProgressDialog progress;

        int[] Dims;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Console.WriteLine("Creating!!!!!!!!!!!!!!!..................");
            Title = Intent.GetStringExtra("title");
            base.OnCreate(savedInstanceState);

            progress = new ProgressDialog(this);
            progress.SetMessage("Loading...");
            progress.SetCancelable(false);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.Show();

            SetContentView(Resource.Layout.CanvasFullScreen);

            ColorLayout = FindViewById<LinearLayout>(Resource.Id.colourLayout);
            WidthLayout = FindViewById<LinearLayout>(Resource.Id.widthLayout);
            EditExif = FindViewById<EditText>(Resource.Id.editExifText);
            MainCanvas = FindViewById<RelativeLayout>(Resource.Id.mainCanvas);
            BackgroundImage = FindViewById<ImageView>(Resource.Id.backgroundImage);

            Highlight = FindViewById<Switch>(Resource.Id.switchHighlight);
            BrushSize = FindViewById<SeekBar>(Resource.Id.widthSeek);
            WidthText = FindViewById<TextView>(Resource.Id.widthText);

            MainCanvas.ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;

            //obtaining the items
            Binding = JsonConvert.DeserializeObject<Bindings>(Intent.GetStringExtra("binding"));
            Control = JsonConvert.DeserializeObject<Controls>(Intent.GetStringExtra("control"));
            v = WidgetHelper.viewToPass;


            if (Control.appearance.ToLower() == "signature")
            {
                ColorLayout.Visibility = ViewStates.Gone;
                WidgetHelper.mPaint.Color = Color.Black;
            }

            bw = new BackgroundWorker();

            bw.DoWork += delegate
            {

                Window.SetSoftInputMode(SoftInput.StateHidden);


                if (Binding != null && Control != null)
                {
                    path = XForm.GetValue(Binding.nodeset);


                    //if path is null, popup dialog for camera or gallery
                    if (string.IsNullOrEmpty(path))
                    {
                        RunOnUiThread(() => { MainCanvas.Background = new ColorDrawable(Color.White); });
                        newPhoto = true;
                        if(Control.appearance.ToLower() == "annotate" || Control.appearance.ToLower() == "textannotate")
                            TakePhoto("drawing", Binding, v, WidgetHelper.activity, false);
                    }


                    RunOnUiThread(() =>
                    {
                        BrushSize.Progress = strokeWidth;
                        WidthText.Text = $"Width: {strokeWidth.ToString()}";

                        //we don't need colours, text box or the highlight switch if signature
                        if (Control.appearance == "signature")
                        {
                            WidgetHelper.mPaint.Color = Color.Black;
                            ColorLayout.Visibility = ViewStates.Gone;
                            EditExif.Visibility = ViewStates.Gone;
                            Highlight.Visibility = ViewStates.Gone;

                        }
                        //setup events to change paint colour
                        else
                        {

                            ApplyButtonEvents(ColorLayout, WidgetHelper.mPaint);
                        }
                        /*
                        if (Control.appearance.ToLower() == "signature" || Control.appearance.ToLower() == "draw")
                            SetupDrawingCanvas();
                            */
                    });            
                }

                //I have no binding reference, cannot do much here lol
                else
                {
                    Finish();               
                }
            };

            bw.RunWorkerCompleted += delegate
            {
                progress.Dismiss();
            };

            bw.RunWorkerAsync();

        }

        private void SetupDrawingCanvas()
        {

            path = XForm.GetValue(Binding.nodeset);
            if (!string.IsNullOrEmpty(path))
            {

                Bitmap bitmap = BitmapFactory.DecodeFile(path);

                //grab the exif note if there is one
                ExifInterface exifData = new ExifInterface(path);
                string note = exifData.GetAttribute(ExifInterface.TagImageDescription);
                if (note != null)
                    EditExif.Text = note;

                //grab orientation if there is one, some images from the net may not even have it
                int orientation = exifData.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 1);
                switch (orientation)
                {
                    case 6:
                        bitmap = BitmapHelpers.RotateBitmap(bitmap, 90f);
                        break;
                    case 3:
                        bitmap = BitmapHelpers.RotateBitmap(bitmap, 180f);
                        break;
                    case 8:
                        bitmap = BitmapHelpers.RotateBitmap(bitmap, 270f);
                        break;
                }

                Dims = new int[] { bitmap.Width, bitmap.Height };

                //this one liner does all, gets the lower of the two to determine what to scale it by
                decimal scale = Math.Min(Decimal.Divide(MainCanvas.Width, Dims[0]), Decimal.Divide(MainCanvas.Height, Dims[1]));

                int width = (int)(Dims[0] * scale);
                int height = (int)(Dims[1] * scale);

                //background image

                if(Control.appearance.ToLower() == "annotate" || Control.appearance.ToLower() == "textannotate")
                {
                    bitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);
                    BackgroundImage.SetImageBitmap(bitmap);

                }

                //canvas image
                if (TempBitmap != null)
                    canvasBitmap = Bitmap.CreateScaledBitmap(TempBitmap, width, height, true);
                else
                    canvasBitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);


                if (dv == null)
                    dv = new DrawingView(this, canvasBitmap, 10, BackgroundImage);
                if(Control.appearance.ToLower() != "signature")
                    SetupEvents();

                var layoutParams = new RelativeLayout.LayoutParams(width, height);
                layoutParams.AddRule(LayoutRules.CenterInParent);
                dv.LayoutParameters = layoutParams;
                MainCanvas.AddView(dv);
                GC.Collect();
            }
            else if (Control.appearance.ToLower() == "signature" || Control.appearance.ToLower() == "draw")

            {
                int width = MainCanvas.Width;
                int height = MainCanvas.Height;

                //canvas image
                if (TempBitmap != null)
                    canvasBitmap = Bitmap.CreateScaledBitmap(TempBitmap, width, height, true);
                else
                    canvasBitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

                if (dv == null)
                    dv = new DrawingView(this, canvasBitmap, 10, BackgroundImage);
                if (Control.appearance.ToLower() != "signature")
                    SetupEvents();

                var layoutParams = new RelativeLayout.LayoutParams(width, height);
                layoutParams.AddRule(LayoutRules.CenterInParent);
                dv.LayoutParameters = layoutParams;
                MainCanvas.AddView(dv);
                GC.Collect();
            }
        } 

        private void SetupEvents()
        {
            BrushSize.ProgressChanged += delegate
            {
                strokeWidth = BrushSize.Progress;
                dv.UpdateStrokeWidth(strokeWidth);
                WidthText.Text = $"Width: {strokeWidth.ToString()}";
            };

            //add transparency in the drawing
            Highlight.CheckedChange += (t, f) =>
            {
                if (f.IsChecked)
                    WidgetHelper.mPaint.Color = Color.Argb(126, WidgetHelper.mPaint.Color.R, WidgetHelper.mPaint.Color.G, WidgetHelper.mPaint.Color.B);
                else
                    WidgetHelper.mPaint.Color = Color.Argb(255, WidgetHelper.mPaint.Color.R, WidgetHelper.mPaint.Color.G, WidgetHelper.mPaint.Color.B);

                IsHighlighted = f.IsChecked;
            };
        }

        private void SaveDrawing()
        {
            if (Control.appearance.ToLower() == "annotate" || Control.appearance.ToLower() == "textannotate")
            {
                Bitmap Background = ((BitmapDrawable)BackgroundImage.Drawable).Bitmap;
                Bitmap CombinedBitmap = OverlayCanvas(Background, dv.bm);

                using (var os = new FileStream(path, FileMode.Create))
                {
                    CombinedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, os);
                }

                //save any note that the user entered
                ExifInterface exifData = new ExifInterface(path);
                exifData.SetAttribute(ExifInterface.TagImageDescription, EditExif.Text);
                exifData.SaveAttributes();
            }
            else
            {
                using (var os = new FileStream(path, FileMode.Create))
                {
                    dv.bm.Compress(Bitmap.CompressFormat.Jpeg, 100, os);
                }
            }


            XForm.SetValue(Binding.nodeset, path);
            Finish();
        }

        protected override void OnDestroy()
        {
            //we are changing orientation yo, so save the canvas layer
            if (!IsFinishing)
            {
                if(dv != null)
                    TempBitmap = dv.bm;
            }
            base.OnDestroy();
        }

        private void ViewTreeObserver_GlobalLayout(object sender, EventArgs e)
        {
            savedWidth = BackgroundImage.MeasuredWidth;
            savedHeight = BackgroundImage.MeasuredHeight;


            Console.WriteLine($"{savedWidth},{savedHeight}\n");
            if(dv == null)
                SetupDrawingCanvas();
            

        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            if(newConfig.Orientation == Android.Content.Res.Orientation.Landscape)
            {
                Console.WriteLine("landscape!");
            }
            else if(newConfig.Orientation == Android.Content.Res.Orientation.Portrait)
            {
                Console.WriteLine("portrait");

            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (newPhoto)
            {
                if (dv != null)
                {
                    dv.ClearCanvas();
                    newPhoto = false;
                }
            }
            Console.WriteLine("Resuming!!!!!!!!!!!!!!!..................");
        }



        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            //if just drawing or signature
            if(Control.appearance.ToLower() == "signature" || Control.appearance.ToLower() == "draw")
                MenuInflater.Inflate(Resource.Menu.menu_draw_sig, menu);
            else
                MenuInflater.Inflate(Resource.Menu.menu_canvas, menu);


            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent i = new Intent();

            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:

                    AlertDialog saveAlert = new AlertDialog.Builder(this).Create();
                    saveAlert.SetTitle("Warning");
                    saveAlert.SetMessage("Do you wish to save the drawing?");
                    saveAlert.SetButton("Save", delegate
                    {
                        SaveDrawing();
                        i.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                        SetResult(Result.Ok, i);
                        WidgetHelper.activity.FireFormChangeEvent(v);

                        Finish();
                    });

                    saveAlert.SetButton2("Discard", delegate
                    {
                        if(!string.IsNullOrEmpty(path))
                            File.Delete(path);
                        if (dv != null)
                        {
                            dv.ClearCanvas();
                            dv = null;
                        }
                        SetResult(Result.Canceled);
                        XForm.SetValue(Binding.nodeset, "");
                        Finish();

                    });
                    saveAlert.Show();

                    return true;
                case Resource.Id.menu_save:
                    //overlay the two bitmaps together



                    i.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                    SetResult(Result.Ok, i);
                    SaveDrawing();
                    WidgetHelper.activity.FireFormChangeEvent(v);
                    return true;
                //reset the item and canvas
                case Resource.Id.menu_delete:
                    AlertDialog alert = new AlertDialog.Builder(this).Create();
                    alert.SetTitle("Warning");
                    alert.SetMessage("Do you want to delete the annotated image?");
                    alert.SetButton("Yes", delegate 
                    {
                        File.Delete(path);
                        XForm.SetValue(Binding.nodeset, "");
                        BackgroundImage.SetImageBitmap(null);
                        if(dv != null)
                        {
                            dv.ClearCanvas();
                            dv = null;
                        }
                    });
                    alert.SetButton2("No", delegate { return; });
                    alert.Show();
                    return true;
                //image picker
                case Resource.Id.menu_select:
                    TakePhoto("drawing", Binding, v, WidgetHelper.activity, false);
                    newPhoto = true;
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);


            }
        }

        private Bitmap OverlayCanvas(Bitmap BaseLayer, Bitmap CanvasLayer)
        {
            //scale the background
            Bitmap Combined = Bitmap.CreateScaledBitmap(BaseLayer, Dims[0], Dims[1], true);
            Bitmap MutableBitmap = Combined.Copy(Bitmap.Config.Argb8888, true);
            //scale the canvas
            CanvasLayer = Bitmap.CreateScaledBitmap(CanvasLayer, Dims[0], Dims[1], true);

            //now piece the two together
            Canvas canvas = new Canvas(MutableBitmap);
            //canvas.DrawBitmap(BaseLayer, new Matrix(), null);
            canvas.DrawBitmap(CanvasLayer, 0, 0, null);
            return MutableBitmap;
        }

        private void ApplyButtonEvents(View v, Paint paint)
        {
            //the event

            if (v is LinearLayout ll)
            {
                for (int i = 0; i < ll.ChildCount; i++)
                {
                    var button = ll.GetChildAt(i);
                    if (button is Button b)
                        b.Click += delegate
                        {
                            if (button.Background is ColorDrawable cd)
                            {
                                cd = (ColorDrawable)button.Background;
                                if (IsHighlighted)
                                    paint.Color = Color.Argb(128, cd.Color.R, cd.Color.G, cd.Color.B);
                                else
                                    paint.Color = cd.Color;
                            }                            
                        };
                }
            }
        }

    }
}