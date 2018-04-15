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
using Android.Graphics.Drawables;
using Android.Util;

namespace XForms.Droid
{
    public static class DesignHelper
    {
        public static Color controlColor = Color.Rgb(74, 131, 144);
        public static Color fontColor = Color.Rgb(85, 85, 85);
        public static Color hintColor = Color.DarkGray;
        public static Color titleColor = Color.WhiteSmoke;
        public static Color backgroundColor = Color.Rgb(237, 242, 244);
        public static Color statusBarColor = Color.Rgb(52, 92, 101);

        public static Color buttonColor = Color.Rgb(128, 168, 177);
        public static Bitmap header;



        /// <summary>
        /// Set a button to design colour.
        /// </summary>
        /// <param name="button"></param>
        public static void DesignButton(this Button button)
        {
            button.Background.SetColorFilter(buttonColor, PorterDuff.Mode.SrcAtop);
            button.SetTextColor(fontColor);
        }

        /// <summary>
        /// Set text box to design colours.
        /// </summary>
        /// <param name="textbox"></param>
        public static void DesignEditText(this EditText textbox)
        {
            textbox.SetBackgroundColor(controlColor);
            textbox.SetTextColor(fontColor);
            textbox.SetHintTextColor(hintColor);
        }

        /// <summary>
        /// Set a label to design colour.
        /// </summary>
        /// <param name="label"></param>
        public static void DesignTitle(this TextView label)
        {
            label.SetTextColor(titleColor);
        }

        /// <summary>
        /// Set a label to design colour which is bold.
        /// </summary>
        /// <param name="label"></param>
        public static void DesignTitleBold(this TextView label)
        {
            label.SetTextColor(titleColor);
            label.SetTypeface(label.Typeface, TypefaceStyle.Bold);
        }

        /// <summary>
        /// Set label to the design font colour.
        /// </summary>
        /// <param name="text"></param>
        public static void DesignText(this TextView text)
        {
            text.SetTextColor(fontColor);
        }

        /// <summary>
        /// Set radio buttons to design font colour.
        /// </summary>
        /// <param name="radio"></param>
        public static void DesignRadioButtons(this RadioButton radio)
        {
            radio.SetTextColor(fontColor);
            radio.SetPadding(10, 10, 0, 10);
        }

        /// <summary>
        /// Sets the spinner to default readable white.
        /// </summary>
        /// <param name="combobox"></param>
        public static void DesignComboBox(this Spinner combobox)
        {
            combobox.Background.SetColorFilter(Color.White, PorterDuff.Mode.SrcAtop);

            //combobox.SetBackgroundColor(controlColor);
        }

        /// <summary>
        /// Sets a checkbox to design font colour.
        /// </summary>
        /// <param name="checkbox"></param>
        public static void DesignCheckbox(this CheckBox checkbox)
        {
            checkbox.SetTextColor(fontColor);
            checkbox.SetPadding(10, 10, 0, 10);
        }

        /// <summary>
        /// Creates a bitmap with supplied base64.
        /// </summary>
        /// <param name="base64Input"></param>
        /// <returns></returns>
        public static Bitmap CreateBitmap(this string base64Input)
        {
            try
            {
                byte[] bitmapdata = new byte[base64Input.Length];
                bitmapdata = Convert.FromBase64String(FixBase64ForImage(base64Input));
                System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapdata);
                Bitmap bitmap = BitmapFactory.DecodeStream(streamBitmap);
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
        private static string FixBase64ForImage(String image)
        {
            StringBuilder sbText = new StringBuilder(image, image.Length);
            sbText.Replace("\r\n", String.Empty);
            sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }


        public static void SetBar(Activity activity, String title, String recentTitle, int imageResource)
        {
            if (controlColor == Color.LightGray)
            {
                controlColor = new Color(82, 121, 143);
            }
            activity.ActionBar.SetBackgroundDrawable(new ColorDrawable(controlColor));
            activity.ActionBar.Title = title;


            Bitmap bm = BitmapFactory.DecodeResource(activity.Resources, imageResource);
            ActivityManager.TaskDescription taskDesc = new ActivityManager.TaskDescription(recentTitle, bm, controlColor);
            activity.SetTaskDescription(taskDesc);

            Window window = activity.Window;
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);



            //set the status bar to a slight darker color
            window.SetStatusBarColor(new Color((int)(controlColor.R * 0.8f), (int)(controlColor.G * 0.8f), (int)(controlColor.B * 0.8f)));
            //window.SetStatusBarColor(controlColor);
            window.SetNavigationBarColor(controlColor);
        }

        public static void SetBackgroundBar(Toolbar v, Context c)
        {
            v.Background = new ColorDrawable(controlColor);
            v.SetTitleTextAppearance(c, Android.Resource.Attribute.TitleTextAppearance);
            v.SetTitleTextColor((controlColor.R * 0.299 + controlColor.G * 0.587 + controlColor.B * 0.114) > 186 ? Color.Black : Color.White);
        }

