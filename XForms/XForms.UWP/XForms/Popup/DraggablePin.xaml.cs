using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class DraggablePin : UserControl
    {
        #region Private Properties
        private MapControl _map;
        private bool isDragging = false;
        private Geopoint _location;
        #endregion


        #region Constructor
        public DraggablePin(MapControl map)
        {
            this.InitializeComponent();
            _map = map;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// A boolean indicating whether the pushpin can be dragged.
        /// </summary>
        public bool Draggable { get; set; }
        #endregion

        #region  Public Events

        /// <summary>
        /// Occurs when the pushpin is being dragged.
        /// </summary>
        public event EventHandler Drag;

        /// <summary>
        /// Occurs when the pushpin starts being dragged.
        /// </summary>
        public event EventHandler DragStart;

        /// <summary>
        /// Occurs when the pushpin stops being dragged.
        /// </summary>
        public event EventHandler DragEnd;

        #endregion

        #region Private Methods
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (Draggable)
            {
                if (_map != null)
                {
                    _map.PointerReleased += Map_PointerReleased;
                    _map.PointerMoved += Map_PointerMoved;
                    _map.PanInteractionMode = MapPanInteractionMode.Disabled;
                }
                var pointerPosition = e.GetCurrentPoint(_map);

                _location = null;
                var x = pointerPosition.Position.X;// - Window.Current.Bounds.X;
                var y = pointerPosition.Position.Y;// - Window.Current.Bounds.Y;
                
                _map.GetLocationFromOffset(new Point(x, y), out _location);
                
                this.SetValue(MapControl.LocationProperty, _location);
                DragStart?.Invoke(_location, new EventArgs());
                this.isDragging = true;
            }
        }

        private void Map_PointerMoved(object sender, PointerRoutedEventArgs e)
        {            
            if (this.isDragging)
            {
                var pointerPosition = e.GetCurrentPoint(_map);
                _location = null;
                var x = pointerPosition.Position.X;// - Window.Current.Bounds.X;
                var y = pointerPosition.Position.Y;// - Window.Current.Bounds.Y;                
                _map.GetLocationFromOffset(new Point(x, y), out _location);                
                this.SetValue(MapControl.LocationProperty, _location);
                Drag?.Invoke(_location, new EventArgs());
            }
        }

        private void Map_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Pushpin released, remove dragging events
            if (_map != null)
            {
                _map.PanInteractionMode = MapPanInteractionMode.Auto;                
                _map.PointerReleased -= Map_PointerReleased;
                _map.PointerMoved -= Map_PointerMoved;
            }
            var pointerPosition = e.GetCurrentPoint(_map);
            _location = null;
            var x = pointerPosition.Position.X;// - Window.Current.Bounds.X;
            var y = pointerPosition.Position.Y;// - Window.Current.Bounds.Y;
            _map.GetLocationFromOffset(new Point(x, y), out _location);            
            this.SetValue(MapControl.LocationProperty, _location);
            DragEnd?.Invoke(_location, new EventArgs());
            this.isDragging = false;
        }

        #endregion
    }
}
