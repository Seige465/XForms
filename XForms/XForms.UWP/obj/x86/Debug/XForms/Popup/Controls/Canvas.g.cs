﻿#pragma checksum "S:\CJ Backup\Projects\XForms\XForms\XForms.UWP\XForms\Popup\Controls\Canvas.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "040EF1957F7975E77F1FC7D7464C3C34"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XForms.UWP.XForms.PopupControls
{
    partial class Canvas : 
        global::Windows.UI.Xaml.Controls.UserControl, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Controls_InkToolbar_TargetInkCanvas(global::Windows.UI.Xaml.Controls.InkToolbar obj, global::Windows.UI.Xaml.Controls.InkCanvas value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Controls.InkCanvas) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Controls.InkCanvas), targetNullValue);
                }
                obj.TargetInkCanvas = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(global::Windows.UI.Xaml.Controls.TextBlock obj, global::Windows.UI.Xaml.Media.Brush value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.Brush) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.Brush), targetNullValue);
                }
                obj.Foreground = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        private class Canvas_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            ICanvas_Bindings
        {
            private global::XForms.UWP.XForms.PopupControls.Canvas dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.InkToolbar obj3;
            private global::Windows.UI.Xaml.Controls.TextBlock obj23;

            private Canvas_obj1_BindingsTracking bindingsTracking;

            public Canvas_obj1_Bindings()
            {
                this.bindingsTracking = new Canvas_obj1_BindingsTracking(this);
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 3:
                        this.obj3 = (global::Windows.UI.Xaml.Controls.InkToolbar)target;
                        break;
                    case 23:
                        this.obj23 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            // ICanvas_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::XForms.UWP.XForms.PopupControls.Canvas)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::XForms.UWP.XForms.PopupControls.Canvas obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_inkCanvas(obj.inkCanvas, phase);
                    }
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_calligraphyPen(obj.calligraphyPen, phase);
                    }
                }
            }
            private void Update_inkCanvas(global::Windows.UI.Xaml.Controls.InkCanvas obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_InkToolbar_TargetInkCanvas(this.obj3, obj, null);
                }
            }
            private void Update_calligraphyPen(global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_calligraphyPen(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_calligraphyPen_SelectedBrush(obj.SelectedBrush, phase);
                    }
                }
            }
            private void Update_calligraphyPen_SelectedBrush(global::Windows.UI.Xaml.Media.Brush obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(this.obj23, obj, null);
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
            private class Canvas_obj1_BindingsTracking
            {
                private global::System.WeakReference<Canvas_obj1_Bindings> WeakRefToBindingObj; 

                public Canvas_obj1_BindingsTracking(Canvas_obj1_Bindings obj)
                {
                    WeakRefToBindingObj = new global::System.WeakReference<Canvas_obj1_Bindings>(obj);
                }

                public void ReleaseAllListeners()
                {
                    UpdateChildListeners_calligraphyPen(null);
                }

                public void DependencyPropertyChanged_calligraphyPen_SelectedBrush(global::Windows.UI.Xaml.DependencyObject sender, global::Windows.UI.Xaml.DependencyProperty prop)
                {
                    Canvas_obj1_Bindings bindings;
                    if (WeakRefToBindingObj.TryGetTarget(out bindings))
                    {
                        global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton obj = sender as global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton;
                        if (obj != null)
                        {
                            bindings.Update_calligraphyPen_SelectedBrush(obj.SelectedBrush, DATA_CHANGED);
                        }
                    }
                }
                private global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton cache_calligraphyPen = null;
                private long tokenDPC_calligraphyPen_SelectedBrush = 0;
                public void UpdateChildListeners_calligraphyPen(global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton obj)
                {
                    if (obj != cache_calligraphyPen)
                    {
                        if (cache_calligraphyPen != null)
                        {
                            cache_calligraphyPen.UnregisterPropertyChangedCallback(global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton.SelectedBrushProperty, tokenDPC_calligraphyPen_SelectedBrush);
                            cache_calligraphyPen = null;
                        }
                        if (obj != null)
                        {
                            cache_calligraphyPen = obj;
                            tokenDPC_calligraphyPen_SelectedBrush = obj.RegisterPropertyChangedCallback(global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton.SelectedBrushProperty, DependencyPropertyChanged_calligraphyPen_SelectedBrush);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2:
                {
                    this.grdMain = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3:
                {
                    this.inkToolbar = (global::Windows.UI.Xaml.Controls.InkToolbar)(target);
                }
                break;
            case 4:
                {
                    this.outputGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 5:
                {
                    this.btnText = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 70 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnText).Tapped += this.btnText_Tapped;
                    #line default
                }
                break;
            case 6:
                {
                    this.stkAnnotate = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 7:
                {
                    this.btnCancel = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 92 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnCancel).Tapped += this.btnCancel_Tapped;
                    #line default
                }
                break;
            case 8:
                {
                    this.btnClear = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 93 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnClear).Tapped += this.btnClear_Tapped;
                    #line default
                }
                break;
            case 9:
                {
                    this.btnSave = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 94 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSave).Tapped += this.btnSave_Tapped;
                    #line default
                }
                break;
            case 10:
                {
                    this.btnTakeImage = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 78 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnTakeImage).Tapped += this.btnTakeImage_Tapped;
                    #line default
                }
                break;
            case 11:
                {
                    this.btnLoadImage = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 79 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnLoadImage).Tapped += this.btnLoadImage_Tapped;
                    #line default
                }
                break;
            case 12:
                {
                    this.txtAnnotation = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 13:
                {
                    this.imgCanvas = (global::Windows.UI.Xaml.Controls.Image)(target);
                    #line 60 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.Image)this.imgCanvas).SizeChanged += this.imgCanvas_SizeChanged;
                    #line default
                }
                break;
            case 14:
                {
                    this.inkCanvas = (global::Windows.UI.Xaml.Controls.InkCanvas)(target);
                    #line 61 "..\..\..\..\..\XForms\Popup\Controls\Canvas.xaml"
                    ((global::Windows.UI.Xaml.Controls.InkCanvas)this.inkCanvas).PointerExited += this.inkCanvas_PointerExited;
                    #line default
                }
                break;
            case 15:
                {
                    this.inkCanvasScaleTransform = (global::Windows.UI.Xaml.Media.ScaleTransform)(target);
                }
                break;
            case 16:
                {
                    this.calligraphyPen = (global::Windows.UI.Xaml.Controls.InkToolbarCustomPenButton)(target);
                }
                break;
            case 17:
                {
                    this.itbPen = (global::Windows.UI.Xaml.Controls.InkToolbarBallpointPenButton)(target);
                }
                break;
            case 18:
                {
                    this.itbPencil = (global::Windows.UI.Xaml.Controls.InkToolbarPencilButton)(target);
                }
                break;
            case 19:
#pragma warning disable 0618  //   Warning on Deprecated usage
                {
                    this.itbRuler = (global::Windows.UI.Xaml.Controls.InkToolbarRulerButton)(target);
#pragma warning restore 0618
                }
                break;
            case 20:
                {
                    this.itbEraser = (global::Windows.UI.Xaml.Controls.InkToolbarEraserButton)(target);
                }
                break;
            case 21:
                {
                    this.itbHighlighter = (global::Windows.UI.Xaml.Controls.InkToolbarHighlighterButton)(target);
                }
                break;
            case 22:
                {
                    this.itbStencil = (global::Windows.UI.Xaml.Controls.InkToolbarStencilButton)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1:
                {
                    global::Windows.UI.Xaml.Controls.UserControl element1 = (global::Windows.UI.Xaml.Controls.UserControl)target;
                    Canvas_obj1_Bindings bindings = new Canvas_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            }
            return returnValue;
        }
    }
}

