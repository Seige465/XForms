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
    public sealed partial class SegmentControl : UserControl
    {
        List<ControlOptions> _options;
        string group = Guid.NewGuid().ToString();
        bool _isReadOnly;
        WidgetMaster _master;
        public SegmentControl(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            _options = DLL.GetControlOptions(_master._control.id);
            _isReadOnly = isReadOnly;
            SetOptions();
        }

        private void SetOptions()
        {
            //Get the current value
            string currentvalue = XForm.GetValue(_master._binding.nodeset);
            for (int i = 0; i < _options.Count; i++)
            {
                RadioButton radio = new RadioButton();
                if(i == 0)
                    radio.Style = (Style)this.Resources["RadioSegmentFirst"];
                else if (i == _options.Count - 1)
                    radio.Style = (Style)this.Resources["RadioSegmentLast"];
                else
                    radio.Style = (Style)this.Resources["RadioSegmentMiddle"];
                radio.GroupName = group;
                radio.Content = _options[i].label;
                radio.Tag = _options[i].value;
                if (currentvalue != null && _options[i].value == currentvalue)
                    radio.IsChecked = true;
                if (_isReadOnly)
                    radio.IsEnabled = false;
                else                    
                    radio.Checked += Radio_Checked;
                stkOptions.Children.Add(radio);
            }
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            _master.UpdateValue(((RadioButton)sender).Tag.ToString());
        }
    }
}