        /// <summary>
        /// Sets the button text based on the control colour supplied
        /// </summary>
        /// <param name="b">The button to set text colour.</param>
        public static void SetButtonText(this Button b)
        {

            b.SetTextColor((controlColor.R * 0.299 + controlColor.G * 0.587 + controlColor.B * 0.114) > 186 ? Color.Black : Color.White);
            


        }

        public static void SetTabs(this Activity activity)
        {
            activity.ActionBar.SetStackedBackgroundDrawable(new ColorDrawable(backgroundColor));
        }

        public static void SetDesigner(Designer designer)
        {
            backgroundColor = Color.ParseColor(designer.Backgroundcolour);
            controlColor = Color.ParseColor(designer.Controlcolour);
            fontColor = Color.ParseColor(designer.Fontcolour);
            titleColor = Color.ParseColor(designer.Titlecolour);
            //header = CreateBitmap(designer.headerimage);
        }

        /// <summary>
        /// properly sets the listview height
        /// </summary>
        /// <param name="listView"></param>
        public static void SetListViewHeightBasedOnChildren(this ListView listView)
        {
            BaseAdapter adapter = (BaseAdapter)listView.Adapter;
            if (adapter == null)
                return;

            int height = 0;
            View view = null;

            for (int i = 0; i < adapter.Count; i++)
            {
                view = adapter.GetView(i, view, listView);
                height += view.MeasuredHeight;
            }

            ViewGroup.LayoutParams parameters = listView.LayoutParameters;
            parameters.Height = height + ((listView.DividerHeight) * (adapter.Count - 1));
            listView.LayoutParameters = parameters;
            listView.RequestLayout();

        }

        /// <summary>
        /// Changes the buttons of the alertdialog to the controlcolour given.
        /// </summary>
        /// <param name="dialog">The dialog which you want to change its buttons' backgrounds.</param>
        public static void SetDialogButtons(this AlertDialog dialog)
        {
            for(int i = -1; i > -4; i--)
            {
                Button b = dialog.GetButton(i);
                if(b != null)
                {
                    LinearLayout.LayoutParams lp = (LinearLayout.LayoutParams)b.LayoutParameters;
                    lp.SetMargins(5, 5, 5, 5);


                    b.SetAllCaps(false);
                    b.SetBackgroundResource(Resource.Drawable.corner5dp);
                    b.SetDrawableBackground();
                    b.Elevation = 3;
                    b.LayoutParameters = lp;
                    b.SetButtonText();

                }
            }
        }
        /// <summary>
        /// This changes the colour of a view's background, and if needed, gets the xml drawable used.
        /// </summary>
        /// <param name="view">The view to get its background changed.</param>
        public static void SetDrawableBackground(this View view)
        {
            LayerDrawable gd = (LayerDrawable)view.Background;
            
            gd.SetColorFilter(controlColor,PorterDuff.Mode.SrcAtop);

        }

        public static bool IsLessThanHalf(Activity parent, View parentView, View childView)
        {
            //change to parent view, not main window
            //DisplayMetrics dm = act.GetDisplayInfo();
            // Display display = act.WindowManager.DefaultDisplay;

            DisplayMetrics dm = parent.GetDisplayInfo();
            Display display = parent.WindowManager.DefaultDisplay;


            int width = (int)(dm.WidthPixels / dm.Density);

        //    parentView.Measure(0, 0);

          //  Rect bounds = new Rect();
          //  if(childView is TextView tv)
          //  {
          //      tv.Paint.GetTextBounds(tv.Text, 0, tv.Text.Length, bounds);
          //  }

            childView.Measure(0, 0);

            //  int width = (int)(dm.WidthPixels / dm.Density);
            //childView.Measure(0, 0);
            //   int textWidth = ;

           // Console.WriteLine($"TextView: {bounds.Width()}, parent view halved: {parentView.MeasuredWidth /2}");

            return childView.MeasuredWidth < (width / 2);

            //return bounds.Width() < (parentView.MeasuredWidth / 2);

        }

        public static DisplayMetrics GetDisplayInfo(this Activity act)
        {
            DisplayMetrics dm = new DisplayMetrics();
            act.WindowManager.DefaultDisplay.GetMetrics(dm);
            return dm;
        }


        public static void ChangeLayoutHorizontal(LinearLayout ll)
        {
            ll.Orientation = Orientation.Horizontal;
            //ll.WeightSum = 2;
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.WrapContent);
            //lp.SetMargins(5, 5, 5, 5);
            ll.LayoutParameters = lp;
            //have padding on bottom line
            //play with these params, take out padding
            //ll.SetPadding(5, 5, 5, 5);
            //the params for each item in the input edit area
            LinearLayout.LayoutParams textParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            LinearLayout.LayoutParams editParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);


            EditText edit = ll.FindViewById<EditText>(Resource.Id.textEdit);
            edit.LayoutParameters = editParams;
            edit.Gravity = GravityFlags.Right;

            TextView text = (TextView)ll.FindViewWithTag("title");
            text.LayoutParameters = textParams;

        }


        public class Designer
        {

            public string Backgroundcolour { get; set; }
            public string Controlcolour { get; set; }
            public string Fontcolour { get; set; }
            public string Titlecolour { get; set; }
            public string Headerimage { get; set; }
        }
    }
}