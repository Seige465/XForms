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
using Windows.Media.Capture;
using Windows.Storage;
using XForms.XForms;
using System.Xml.Linq;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Popups;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.Widgets
{
    public sealed partial class Upload_Image : UserControl
    {
        WidgetMaster _master;
        public Upload_Image(WidgetMaster master, bool isReadOnly)
        {
            this.InitializeComponent();
            _master = master;
            string value = XForm.GetValue(_master._binding.nodeset);            
            lblName.Text = string.IsNullOrWhiteSpace(value) ? "No Image Taken" : Path.GetFileName(value);            
            if (isReadOnly)
            {
                lblName.IsHitTestVisible = false;
            }
        }
       
        private async void lblName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //show camera / pictures folder
            StorageFile imageFile = null;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            MessageDialog dialog = new MessageDialog("Take a new picture or select from library?", "New or Library");
            dialog.Commands.Add(new UICommand("Picture", null));
            dialog.Commands.Add(new UICommand("Library", null));
            dialog.DefaultCommandIndex = 0;
            IUICommand result = await dialog.ShowAsync();
            if (result.Label == "Library")
            {
                StorageFile file = await Imaging.ShowImageLoadDialog();
                if (file == null)
                    return;
                imageFile = file;
            }
            else
            {
                StorageFile photo = await Imaging.ShowCameraDialog();
                if (photo == null)
                    return;
                imageFile = photo;
            }            
            BitmapImage bitmapImage = new BitmapImage();            
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmapImage.UriSource = new Uri(imageFile.Path, UriKind.Absolute);
            lblName.Text = imageFile.Name;
            imgImage.Source = bitmapImage;
            _master.UpdateValue(imageFile.Path);
        }
    }
}
