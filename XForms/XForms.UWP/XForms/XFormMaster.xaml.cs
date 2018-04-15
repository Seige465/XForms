using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XForms.UWP.XForms.PopupControls;
using XForms.XForms;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace XForms.UWP.XForms
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class XFormMaster : Page, IXFormBuild
    {
        FormLoadArgs _formArgs;        
        NavigationMode navigationMode;
        public XFormMaster()
        {
            this.InitializeComponent();
            masterScrollviewer.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 238, 238, 238));
        }
        
        public event EventHandler FormChanged;
        public event EventHandler PopupChanged;
        private void OnFormChanged()
        {
            FormChanged?.Invoke(null, null);
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //Update groups bindings.
                foreach (StackPanel grp in pnlFormMain.Children.Where(x => x is StackPanel))
                {
                    int groupid = (int)grp.Tag;
                    Groups group = DLL.GetGroup(groupid);
                    Bindings binding = DLL.GetBindingByReference(group.reference);
                    if (binding != null && binding.relevant != null)
                    {
                        var evaluation = XForm.Evaluate(binding.relevant);
                        if (evaluation != null)
                        {
                        
                                grp.Visibility = (bool)evaluation ? Visibility.Visible : Visibility.Collapsed;
             
                        }
                    }                    
                }
            });
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationMode = e.NavigationMode;
            _formArgs = e.Parameter as FormLoadArgs;            
            if (_formArgs.group.HasValue)
                btnBack.Visibility = Visibility.Visible;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            pbrProgress.Visibility = Visibility.Visible;
            pbrProgress.IsActive = true;            
            await Task.Delay(100);
            pnlFormMain.Children.Clear();
            XForm.LoadForm(_formArgs.group, this);
            FireFormChangeEvent(null);
            pbrProgress.Visibility = Visibility.Collapsed;
            pbrProgress.IsActive = false;
        }      

        #region Interface Methods
        void IXFormBuild.SetTitle(string title)
        {
            lblFormTitle.Text = title;
        }
        void IXFormBuild.AddControl(Controls control, int? parentSectionID)
        {
            string parentName;
            if (parentSectionID == null || parentSectionID == -1)
                parentName = "pnlFormMain";
            else parentName = parentSectionID.ToString();
            StackPanel parentSection = this.FindName(parentName) as StackPanel;

            parentSection.Children.Add(new Widgets.WidgetMaster(control, this, _formArgs.isReadonly));

            //switch ((ControlType)control.type)
            //{
            //    case ControlType.input:
            //        //Get the binding
            //        Bindings binding = XForm.GetBindingForControl(control);
            //        switch ((BindType)binding.type)
            //        {
            //            case BindType.xGeoPoint:                            
            //                parentSection.Children.Add(new Widgets.WidgetMaster(control, this, _formArgs.isReadonly));
            //                break;
            //            default:
            //                parentSection.Children.Add(new Widgets.WidgetMaster(control, this, _formArgs.isReadonly));
            //                //var input = new Widgets.Input(control, this, _formArgs.isReadonly);
            //                //parentSection.Children.Add(input);
            //                break;
            //        }
            //        break;
            //    case ControlType.select:
            //        parentSection.Children.Add(new Widgets.WidgetMaster(control, this, _formArgs.isReadonly));
            //        //parentSection.Children.Add(new Widgets.Select(control, this, _formArgs.isReadonly));
            //        break;
            //    case ControlType.select1:
            //        parentSection.Children.Add(new Widgets.WidgetMaster(control, this, _formArgs.isReadonly));
            //        //parentSection.Children.Add(new Widgets.Select1(control, this, _formArgs.isReadonly));
            //        break;
            //    case ControlType.upload:                    
            //        switch (control.appearance)
            //        {
            //            case "draw":
            //                parentSection.Children.Add(new XForms.Widgets.Upload_Draw(control, this, _formArgs.isReadonly));
            //                break;
            //            case "signature":
            //                parentSection.Children.Add(new XForms.Widgets.Upload_Draw(control, this, _formArgs.isReadonly));
            //                break;
            //            case "annotate":
            //                parentSection.Children.Add(new XForms.Widgets.Upload_Draw(control, this, _formArgs.isReadonly));
            //                break;
            //            case "textannotate":
            //                parentSection.Children.Add(new XForms.Widgets.Upload_Draw(control, this, _formArgs.isReadonly));
            //                break;
            //            case "image":
            //            default:
            //                parentSection.Children.Add(new XForms.Widgets.Upload_Image(control, this, _formArgs.isReadonly));
            //                break;
            //        }
            //        break;
            //}
        }
        void IXFormBuild.AddGroup(int groupid)
        {            
            Groups group = DLL.GetGroup(groupid);
            StackPanel parentSection = pnlFormMain;
            //Create a group 
            StackPanel groupPanel = new StackPanel();
            groupPanel.Name = group.id.ToString();
            groupPanel.Tag = group.id;
            groupPanel.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

            groupPanel.BorderThickness = new Thickness(1);
            groupPanel.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 80, 80, 80));

            groupPanel.Padding = new Thickness(2, 5, 2, 5);
            groupPanel.Margin = new Thickness(4);


            TextBlock groupTitle = new TextBlock();
            groupTitle.Text = group.label;
            //groupTitle.FontSize = groupTitle.FontSize * 1.25;
            groupTitle.Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"];
            groupTitle.FontWeight = Windows.UI.Text.FontWeights.Bold;
            groupTitle.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30));
            groupTitle.Margin = new Thickness(8, 2, 2, 2);


            groupPanel.Children.Add(groupTitle);
            parentSection.Children.Add(groupPanel);
            //parentSection = groupPanel;
        }
        void IXFormBuild.AddPageStub(int groupid, int? parentSectionID)
        {
            string parentName;
            if (parentSectionID == null || parentSectionID == -1)
                parentName = "pnlFormMain";
            else parentName = parentSectionID.ToString();
            Groups group = DLL.GetGroup(groupid);
            StackPanel parentSection = this.FindName(parentName) as StackPanel;
            parentSection.Children.Add(new Widgets.Page(group, this));
        }
        #endregion
        
        #region Popup Code
        public object _popupValue = null;
        public PopupType _popupType;
        public void ShowPopup(UserControl sender, PopupType type, string title, object subtype, object value)
        {
            _popupValue = null;
            _popupType = type;
            switch (type)
            {
                case PopupType.Map:
                    popupContent.Content = new Map(this, value);
                    break;
                case PopupType.Canvas:                    
                    popupContent.Content = new PopupControls.Canvas(this, subtype.ToString(), value);
                    break;
                case PopupType.Checkbox:
                    popupContent.Content = new PopupControls.Select(this, type, title, subtype, value);
                    break;
                case PopupType.Radio:
                    popupContent.Content = new PopupControls.Select(this, type, title, subtype, value);
                    break;
                case PopupType.Loading:
                    popupContent.Content = new PopupControls.Loading(this, title, subtype);
                    break;
                case PopupType.DateTime:   
                    popupContent.Content = new XForms.Popup.Controls.DateTimePicker(this, title, (ValueTuple<int?, string>)subtype, (DateTime?)value);
                    break;
                default:
                    break;
            }            
            ppup.IsOpen = true;
        }
        private async void ppup_Closed(object sender, object e)
        {            
            await Task.Delay(200);            
            PopupChanged?.Invoke(sender, new PopupEventArgs() { Value = _popupValue });
        }
        public void ClosePopup(bool save = true)
        {
            if (save)
            {
                ppup.IsOpen = false;
                return;
            }
            ppup.Closed -= ppup_Closed;
            ppup.IsOpen = false;
            PopupChanged?.Invoke(null, new PopupEventArgs() { Save = save, Value = _popupValue});
            ppup.Closed += ppup_Closed;
        }
        public void PopupAllowLightClose(bool allow)
        {
            ppup.IsLightDismissEnabled = allow;
        }
        public void PopupChangeValue(DependencyProperty property, object value)
        {
            ppup.SetValue(property, value);
        }
        public enum PopupType
        {
            Map,
            Canvas,
            Radio,
            Checkbox,
            Loading,
            DateTime
        }
        #endregion
        
        public void Page_Navigate(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(XFormMaster), new FormLoadArgs() { group = (int)sender, isReadonly = _formArgs.isReadonly });
        }
        public void FireFormChangeEvent(object sender)
        {
            Task.Run(() =>
            {
                XForm.RunCalculates();
                DLL.UpdateInstance(XForm.instance);
                OnFormChanged();
            });
        }
        private void btnBack_Tapped(object sender, TappedRoutedEventArgs e)
        {               
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
        
        #region Button events (NOT XFORM)
        private void btnFireEvent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FireFormChangeEvent(sender);
        }
        private async void btnSaveInstance_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("XML", new List<string>() { ".xml" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Instance XML";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, XForm.instance.instance.ToString());
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    Console.WriteLine("File saved");
                }
                else
                {

                }
            }
            else
            {

            }
        }
        private async void btnSaveDBInstance_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("XML", new List<string>() { ".xml" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Database XML";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, DLL.GetInstance().instance.ToString());
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    Console.WriteLine("File saved");
                }
                else
                {

                }
            }
            else
            {

            }
        }
        private void btnBackToFormSelection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
        
        #endregion

        public async void ShowHintDialog(string title, string content)
        {
            ContentDialog helpDialog = new ContentDialog();
            helpDialog.IsPrimaryButtonEnabled = true;
            helpDialog.CloseButtonText = "OK";
            helpDialog.Title = title;
            
            TextBlock contents = new TextBlock();
            contents.TextWrapping = TextWrapping.Wrap;

            if (content.Contains("<a"))
            {
                List<int> openLinks = content.AllIndexesOf("<a").ToList();
                List<int> closeLinks = content.AllIndexesOf("</a>").ToList();
                List<Inline> runs = new List<Inline>();
                int position = 0;
                for (int i = 0; i < openLinks.Count; i++)
                {
                    runs.Add(new Run() { Text = content.Substring(position, openLinks[i] - position) });
                    int substringlength = closeLinks[i] - openLinks[i];
                    string asubstring = content.Substring(openLinks[i], substringlength + 4);
                    XElement href = XElement.Parse(asubstring);

                    Run hlinkrun = new Run();
                    hlinkrun.Text = href.Value;

                    Hyperlink hlink = new Hyperlink();
                    hlink.NavigateUri = new System.Uri(href.Attribute("href").Value);
                    hlink.Inlines.Add(hlinkrun);
                    runs.Add(hlink);

                    position = closeLinks[i] + 4;
                }
                if (position < content.Length)
                    runs.Add(new Run() { Text = content.Substring(position, content.Length - position) });

                contents.Inlines.Clear();
                foreach (Inline inline in runs)
                {                    
                    contents.Inlines.Add(inline);
                }
            }
            else
            {
                contents.Text = content;
            }
            helpDialog.Content = contents;
            await helpDialog.ShowAsync();           
        }
    }
}
