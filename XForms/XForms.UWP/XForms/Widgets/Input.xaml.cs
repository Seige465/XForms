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
    public sealed partial class Input : UserControl
    {
        WidgetMaster _master;
        BindType _type;
        DateTime? _value;
        public Input(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            _type = (BindType)_master._binding.type.Value;
            txtInput.Text = string.Empty;
            SetStyle();
            if (isReadOnly)
                txtInput.IsEnabled = txtInput.IsTapEnabled = false;
        }

        
       
        private void SetStyle()
        {
            switch (_type)
            {
                case BindType.xString: //Normal textbox                
                    txtInput.Text = XForm.GetValue(_master._binding.nodeset);
                    txtInput.PlaceholderText = _master._control.hint ?? string.Empty;
                    SetStringAppearance();
                    break;
                case BindType.xInt:
                    txtInput.Text = XForm.GetValue(_master._binding.nodeset);
                    txtInput.PlaceholderText = _master._control.hint ?? string.Empty;
                    txtInput.TextChanging += IntegerTextbox_TextChanging;
                    SetIntAppearance();
                    break;
                case BindType.xDecimal:
                    txtInput.Text = XForm.GetValue(_master._binding.nodeset);
                    txtInput.PlaceholderText = _master._control.hint ?? string.Empty;
                    SetDecimalAppearance();
                    break;
                case BindType.xDateTime:
                case BindType.xDate:
                case BindType.xTime:
                    //TODO: set up for sending to popup.
                    txtInput.LostFocus -= txtInput_LostFocus;
                    brdrValuePlaceholder.Visibility = Visibility.Visible;
                    txtInput.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }


        private void IntegerTextbox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (!args.IsContentChanging)
                return;
            List<char> acceptedChars = new List<char>() { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            List<char> lastchar = sender.Text.ToList();
            lastchar.RemoveAll(x => !acceptedChars.Contains(x));
            sender.Text = string.Join("", lastchar);            
        }

        private void SetStringAppearance()
        {
            if (_master._binding.isreadonly != null && (bool)XForm.Evaluate(_master._binding.isreadonly))
                txtInput.Visibility = Visibility.Collapsed;
            if (_master._control.appearance == null)
                return;
            switch (_master._control.appearance.ToLower())
            {
                case null:
                    break;
                case "multiline":
                    txtInput.SetValue(Grid.RowProperty, 1);
                    txtInput.SetValue(Grid.ColumnProperty, 0);
                    txtInput.SetValue(Grid.ColumnSpanProperty, 99);                    
                    txtInput.AcceptsReturn = true;                    
                    break;
                case "numbers":
                    txtInput.InputScope = new InputScope() { Names = { new InputScopeName(InputScopeNameValue.Digits) } };
                    break;
            }
            
        }
        private void SetIntAppearance()
        {
            if (_master._binding.isreadonly != null && (bool)XForm.Evaluate(_master._binding.isreadonly))
                txtInput.Visibility = Visibility.Collapsed;
            txtInput.InputScope = new InputScope() { Names = { new InputScopeName(InputScopeNameValue.Number) } };
            if (_master._control.appearance == null)
                return;
        }
        private void SetDecimalAppearance()
        {
            if (_master._binding.isreadonly != null && (bool)XForm.Evaluate(_master._binding.isreadonly))
                txtInput.Visibility = Visibility.Collapsed;
            txtInput.InputScope = new InputScope() { Names = { new InputScopeName(InputScopeNameValue.Digits) } };
            if (_master._control.appearance == null)
                return;
        }
        
        private void lblInput_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _master._parent.PopupChanged += _parent_PopupChanged;
            var data = ( type : _master._binding.type, appearance: _master._control.appearance);
            _master._parent.ShowPopup(this, XFormMaster.PopupType.DateTime, _master._control.label, data,_value);
        }

        private void _parent_PopupChanged(object sender, EventArgs e)
        {
            _master._parent.PopupChanged -= _parent_PopupChanged;
            if (!(bool)((PopupControls.PopupEventArgs)e).Save)
                return;
            if (e == null || ((PopupControls.PopupEventArgs)e).Value == null || string.IsNullOrWhiteSpace(((PopupControls.PopupEventArgs)e).Value.ToString()))
                return;
            _value = (DateTime)((PopupControls.PopupEventArgs)e).Value;
            string val = string.Empty;
            switch ((BindType)_master._binding.type)
            {
                case BindType.xDateTime:
                    val = _value.Value.ToString("s") + "Z";
                    break;
                case BindType.xDate:
                    val = _value.Value.ToString("yyyy-MM-dd");
                    break;
                case BindType.xTime:
                    val = _value.Value.ToString("HH:mm:ssZ");
                    break;
            }
            lblInput.Text = val;
            _master.UpdateValue(val);
        }

        private void txtInput_LostFocus(object sender, RoutedEventArgs e)
        {
            _master.UpdateValue(txtInput.Text);
        }
        
        private void txtInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (_type)
            {
                case BindType.xInt:
                    if(!Keys.Numeric.Contains(e.Key) && !Keys.Override.Contains(e.Key))
                        e.Handled = true;                    
                    break;
                case BindType.xDecimal:
                    if (!Keys.Numeric.Contains(e.Key) && !Keys.Decimal.Contains(e.Key) && !Keys.Override.Contains(e.Key))
                        e.Handled = true;
                    break;
            }
        }
        
    }
}
