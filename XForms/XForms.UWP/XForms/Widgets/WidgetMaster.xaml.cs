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
    public sealed partial class WidgetMaster : UserControl
    {
        public Controls _control;
        public Bindings _binding;
        public XFormMaster _parent;        
        public WidgetMaster(Controls control, XFormMaster parent, bool isReadOnly)
        {
            this.InitializeComponent();
            //Hide unexpected values           
            lblLabel.Text = string.Empty;           

            _parent = parent;
            _control = control;            
            _binding = XForm.GetBindingForControl(_control);
            _parent.FormChanged += _parent_FormChanged;
            if (!string.IsNullOrWhiteSpace(_control.xpathhint))
                _control.hint = XForm.Evaluate(_control.xpathhint).ToString();
            if (string.IsNullOrWhiteSpace(_control.hint))
                ellHint.Visibility = Visibility.Collapsed;
            lblLabel.Text = _control.label;
            AddControl(isReadOnly);
        }
        private void _parent_FormChanged(object sender, EventArgs e)
        {
            CheckRelevence();
            CheckRequired();
            CheckCalculate();
            //CheckConstraint();
        }
        private void AddControl(bool isReadOnly)
        {
            switch ((ControlType)_control.type)
            {
                case ControlType.input:                    
                    switch ((BindType)_binding.type)
                    {
                        case BindType.xGeoPoint:
                            control.Content = new GeoPoint(this, isReadOnly);
                            break;
                        default:
                            bool multilineAppearance = _control.appearance?.Equals("multiline", StringComparison.OrdinalIgnoreCase) ?? false;
                            if (lblLabel.ActualWidth > (grdInput.ActualWidth / 2) || multilineAppearance)
                            {
                                control.SetValue(Grid.RowProperty, 1);
                                control.SetValue(Grid.ColumnProperty, 0);
                                control.SetValue(Grid.ColumnSpanProperty, 99);                                
                            }
                            control.Content = new Input(this, isReadOnly);
                            break;
                    }
                    break;
                case ControlType.select:
                    if (_control.appearance?.Equals("minimal", StringComparison.OrdinalIgnoreCase) ?? false)
                        control.Content = new MultiSegmentControl(this, isReadOnly);
                    else
                        control.Content = new Select(this, isReadOnly);
                    break;
                case ControlType.select1:
                    if(_control.appearance?.Equals("minimal", StringComparison.OrdinalIgnoreCase) ?? false)
                        control.Content = new SegmentControl(this, isReadOnly);
                    else 
                        control.Content = new Select1(this, isReadOnly);
                    break;
                case ControlType.upload:
                    switch (_control.appearance)
                    {
                        case "draw":
                            control.Content = new Upload_Draw(this, isReadOnly);
                            break;
                        case "signature":
                            control.Content = new Upload_Draw(this, isReadOnly);
                            break;
                        case "annotate":
                            control.Content = new Upload_Draw(this, isReadOnly);
                            break;
                        case "textannotate":
                            control.Content = new Upload_Draw(this, isReadOnly);
                            break;
                        case "image":
                        default:
                            control.Content = new Upload_Image(this, isReadOnly);
                            break;
                    }
                    break;
            }
        }

        public void UpdateValue(string value)
        {
            string oldValue = XForm.GetValue(_binding.nodeset);
            XForm.SetValue(_binding.nodeset, value);
            if (!string.IsNullOrWhiteSpace(oldValue))
            {
                if (!CheckConstraint(oldValue))
                    _parent.FireFormChangeEvent(this);
            }
            else
                _parent.FireFormChangeEvent(this);
        }

        private async void CheckRelevence()
        {
            if (string.IsNullOrWhiteSpace(_binding.relevant))
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
        private async void CheckRequired()
        {
            if (string.IsNullOrWhiteSpace(_binding.required))
                return;
            var required = XForm.Evaluate(_binding.required);
            if (required == null) return;
            //Get the value of the control from the instance.
            if ((bool)required && string.IsNullOrWhiteSpace(XForm.GetValue(_binding.nodeset)))
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    lblLabel.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 10, 10));                    
                });
            else
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    lblLabel.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 10, 10));                    
                });
        }
        private async void CheckCalculate()
        {
            if (_control.xpathlabel != null)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    lblLabel.Text = XForm.Evaluate(_control.xpathlabel).ToString();
                });

            if (_control.xpathhint != null)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _control.hint = XForm.Evaluate(_control.xpathhint).ToString();
                    //txtInput.PlaceholderText = XForm.Evaluate(_control.xpathhint).ToString();
                });
        }
        public bool CheckConstraint(string oldval)
        {
            if (_binding.constraint == null)
                return false;
            object eval = XForm.Evaluate(_binding.constraint, _binding.nodeset);
            if (eval != null && !(bool)eval)
            {

                lblConstraintMessage.Text = _binding.constraintMsg;
                lblConstraintMessage.Visibility = Visibility.Visible;
                XForm.SetValue(_binding.nodeset, oldval);
                return true;
            }
            else
            {
                lblConstraintMessage.Text = string.Empty;
                lblConstraintMessage.Visibility = Visibility.Collapsed;
                return false;
            }
        }
        
        private void Hint_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.ShowHintDialog(lblLabel.Text ?? "" + " - Hint", _control.hint);
        }

        private void _WidgetMaster_GotFocus(object sender, RoutedEventArgs e)
        {
            control.Focus(FocusState.Programmatic);
        }
    }
}
