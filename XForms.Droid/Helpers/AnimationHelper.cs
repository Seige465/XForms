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
using Android.Views.Animations;

namespace XForms.Droid.Helpers
{
    public class AlphaViewAnimation : Animation.IAnimationListener
    {
        int Duration;
        Dialog dialog;
        AlphaAnimation aa;
        bool FadeIn;

        View view;

        /// <summary>
        /// Creates a show/hide animation for a dialog item
        /// </summary>
        /// <param name="dlg">The dialog you wish to show/hide</param>
        /// <param name="duration">How long it takes to do said animation.</param>
        /// <param name="fadeIn">If true, animation will fade in, false for fade out.</param>
        public AlphaViewAnimation(Dialog dlg, int duration, bool fadeIn)
        {
            if (fadeIn)
                aa = new AlphaAnimation(0, 1);
            else
                aa = new AlphaAnimation(1, 0);

            dialog = dlg;
            Duration = duration;
            FadeIn = fadeIn;

            aa.Duration = Duration;
        }

        /// <summary>
        /// Creates a show/hide animation for a view item
        /// </summary>
        /// <param name="v">The view you wish to show/hide</param>
        /// <param name="duration">How long it takes to do said animation.</param>
        /// <param name="fadeIn">If true, animation will fade in, false for fade out.</param>
        public AlphaViewAnimation(View v, int duration, bool fadeIn)
        {
            if (fadeIn)
                aa = new AlphaAnimation(0, 1);
            else
                aa = new AlphaAnimation(1, 0);

            view = v;
            Duration = duration;
            FadeIn = fadeIn;

            aa.Duration = Duration;
        }

        public IntPtr Handle => (IntPtr)0;

        public void Dispose()
        {
            aa = null;
        }

        public void OnAnimationEnd(Animation animation)
        {
            if (!FadeIn)
            {
                dialog?.Dismiss();
                if(view != null) view.Visibility = ViewStates.Gone;
            }
            else
            {
                dialog?.Show();
                if (view != null) view.Visibility = ViewStates.Visible;
            }

        }

        public void OnAnimationRepeat(Animation animation)
        {
        }

        public void OnAnimationStart(Animation animation)
        {
            Console.WriteLine("starting!");
        }
    }
}