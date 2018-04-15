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

namespace XForms.Droid.Helpers
{
    class SelectOneAdapter : BaseAdapter
    {
        List<View> spinnerItems = new List<View>();

        Context context;
        List<string> items;

        public SelectOneAdapter(Context context, List<string> items)
        {
            this.context = context;
            this.items = items;
        }


        public override Java.Lang.Object GetItem(int position)
        {
            return items[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void HideHintView()
        {
           // spinnerItems[items.Count - 1].Visibility = ViewStates.Gone;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                view = inflater.Inflate(Resource.Layout.SelectOneDialogItem, null);
            }

            TextView tv = view.FindViewById<TextView>(Resource.Id.Text);
            tv.Text = items[position];
            //at the end, placeholder
            if (position == items.Count - 1)
            {
                tv.SetTextColor(Android.Graphics.Color.Gray);
                
            }
            spinnerItems.Add(view);
            return view;
        }

        //one less cause of the placeholder
        public override int Count
        {
            get
            {
                return items.Count - 1;
            }
        }

    }
}