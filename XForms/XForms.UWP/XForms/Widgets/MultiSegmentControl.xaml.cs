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
    public sealed partial class MultiSegmentControl : UserControl
    {
        List<ControlOptions> _options;
        string group = Guid.NewGuid().ToString();
        bool _isReadOnly;
        WidgetMaster _master;
        List<string> values = new List<string>();
        public MultiSegmentControl(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            _options = DLL.GetControlOptions(_master._control.id);
            _isReadOnly = isReadOnly;
            SetOptions();
        }
        private void SetOptions()
        {
            List<string> currentvalue = XForm.GetValue(_master._binding.nodeset)?.Split('|').ToList();
            for (int i = 0; i < _options.Count; i++)
            {
                CheckBox check = new CheckBox();
                if (i == 0)
                    check.Style = (Style)this.Resources["MultiSegmentFirst"];
                else if (i == _options.Count - 1)
                    check.Style = (Style)this.Resources["MultiSegmentLast"];
                else
                    check.Style = (Style)this.Resources["MultiSegmentMiddle"];
                check.Content = _options[i].label;
                check.Tag = _options[i].value;
                if (currentvalue != null && currentvalue.Contains(_options[i].value))
                    check.IsChecked = true;
                if (_isReadOnly)
                    check.IsEnabled = false;
                else
                {                    
                    check.Checked += Check_Checked;
                    check.Unchecked += Check_Unchecked;
                }
                stkOptions.Children.Add(check);
            }
        }

       

        private void Check_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            values.Add(check.Tag.ToString());
            _master.UpdateValue(string.Join("|", values));
        }
        private void Check_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            values.Remove(check.Tag.ToString());
            _master.UpdateValue(string.Join("|", values));
        }

    }
}
