using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;

namespace XForms.UWP.XForms.PopupControls
{
    class DraggableIcon
    {
        #region Private Properties

        private MapControl _map;
        private bool isDragging = false;
        public Geopoint _center;
        public Geopoint _location;

        #endregion


        #region Constructor

        public DraggableIcon(MapControl map)
        {
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
        public Action<BasicGeoposition> Drag;

        /// <summary>
        /// Occurs when the pushpin starts being dragged.
        /// </summary>
        public Action<BasicGeoposition> DragStart;

        /// <summary>
        /// Occurs when the pushpin stops being dragged.
        /// </summary>
        public Action<BasicGeoposition> DragEnd;

        #endregion

        #region Private Methods
        
        
        public void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (Draggable)
            {
                if (_map != null)
                {
                    //Store the center of the map
                    _center = _map.Center;

                    //Attach events to the map to track touch and movement events
                    _map.ActualCameraChanged += Map_ViewChanged;
                    _map.PointerReleased += Map_PointerReleased;
                    _map.PointerMoved += Map_PointerMoved;
                }

                var pointerPosition = e.GetCurrentPoint(_map);

                _location = null;
                var x = pointerPosition.Position.X - Window.Current.Bounds.X;
                var y = pointerPosition.Position.Y - Window.Current.Bounds.Y;
                //Convert the point pixel to a Location coordinate
                _map.GetLocationFromOffset(new Point(x, y), out _location);
                //if (_map.TryPixelToLocation(pointerPosition.Position, out location))
                //{
                //    MapLayer.SetPosition(this, location);
                //}

                DragStart?.Invoke(_location.Position);

                //Enable Dragging
                this.isDragging = true;
            }
        }

        private void Map_ViewChanged(MapControl sender, MapActualCameraChangedEventArgs args)
        {
            if (isDragging)
            {
                //Reset the map center to the stored center value.
                //This prevents the map from panning when we drag across it.
                _map.Center = _center;
            }
        }

        private void Map_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Check if the user is currently dragging the Pushpin
            if (this.isDragging)
            {
                //If so, move the Pushpin to where the pointer is.
                var pointerPosition = e.GetCurrentPoint(_map);

                _location = null;

                var x = pointerPosition.Position.X - Window.Current.Bounds.X;
                var y = pointerPosition.Position.Y - Window.Current.Bounds.Y;
                //Convert the point pixel to a Location coordinate
                _map.GetLocationFromOffset(new Point(x, y), out _location);
                //if (location != null)
                //{
                //    maplayer.setposition(this, location);
                //}

                Drag?.Invoke(_location.Position);
            }
        }

        private void Map_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Pushpin released, remove dragging events
            if (_map != null)
            {
                _map.ActualCameraChanged -= Map_ViewChanged;
                _map.PointerReleased -= Map_PointerReleased;
                _map.PointerMoved -= Map_PointerMoved;
            }

            var pointerPosition = e.GetCurrentPoint(_map);

            _location = null;
            var x = pointerPosition.Position.X - Window.Current.Bounds.X;
            var y = pointerPosition.Position.Y - Window.Current.Bounds.Y;
            //Convert the point pixel to a Location coordinate
            _map.GetLocationFromOffset(new Point(x, y), out _location);
            //if (_map.TryPixelToLocation(pointerPosition.Position, out location))
            //{
            //    MapLayer.SetPosition(this, location);
            //}
            DragEnd?.Invoke(_location.Position);
            this.isDragging = false;
        }

        #endregion
    }
}
