using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XForms.XForms;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.Widgets
{
    public sealed partial class Page : UserControl
    {
        XFormMaster _parent;
        Groups _group;
        Bindings _binding;
        public Page()
        {
            this.InitializeComponent();
        }
        public Page(Groups group, XFormMaster parent)
        {
            this.InitializeComponent();
            _parent = parent;
            _group = group;
            _binding = XForm.GetBindingForGroup(_group);
            //_parent.FormChanged += _parent_FormChanged;
            lblTitle.Text = group.label;
        }
        private void _parent_FormChanged(object sender, EventArgs e)
        {
            CheckRelevence();
        }
        private async void CheckRelevence()
        {
            if (_binding == null || string.IsNullOrWhiteSpace(_binding.relevant))
                return;
            var relevent = XForm.Evaluate(_binding.relevant);
            if (relevent == null) return;
            if ((bool)relevent)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Visibility = Visibility.Visible;
                    Show.Begin();
                });
            else
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.Visibility = Visibility.Collapsed;
                    Hide.Begin();
                });
        }

        private void userPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.Page_Navigate(_group.id, e);
        }
    }
}
