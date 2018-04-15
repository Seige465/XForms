using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
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
using XForms.XForms;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.Widgets
{
    public sealed partial class GeoPoint : UserControl
    {
        WidgetMaster _master;
        public GeoPoint(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            if (isReadOnly)
            {
                lblMap.IsHitTestVisible = false;
                lblMap.IsTapEnabled = false;
            }
        }
        private async void lblMap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _master._parent.PopupChanged += _parent_PopupChanged;
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus != GeolocationAccessStatus.Allowed)
            {
                Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog("Location Services are turned off, please turn on location services to get GPS", "Location Services are Off");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ok"));
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel"));
                dialog.DefaultCommandIndex = 0;
                var response = await dialog.ShowAsync();
                if (response.Label == "Ok")
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(@"ms-settings:privacy-location"));
                accessStatus = await Geolocator.RequestAccessAsync();
                if (accessStatus != GeolocationAccessStatus.Allowed)
                    return;
            }
            bool isInternetConnected = NetworkInterface.GetIsNetworkAvailable();
            if (!isInternetConnected)
            {
                _master._parent.ShowPopup(this, XFormMaster.PopupType.Loading, "Getting Location", "Loading your GPS location.", null);
            }


            string val = XForm.GetValue(_master._binding.nodeset);
            _master._parent.ShowPopup(this, XFormMaster.PopupType.Map, _master._control.label, null, val);
        }
        private void _parent_PopupChanged(object sender, EventArgs e)
        {
            _master._parent.PopupChanged -= _parent_PopupChanged;   
            if(!(bool)((PopupControls.PopupEventArgs)e).Save)
                return;
            object value = ((PopupControls.PopupEventArgs)e).Value;
            if (value == null)
                return;
            //Set the value. 
            Geopoint location = (Geopoint)value;
            lblMap.Text = $"{location.Position.Latitude} {location.Position.Longitude} - Click here to goto map";
            _master.UpdateValue($"{location.Position.Latitude} {location.Position.Longitude}");           
        }
    }
}
