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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using XForms.XForms;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.Widgets
{
    public sealed partial class Upload_Draw : UserControl
    {
        WidgetMaster _master;
        public Upload_Draw(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            if (isReadOnly)
            {
                lblCanvas.IsHitTestVisible = false;
                lblCanvas.IsTapEnabled = false;
            }
            string value = XForm.GetValue(_master._binding.nodeset);
            if(!string.IsNullOrWhiteSpace(value))
                imgImage.Source = new BitmapImage(new Uri(value, UriKind.Absolute));
        }
        private void lblCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _master._parent.PopupChanged += _parent_PopupChanged;
            _master._parent.ShowPopup(this, XFormMaster.PopupType.Canvas, _master._control.label, _master._control.appearance, XForm.GetValue(_master._binding.nodeset));
        }

        private void _parent_PopupChanged(object sender, EventArgs e)
        {
            _master._parent.PopupChanged -= _parent_PopupChanged;
            if (!(bool)((PopupControls.PopupEventArgs)e).Save)
                return;
            if (e != null && ((PopupControls.PopupEventArgs)e).Value != null && !string.IsNullOrWhiteSpace(((PopupControls.PopupEventArgs)e).Value.ToString()))
            {
                string imagepath = ((PopupControls.PopupEventArgs)e).Value.ToString();
                if (!File.Exists(imagepath))
                {
                    imgImage.Source = null;
                    return;
                }
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.UriSource = new Uri(imagepath, UriKind.Absolute);
                imgImage.Source = bitmapImage;
                _master.UpdateValue(imagepath);                
            }
        }
    }
}
