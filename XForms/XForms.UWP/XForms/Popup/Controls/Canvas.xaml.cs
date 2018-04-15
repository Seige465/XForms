using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XForms.UWP.XForms.PopupControls
{
    public sealed partial class Canvas : UserControl
    {
        XFormMaster _parent;
        string _appearance;
        string _filename;
        string _annotation;
        //bool _cancel = false;
        public Canvas(XFormMaster parent, string appearance, object value)
        {
            this.InitializeComponent();
            _parent = parent;            
            _appearance = appearance;
            grdMain.Height = _parent.Frame.ActualHeight * 0.9;
            grdMain.Width = _parent.Frame.ActualWidth * 0.9;
            grdMain.Margin = new Thickness(_parent.Frame.ActualWidth * 0.05, _parent.Frame.ActualHeight * 0.05, _parent.Frame.ActualWidth * 0.05, _parent.Frame.ActualHeight * 0.05);

            if (_appearance.Equals("draw", StringComparison.OrdinalIgnoreCase))
            {
                calligraphyPen.Visibility = Visibility.Collapsed;
            }
            else if (_appearance.Equals("signature", StringComparison.OrdinalIgnoreCase))
            {
                itbHighlighter.Visibility = itbStencil.Visibility = itbPencil.Visibility = itbRuler.Visibility = itbPen.Visibility = Visibility.Collapsed;
                inkToolbar.ActiveTool = calligraphyPen;
            }
            

            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;

            if (string.IsNullOrWhiteSpace(value.ToString()))
                _filename = Guid.NewGuid().ToString() + ".jpeg";
            else
                SetBackgroundImage(value.ToString());
            
            //Check if the image allows annotation
            if (_appearance.Equals("annotate", StringComparison.OrdinalIgnoreCase))
            {
                btnText.Visibility = Visibility.Visible;
                //Set the annotation values;
                GetAnnotation();
            }
        }
        private void SetBackgroundImage(string filepath)
        {
            _filename = Path.GetFileName(filepath);
            BitmapImage bitmapImage = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.IgnoreImageCache,
                UriSource = new Uri(filepath, UriKind.Absolute)
            };
            imgCanvas.Source = bitmapImage;
        }
        #region EXIF 2.0
        private async void GetAnnotation()
        {
            //string annotation = await GetFileProperty("annotation");
            //txtAnnotation.Text = annotation ?? string.Empty;
            txtAnnotation.Text = await GetExifTitle();
        }
        private async void SetExifTitle()
        {
            StorageFile inputFile = null;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            try
            {
                inputFile = await LoadStorageFile(_filename);
            }
            catch { return; }
            if (inputFile == null)
                return;
            ImageProperties props = await inputFile.Properties.GetImagePropertiesAsync();
            string title = props.Title;            
            try
            {
                props.Title = txtAnnotation.Text;
                await props.SavePropertiesAsync();
            }
            catch
            { App.OutputWrite("Unable to write to image title"); }
            
        }
        private async Task<string> GetExifTitle()
        {
            StorageFile inputFile = null;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            try
            {
                inputFile = await LoadStorageFile(_filename);
            }
            catch { return string.Empty; }
            if (inputFile == null)
                return string.Empty;
            ImageProperties props = await inputFile.Properties.GetImagePropertiesAsync();
            string title = props.Title;
            if (title == null)
            {
                return string.Empty;
            }
            return title;
        }
        #endregion

        //private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        //{            
        //    SaveImage();            
        //}

        int height, width;
        public async Task<bool> SaveImage()
        {
            StorageFile inputFile = null;
            inputFile = await LoadStorageFile(_filename);
            double scalex = 0;
            double scaley = 0;
            double offsety = 0;
            double offsetx = 0;
            if (inputFile != null)
            {
                var prop = await inputFile.Properties.GetImagePropertiesAsync();
                offsetx = (double)prop.Width - width;
                offsety = (double)prop.Height - height;
                scalex = ((double)prop.Width - width) / width;
                scaley = ((double)prop.Height - height) / height;
                width = (int)prop.Width;
                height = (int)prop.Height;
            }


            //FUCK WITH THE STROKES ON THE CANVAS!
            //inkCanvasScaleTransform.ScaleX += scalex;
            //inkCanvasScaleTransform.ScaleY += scaley;
            //inkCanvasScaleTransform.TransformBounds(new Rect(0, 0, width, height));


            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            List<InkStroke> ExpandedStrokes = new List<InkStroke>();
            foreach (var stroke in strokes)
            {
                List<InkPoint> ExpandedPoints = new List<InkPoint>();
                var points = stroke.GetInkPoints();
                foreach (InkPoint point in points)
                {
                    InkPoint newPoint = new InkPoint(new Point(point.Position.X, point.Position.Y), point.Pressure, point.TiltX, point.TiltY, point.Timestamp);
                    ExpandedPoints.Add(newPoint);
                }
                InkStroke strk = new InkStrokeBuilder().CreateStrokeFromInkPoints(ExpandedPoints.AsEnumerable(), Matrix3x2.CreateScale(1 + (float)scalex, 1 + (float)scaley));
                strk.DrawingAttributes = stroke.DrawingAttributes;
                ExpandedStrokes.Add(strk);
            }
            
            //InkStrokeContainer container = new InkStrokeContainer();
            //container.AddStrokes(ExpandedStrokes.AsEnumerable());

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, width, height, 96);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                if (inputFile != null)
                {
                    using (CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, 96))
                    {
                        ds.DrawImage(image);
                    }
                }
                ds.DrawInk(ExpandedStrokes);
            }
            if (inputFile == null)
                inputFile = await storageFolder.CreateFileAsync(_filename, CreationCollisionOption.ReplaceExisting);
            _parent._popupValue = inputFile.Path;
            using (var fileStream = await inputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            }
            if (_appearance.Equals("annotate", StringComparison.OrdinalIgnoreCase))
                SetExifTitle();
            

            //////Save the canvas image.
            ////CanvasDevice device = CanvasDevice.GetSharedDevice();
            ////CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, width, height, 96);
            ////using (var ds = renderTarget.CreateDrawingSession())
            ////{
            ////    ds.Clear(Colors.White);
            ////    ds.DrawInk(ExpandedStrokes);
            ////    //ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            ////}
            ////string tempFileName = "canvasTemp.png";
            ////StorageFile tempCanvas = await storageFolder.CreateFileAsync(tempFileName, CreationCollisionOption.ReplaceExisting);            
            ////using (var fileStream = await tempCanvas.OpenAsync(FileAccessMode.ReadWrite))
            ////{
            ////    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
            ////}
            
            //////If no background image is currently set.
            ////if (inputFile == null)
            ////{
            ////    await tempCanvas.RenameAsync(_filename);
            ////    if (_appearance.Equals("annotate", StringComparison.OrdinalIgnoreCase))
            ////        SetExifTitle();
            ////    _parent._popupValue = tempCanvas.Path;
            ////    return true;
            ////}
            //////If a background image IS set
            //////Get the size of the set image
           
            ////device = CanvasDevice.GetSharedDevice();
            ////renderTarget = new CanvasRenderTarget(device, width, height, 96);
            //////BitmapDecoder imagedecoder = null;
            //////if (inputFile != null)
            //////{
            //////    using (var imagestream = await inputFile.OpenAsync(FileAccessMode.Read))
            //////    {
            //////        imagedecoder = await BitmapDecoder.CreateAsync(imagestream);
            //////    }                
            //////}
            ////using (var ds = renderTarget.CreateDrawingSession())
            ////{                
            ////    ds.Clear(Colors.White);
            ////    using (CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, 96))
            ////    {                    
            ////        ds.DrawImage(image);                    
            ////    }
            ////    ds.DrawInk(ExpandedStrokes);
            ////    //using (CanvasBitmap image = await CanvasBitmap.LoadAsync(device, tempCanvas.Path, 96))
            ////    //{
            ////    //    ds.DrawImage(image, new System.Numerics.Vector2(0,0), new Rect(0, 0, width, height));//,0.5f, CanvasImageInterpolation.Linear
            ////    //}
            ////}
            ////if (inputFile == null)
            ////    inputFile = await storageFolder.CreateFileAsync(_filename, CreationCollisionOption.ReplaceExisting);            
            ////using (var fileStream = await inputFile.OpenAsync(FileAccessMode.ReadWrite))
            ////{
            ////    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            ////}
            ////_parent._popupValue = inputFile.Path;
            ////if (_appearance.Equals("annotate", StringComparison.OrdinalIgnoreCase))
            ////    SetExifTitle();





            //CanvasDevice device = CanvasDevice.GetSharedDevice();            
            //CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, width, height, 96);
            //using (var ds = renderTarget.CreateDrawingSession())
            //{
            //    ds.Clear(Colors.White);
            //    if (inputFile != null)
            //    {
            //        using (CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, 96))
            //        {                        
            //            double offsetwidth = (width - imgCanvas.ActualWidth) / 2;
            //            ds.DrawImage(image, new System.Numerics.Vector2((float)offsetwidth, 0), new Rect(0, 0, imgCanvas.ActualWidth, imgCanvas.ActualHeight));
            //        }
            //    }
            //    ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            //}
            //if(inputFile == null)
            //    inputFile = await storageFolder.CreateFileAsync(_filename, CreationCollisionOption.ReplaceExisting);
            //_parent._popupValue = inputFile.Path;
            //using (var fileStream = await inputFile.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            //}

            //ResizeImage(inputFile, w, h);

            //if (_appearance.Equals("annotate", StringComparison.OrdinalIgnoreCase))
            //    SetExifTitle();
            return true;
        }

        private async void ResizeImage(StorageFile fileToResize, uint newHeight, uint newWidth)
        {
            //https://gist.github.com/alexsorokoletov/56a120c5562344d60e1a6b3fa75bda2c
            var imageStream = await fileToResize.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile resizedFile = await storageFolder.CreateFileAsync(Guid.NewGuid().ToString() + ".jpeg");
            using (imageStream)
            {
                using (var resizedStream = await resizedFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                    encoder.BitmapTransform.ScaledHeight = newHeight;
                    encoder.BitmapTransform.ScaledWidth = newWidth;
                    encoder.BitmapTransform.Bounds = new BitmapBounds()
                    {
                        Width = newWidth,
                        Height = newHeight,
                        X = 0,
                        Y = 0,
                    };
                    await encoder.FlushAsync();
                }
            }            
        }






        private async Task<StorageFile> LoadStorageFile(string filename)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                return await storageFolder.GetFileAsync(_filename);
            }
            catch { return null; }

        }
        private void btnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this.Unloaded -= UserControl_Unloaded;
            _parent.ClosePopup(false);
           // this.Unloaded += UserControl_Unloaded;
        }

        private async void btnSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this.Unloaded -= UserControl_Unloaded;
            await SaveImage();
            _parent.ClosePopup();
            //this.Unloaded += UserControl_Unloaded;
        }

        private async void btnClear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //TODO: show mesagebox
            //_parent.PopupAllowLightClose(false);
            MessageDialog dialog = new MessageDialog("This will perminately delete all traces of this image.", "Are you sure?");
            dialog.Commands.Add(new UICommand("Yes", null));
            dialog.Commands.Add(new UICommand("No", null));
            dialog.DefaultCommandIndex = 0;
            IUICommand result = await dialog.ShowAsync();
            if (result.Label == "Yes")
            {
                //TODO: Delete the old file from the 
                StorageFile oldImg = await LoadStorageFile(_filename);
                if (oldImg != null)
                    await oldImg.DeleteAsync(StorageDeleteOption.PermanentDelete);
                _filename = Guid.NewGuid().ToString() + ".jpeg";
                imgCanvas.Source = null;
                inkCanvas.InkPresenter.StrokeContainer.Clear();
            }
            //_parent.PopupAllowLightClose(true);
        }

        private void btnText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stkAnnotate.Visibility == Visibility.Visible)
                stkAnnotate.Visibility = Visibility.Collapsed;
            else
                stkAnnotate.Visibility = Visibility.Visible;
        }

        private async void btnTakeImage_Tapped(object sender, TappedRoutedEventArgs e)
        {            
            StorageFile file = await Imaging.ShowCameraDialog();
            if (file == null)
                return;
            stkAnnotate.Visibility = Visibility.Collapsed;
            SetBackgroundImage(file.Path);
            stkAnnotate.Visibility = Visibility.Visible;
        }

        private async void btnLoadImage_Tapped(object sender, TappedRoutedEventArgs e)
        {            
            StorageFile file = await Imaging.ShowImageLoadDialog();
            if (file == null)
                return;            
            SetBackgroundImage(file.Path);
        }

        private void imgCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            inkCanvas.Width = imgCanvas.ActualWidth;
            inkCanvas.Height = imgCanvas.ActualHeight;
            height = (int)inkCanvas.ActualHeight;
            width = (int)inkCanvas.ActualWidth;
        }

        private void inkCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            height = (int)inkCanvas.ActualHeight;
            width = (int)inkCanvas.ActualWidth;
        }
    }
    



    public class CalligraphicPen : InkToolbarCustomPen
    {
        public CalligraphicPen()
        {
        }

        protected override InkDrawingAttributes CreateInkDrawingAttributesCore(Brush brush, double strokeWidth)
        {
            InkDrawingAttributes inkDrawingAttributes = new InkDrawingAttributes();
            inkDrawingAttributes.PenTip = PenTipShape.Circle;
            inkDrawingAttributes.IgnorePressure = false;
            SolidColorBrush solidColorBrush = (SolidColorBrush)brush;
            if (solidColorBrush != null)
            {
                inkDrawingAttributes.Color = solidColorBrush.Color;
            }
            inkDrawingAttributes.Size = new Size(strokeWidth, 2.0f * strokeWidth);
            inkDrawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.CreateRotation((float)(Math.PI * 45 / 180));
            return inkDrawingAttributes;
        }
    }


}
