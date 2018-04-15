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
    public sealed partial class Select : UserControl
    {      
        List<ControlOptions> _options;
        List<string> _placeholder = new List<string>();
        List<string> _value = new List<string>();
        WidgetMaster _master;        
        public Select(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            _options = DLL.GetControlOptions(_master._control.id);
            if (isReadOnly)
                lblValuePlaceholder.IsTapEnabled = false;
            SetValues();
        }
       
        private void SetValues()
        {
            string value = XForm.GetValue(_master._binding.nodeset);
            if (string.IsNullOrWhiteSpace(value))
                return;
            _value = value.Split('|').ToList();
            foreach (string v in _value)
            {
                ControlOptions co = _options.Find(o => o.value == v.Trim());
                _placeholder.Add(co.label);
            }            
            lblValuePlaceholder.Text = _placeholder.Count > 0 ? string.Join(", ", _placeholder) : "";            
        }        

        private void lblValuePlaceholder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _master._parent.PopupChanged += _parent_PopupChanged;
            _master._parent.ShowPopup(this, XFormMaster.PopupType.Checkbox, _master._control.label, _options, string.Join("|", _value));
        }
        private void _parent_PopupChanged(object sender, EventArgs e)
        {
            _master._parent.PopupChanged -= _parent_PopupChanged;
            if (!(bool)((PopupControls.PopupEventArgs)e).Save)
                return;
            object value = ((PopupControls.PopupEventArgs)e).Value;
            if (value == null)
                return;
            _value = value.ToString().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            _placeholder.Clear();
            _options.Select(o => { o.selected = false; return o; }).ToList();
            foreach (string selected in _value)
            {
                var option = _options.Find(x => x.value == selected);
                option.selected = true;
                _placeholder.Add(option.label);
            }
            lblValuePlaceholder.Text = string.Join(", ", _placeholder);
            _master.UpdateValue(string.Join("|", _value));
        }        
    }
}
