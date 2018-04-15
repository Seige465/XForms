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
using static XForms.UWP.XForms.XFormMaster;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.PopupControls
{
    public sealed partial class Select : UserControl
    {
        XFormMaster _parent;
        string _selectedValue = string.Empty;
        List<ControlOptions> _options = new List<ControlOptions>();
        PopupType _type;
        public Select(XFormMaster parent, PopupType type, string title, object options, object value)
        {
            this.InitializeComponent();
            _parent = parent;
            this.Height = parent.Frame.ActualHeight * 0.8;
            this.Width = parent.Frame.ActualWidth * 0.8;
            this.Margin = new Thickness(parent.Frame.ActualWidth * 0.1, parent.Frame.ActualHeight * 0.1, parent.Frame.ActualWidth * 0.1, parent.Frame.ActualHeight * 0.1);
            _options = options as List<ControlOptions>;
            _type = type;
            lblHeader.Text = title;
            switch (_type)
            {
                case PopupType.Radio:                                        
                    lbxSelect1.ItemsSource = _options;
                    lbxSelect1.Visibility = Visibility.Visible;
                    lbxSelect1.SelectedItem = _options.Find(x => x.value == value as string);                    
                    break;
                case PopupType.Checkbox:
                    _selectedValue = value.ToString();
                    SetOptions();
                    svwSelect.Visibility = Visibility.Visible;
                    break;
            }            
        }


        private void SetOptions()
        {
            List<string> selections = _selectedValue.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            foreach (ControlOptions option in _options)
            {
                CheckBox check = new CheckBox()
                {
                    Content = option.label,
                    Tag = option.value
                };
                check.Margin = new Thickness(10, 3, 5, 3);
                if (selections.Contains(option.value))
                    check.IsChecked = true;
                check.Checked += CheckBox_Checked;
                check.Unchecked += CheckBox_Unchecked;
                stkSelect.Children.Add(check);                
            }
        }
        private void lbxSelect1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxSelect1.SelectedItem != null)
                _parent._popupValue = ((ControlOptions)lbxSelect1.SelectedItem).value;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            List<string> selections = _selectedValue.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            selections.Add(cbx.Tag.ToString());
            _selectedValue = string.Join("|", selections);
            _parent._popupValue = string.Join(", ", _selectedValue);
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            List<string> selections = _selectedValue.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            selections.Remove(cbx.Tag.ToString());
            _selectedValue = string.Join("|", selections);
            _parent._popupValue = string.Join(", ", _selectedValue);
        }

        private void btnSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent.ClosePopup();
        }
        private void btnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _parent._popupValue = null;
            _parent.ClosePopup(false);
        }

    }
}
