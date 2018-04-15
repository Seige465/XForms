using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace XForms.UWP.XForms.PopupControls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Loading : Page
    {
        XFormMaster _parent;
        public Loading(XFormMaster parent, object title, object message)
        {
            this.InitializeComponent();
            _parent = parent;
            _parent.PopupAllowLightClose(false);
            this.Height = parent.Frame.ActualHeight * 0.4;
            this.Width = parent.Frame.ActualWidth * 0.4;
            this.Margin = new Thickness(parent.Frame.ActualWidth * 0.3, parent.Frame.ActualHeight * 0.3, parent.Frame.ActualWidth * 0.3, parent.Frame.ActualHeight * 0.3);

            lblTitle.Text = title.ToString();
            lblMessage.Text = message.ToString();
            pbrProgress.Visibility = Visibility.Visible;
            pbrProgress.IsActive = true;
        }
        private CancellationTokenSource _cts = null;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;            
            Geoposition pos = await geolocator.GetGeopositionAsync().AsTask(token);
            BasicGeoposition geoposition = new BasicGeoposition() { Latitude = pos.Coordinate.Point.Position.Latitude, Longitude = pos.Coordinate.Point.Position.Longitude };
            Geopoint geopoint = new Geopoint(geoposition);
            _parent._popupValue = geopoint;
            _parent.ClosePopup();
        }

        private void btnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.ClosePopup(false);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _parent.PopupAllowLightClose(true); 
        }

    }
}
