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

namespace XForms.UWP.XForms.Popup.Controls
{
    public sealed partial class DateTimePicker : UserControl
    {
        XFormMaster _parent;
        string _selectedValue = string.Empty;
        BindType _type;
        string _appearance;
        public DateTimePicker(XFormMaster parent, string title,  (int? type,string appearance)info, DateTime? value)
        {
            this.InitializeComponent();
            _parent = parent;
            _appearance = info.appearance as string;
            _type = (BindType)info.type;
            lblTitle.Text = title ?? "";
            SetDisplay(value);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //CENTER THE POPUP!
            grdMain.Margin = new Thickness(
                (this.ActualWidth / 2) + ((_parent.Frame.ActualWidth / 2) - this.ActualWidth),
                (this.ActualHeight / 2) + ((_parent.Frame.ActualHeight / 2) - this.ActualHeight),
                (this.ActualWidth / 2) + ((_parent.Frame.ActualWidth / 2) - this.ActualWidth),
                (this.ActualHeight / 2) + ((_parent.Frame.ActualHeight / 2) - this.ActualHeight));
        }
        private void SetDisplay(DateTime? value)
        {
            switch (_type)
            {
                case BindType.xDateTime:
                    SetDateTimeAppearance(value);
                    break;
                case BindType.xDate:
                    SetDateAppearance(value);
                    break;
                case BindType.xTime:
                    SetTimeAppearance(value);
                    break;
            }
        }
        private void SetDateTimeAppearance(DateTime? value)
        {
            if (value.HasValue)
            {
                dtInput.Date = value.Value.Date;
                tmInput.Time = new TimeSpan(value.Value.Hour, value.Value.Minute, value.Value.Second);
            }
            dtInput.Visibility = Visibility.Visible;
            dtInput.Margin = new Thickness(-2, 0, 0, 0);
            tmInput.Visibility = Visibility.Visible;            
        }
        private void SetDateAppearance(DateTime? value)
        {
            if(value.HasValue)
                dtInput.Date = value.Value.Date;
            dtInput.Visibility = Visibility.Visible;
            tmInput.Visibility = Visibility.Collapsed;
            if (_appearance == null)
                return;
            switch (_appearance.ToLower())
            {
                case "month-year":
                    dtInput.DayVisible = false;
                    break;
                case "day":
                    dtInput.MonthVisible = false;
                    dtInput.YearVisible = false;
                    break;
                case "month":
                    dtInput.DayVisible = false;
                    dtInput.YearVisible = false;
                    break;
                case "year":
                    dtInput.DayVisible = false;
                    dtInput.MonthVisible = false;
                    break;
            }


        }
        private void SetTimeAppearance(DateTime? value)
        {
            if (value.HasValue)
                tmInput.Time = new TimeSpan(value.Value.Hour, value.Value.Minute, value.Value.Second);
            tmInput.Visibility = Visibility.Visible;
            dtInput.Visibility = Visibility.Collapsed;
        }

        private void tmInput_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            DateTime date;
            if (_type == BindType.xDateTime)
            {
                date = dtInput.Date.DateTime;
                TimeSpan ts = tmInput.Time;
                date = date + ts;
            }
            else
            {
                date = new DateTime().Add(tmInput.Time);                
            }
            _parent._popupValue = date;
        }
        private void dtInput_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (_type == BindType.xDateTime)
            {
                DateTime date = dtInput.Date.DateTime;
                TimeSpan ts = tmInput.Time;
                date = date + ts;
                _parent._popupValue = date;                
            }
            else
                _parent._popupValue = dtInput.Date.DateTime;            
        }

        private void btnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.ClosePopup(false);
        }

        private void btnSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.ClosePopup();
        }

        
    }
}
