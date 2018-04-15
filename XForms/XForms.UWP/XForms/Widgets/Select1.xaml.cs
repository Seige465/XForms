using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
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
    public sealed partial class Select1 : UserControl
    {
        
        List<ControlOptions> _options;
        string group = Guid.NewGuid().ToString();
        bool _isReadOnly;
        WidgetMaster _master;      
        public Select1(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            _options = DLL.GetControlOptions(_master._control.id);
            if (isReadOnly)
                lblValuePlaceholder.IsTapEnabled = false;
            SetAppearance();
        }

        private void SetAppearance()
        {
            if (_master._control.appearance == null) _master._control.appearance = string.Empty;
            string val = XForm.GetValue(_master._binding.nodeset);
            switch (_master._control.appearance)
            {
                //case "horizontal-compact":
                //case "minimal":
                //    //SEGMENT CONTROL!
                //    SetOptions();
                //    stkpnlItems.Orientation = Orientation.Horizontal;
                //    break;
                case "compact":
                    lbxInline.ItemsSource = _options;
                    brdrValuePlaceholder.Visibility = Visibility.Visible;                    
                    lblValuePlaceholder.Text = !string.IsNullOrWhiteSpace(val) ? val : "";
                    break;
                case "full":
                default:
                    _master._control.appearance = "full";
                    brdrValuePlaceholder.Visibility = Visibility.Visible;
                    lblValuePlaceholder.Text = !string.IsNullOrWhiteSpace(val) ? val : "";
                    break;
            }
        }

        private void lbxInline_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (lbxInline.SelectedItem == null)
                return;
            ControlOptions selectedOption = (ControlOptions)lbxInline.SelectedItem;
            lblValuePlaceholder.Text = selectedOption.label;
            Hide.Begin();
            _master.UpdateValue(selectedOption.value);
            
        }
        private void lbxInline_LostFocus(object sender, RoutedEventArgs e)
        {
            Hide.Begin();
        }

        private void _parent_PopupChanged(object sender, EventArgs e)
        {
            _master._parent.PopupChanged -= _parent_PopupChanged;
            if (!(bool)((PopupControls.PopupEventArgs)e).Save)
                return;
            if (e != null && ((PopupControls.PopupEventArgs)e).Value != null && !string.IsNullOrWhiteSpace(((PopupControls.PopupEventArgs)e).Value.ToString()))
            {
                string val = ((PopupControls.PopupEventArgs)e).Value.ToString();
                lblValuePlaceholder.Text = _options.Find(x => x.value == val).label;
                _master.UpdateValue(val);
            }
        }

        private void DoubleAnimation_Completed(object sender, object e)
        {
            lbxInline.Visibility = Visibility.Collapsed;
        }

   

        private void lblValuePlaceholder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_master._control.appearance) || _master._control.appearance == "full")
            {
                _master._parent.PopupChanged += _parent_PopupChanged;
                _master._parent.ShowPopup(this, XFormMaster.PopupType.Radio, _master._control.label, _options, XForm.GetValue(_master._binding.nodeset));                                
            }
            else
            {
                lbxInline.Visibility = Visibility.Visible;
                Show.Begin();
                lbxInline.Focus(FocusState.Programmatic);                
            }
        }
    }
}
