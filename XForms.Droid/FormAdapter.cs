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
using XForms.XForms;
using Android.Graphics;
using Android.Views.InputMethods;

namespace XForms.Droid
{
    public class FormAdapter : BaseAdapter
    {
        public List<Controls> FormControls;
        public FormActivity fActivity;


        //use this list to iterate through and update the rows
        public List<View> ControlViews;

        public FormAdapter(List<Controls> controls, FormActivity _parent) : base()
        {
            FormControls = controls;
            fActivity = _parent;

            ControlViews = new List<View>();

        }


        public override int Count => FormControls == null ? 0 : FormControls.Count;

        //this isn't relevant for me lol
        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        //neither is this lol
        public override long GetItemId(int position)
        {
            return position;
        }




        //this is
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = LayoutInflater.From(parent.Context);

            View view = convertView;

            Bindings binding = DLL.GetBindingByReference(FormControls[position].reference);
            //what form of control it is
            switch ((ControlType)FormControls[position].type)
            {
                case ControlType.input:
                    //the user puts something in 
                    //view = WidgetHelper.CreateInput(FormControls[position], binding, inflater, fActivity, this);

                    break;
                case ControlType.output:
                    Console.WriteLine("not yet");
                    view = new LinearLayout(parent.Context);
                    view.SetBackgroundColor(Color.Red);
                    view.SetMinimumHeight(25);


                    break;

                case ControlType.select:
                    //view = WidgetHelper.CreateSelect(FormControls[position], binding, inflater, fActivity, this);


                    break;
                case ControlType.select1:
                    //view = WidgetHelper.CreateSelectOne(FormControls[position], binding, inflater, fActivity, this);


                    break;
                case ControlType.upload:
                    Console.WriteLine("not yet");
                    view = new LinearLayout(parent.Context);
                    view.SetBackgroundColor(Color.Blue);
                    view.SetMinimumHeight(25);
                    break;

            }


            ControlViews.Add(view);
            return view;


        }

        public override void NotifyDataSetInvalidated()
        {
            //imm = (InputMethodManager)fActivity.GetSystemService(Context.InputMethodService);
            //imm.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.ImplicitOnly);
            Console.WriteLine("invalidating data set");
            base.NotifyDataSetInvalidated();
        }


    }
}