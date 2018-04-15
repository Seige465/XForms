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
using Android.Text.Format;
using Java.Util;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Reflection;

namespace XForms.Droid.Fragments
{
    #region Helpers
    class DateFragments
    {
        /// <summary>
        /// Returns the date picker view object so we can hide what we like for the relevant controls.
        /// </summary>
        /// <param name="group">The dialog to pass through.</param>
        /// <returns></returns>
        public static DatePicker FindDatePicker(ViewGroup group)
        {
            if(group != null)
            {
                for(int i = 0; i < group.ChildCount; i++)
                {
                    View child = group.GetChildAt(i);
                    Console.WriteLine($"{child.GetType()} - {i}");
                    if (child is DatePicker)
                        return (DatePicker)child;
                    //recursion!
                    else if(child is ViewGroup)
                    {
                        Console.WriteLine("going deeper");
                        DatePicker result = FindDatePicker((ViewGroup)child);
                        if (result != null)
                            return result;
                    }
                }   
            }
            return null;
        }
    }
    #endregion

    #region Date Picker (and its appearance variants)
    public class DatePickerFragment : DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        Action<DateTime> dateSelectedHandler = delegate { };
        string Appearance;
        DatePickerDialog dialog;
        LayoutInflater inflater;

        public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected, string appearance, LayoutInflater inflater)
        {
            DatePickerFragment frag = new DatePickerFragment
            {
                dateSelectedHandler = onDateSelected,
                Appearance = appearance,
                inflater = inflater
            };
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime current = DateTime.Now;

            dialog = new DatePickerDialog(Activity,
            this,
            current.Year,
            current.Month - 1,
            current.Day);

            return dialog;


        }


        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            DateTime selectedDate = new DateTime(year, month + 1, dayOfMonth);
            dateSelectedHandler(selectedDate);
        }

        public override void OnPause()
        {
            base.OnPause();
            if (dialog != null && dialog.IsShowing)
                dialog.Dismiss();
        }
    }
    #endregion

    #region Time Picker
    public class TimePickerFragment : DialogFragment, TimePickerDialog.IOnTimeSetListener
    {
        Action<DateTime> timeSelectedHandler = delegate { };

        public static TimePickerFragment NewInstance(Action<DateTime> onTimeSelected)
        {
            TimePickerFragment frag = new TimePickerFragment
            {
                timeSelectedHandler = onTimeSelected
            };
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currentTime = DateTime.Now;
            bool is24Hour = DateFormat.Is24HourFormat(Activity);
            TimePickerDialog dialog = new TimePickerDialog(
                Activity, this, currentTime.Hour, currentTime.Minute, is24Hour);
            return dialog;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            DateTime currentTime = DateTime.Now;
            DateTime selectedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hourOfDay, minute, 0);
            timeSelectedHandler(selectedTime);
        }
    }
    #endregion
}