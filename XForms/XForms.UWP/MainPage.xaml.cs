using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XForms.UWP.XForms;
using XForms.UWP.XForms.Widgets;
using XForms.XForms;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XForms.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //LoadFormToList();
            //LoadTestControl();
           
        }

        //private void LoadTestControl()
        //{
        //    //SegmentControl testControl = new SegmentControl();
        //    //testControl.HorizontalAlignment = HorizontalAlignment.Center;
        //    //testControl.Margin = new Thickness(5);
        //    //grdMain.Children.Add(testControl);
        //    //testControl.SetValue(Grid.RowProperty, 3);
        //}

        //private async void LoadFormToList()
        //{
            
        //    List<string> forms = resp.data.Split('|').ToList();
        //    forms = forms.Select(s => Path.GetFileName(s)).ToList();
        //    forms = forms.OrderByDescending(x => x).ToList();
        //    lbxForms.ItemsSource = forms;
        //}

        private async void btnLoadForm_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await StorageFile.GetFileFromPathAsync(Path.Combine(appInstalledFolder.Path, "XForm.xml"));
            string xmlform = string.Empty;
            using(var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                xmlform = streamReader.ReadToEnd();
            }


            DLL.DisposeTables();
            Parser.Parse(xmlform, "TestForm");
            Forms form = DLL.GetForm(1);
            XForm.instance = new Instance()
            {
                formid = form.id,
                instance = XForm.LoadInstance(form.instancexml),
            };
            DLL.InsertRow(XForm.instance);
            //if(lbxForms.SelectedItem.ToString().ToLower().Contains("asset"))
            //    Frame.Navigate(typeof(XFormMaster), new FormLoadArgs() { isReadonly = true });
            //else 
                Frame.Navigate(typeof(XFormMaster), new FormLoadArgs());
        }

        private void btnLoadForms_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //LoadFormToList();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            btnLoadForm_Tapped(null, null);
        }
    }
}