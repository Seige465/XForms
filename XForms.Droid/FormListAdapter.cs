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
using Java.Lang;

namespace XForms.Droid
{
    public class FormListAdapter : BaseAdapter
    {
        List<string> forms;
        Context context;

        List<ProgressBar> formProgressBars;

        public FormListAdapter(List<string> formList, Context c)
        {
            forms = formList;
            context = c;

            formProgressBars = new List<ProgressBar>();
        }

        public override int Count => forms.Count;

        public override Java.Lang.Object GetItem(int position)
        {
            return forms[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void HideProgress(int position)
        {
            formProgressBars[position].Visibility = ViewStates.Gone;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = convertView;
            if(v == null)
            {
                LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                v = inflater.Inflate(Resource.Layout.FormList, null);
            }

            TextView tv = v.FindViewById<TextView>(Resource.Id.formName);
            tv.Text = forms[position];
            ProgressBar progress = v.FindViewById<ProgressBar>(Resource.Id.downloadProgress);
            progress.Visibility = ViewStates.Gone;
            formProgressBars.Add(progress);

            return v;

        }
    }
}