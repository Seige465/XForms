using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.PopupControls
{
    public sealed partial class Map : UserControl
    {
        XFormMaster _parent;        
        public  Map(XFormMaster parent, object value)
        {
            this.InitializeComponent();
            _parent = parent;
            mpMap.Height = parent.Frame.ActualHeight * 0.9;
            mpMap.Width = parent.Frame.ActualWidth * 0.9;
            this.Margin = new Thickness(parent.Frame.ActualWidth * 0.05, parent.Frame.ActualHeight * 0.05, parent.Frame.ActualWidth * 0.05, parent.Frame.ActualHeight * 0.05);
            mpMap.Style = MapStyle.AerialWithRoads;
            mpMap.ZoomLevel = 18;
            mpMap.TiltInteractionMode = MapInteractionMode.Disabled;
            mpMap.RotateInteractionMode = MapInteractionMode.Disabled;
            SetFirstLocation(value.ToString());            
        }
        private CancellationTokenSource _cts = null;
        private async Task<Geoposition> SetLocation()
        {
            try
            {
                // Request permission to access location
                var accessStatus = await Geolocator.RequestAccessAsync();
                Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };
                
                if (geolocator.LocationStatus == PositionStatus.Disabled)
                {
                    var uri = new Uri(@"ms-settings:privacy-location");
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:
                        // Get cancellation token
                        _cts = new CancellationTokenSource();
                        CancellationToken token = _cts.Token;
                        // Carry out the operation
                        Geoposition pos = await geolocator.GetGeopositionAsync().AsTask(token);
                        return pos;                        

                    case GeolocationAccessStatus.Denied:       
                        
                        return null;

                    case GeolocationAccessStatus.Unspecified:                        
                        return null;
                }
            }
            catch (TaskCanceledException)
            {
                //_rootPage.NotifyUser("Canceled.", NotifyType.StatusMessage);
            }
            catch (Exception ex)
            {
                //_rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
            }
            finally
            {
                _cts = null;
            }
            return null;
        }

        private async void SetFirstLocation(string value)
        {
            BasicGeoposition geoposition;
            Geoposition position = await SetLocation();
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                geoposition = new BasicGeoposition() { Latitude = double.Parse(value.Split(' ')[0]), Longitude = double.Parse(value.Split(' ')[1]) };
            else if (position == null)
                geoposition = new BasicGeoposition() { Latitude = -40.2260306564156, Longitude = 175.566287033141 };            
            else
                geoposition = new BasicGeoposition() { Latitude = position.Coordinate.Point.Position.Latitude, Longitude = position.Coordinate.Point.Position.Longitude };
            Geopoint geopoint = new Geopoint(geoposition);
            mpMap.Center = geopoint;
            SetNeedleIcon(geopoint);
        }
        
        
        private void mpMap_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            SetNeedleIcon(args.Location);
        }        
        DraggablePin dp;
        private void SetNeedleIcon(Geopoint location)
        {
            dp = new DraggablePin(mpMap);
            dp.Draggable = true;
            dp.SetValue(MapControl.LocationProperty, location);
            dp.DragEnd += Dp_DragEnd;
            mpMap.Children.Clear();
            mpMap.Children.Add(dp);
            _parent._popupValue = location;
        }

        private void Dp_DragEnd(object sender, EventArgs e)
        {
            _parent._popupValue = (Geopoint)sender;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.ClosePopup();
        }
    }
}
