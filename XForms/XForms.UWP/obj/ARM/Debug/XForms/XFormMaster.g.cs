﻿#pragma checksum "C:\Users\cj.GOGET\Documents\Projects\XForms\XForms\XForms.UWP\XForms\XFormMaster.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BDE17D5856A085342CD81728D31C44AF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XForms.UWP.XForms
{
    partial class XFormMaster : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    #line 9 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                    #line default
                }
                break;
            case 2:
                {
                    this.btnBack = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 29 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnBack).Tapped += this.btnBack_Tapped;
                    #line default
                }
                break;
            case 3:
                {
                    this.lblFormTitle = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 4:
                {
                    this.masterScrollviewer = (global::Windows.UI.Xaml.Controls.ScrollViewer)(target);
                    #line 31 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.ScrollViewer)this.masterScrollviewer).GettingFocus += this.ScrollViewer_GettingFocus;
                    #line default
                }
                break;
            case 5:
                {
                    this.ppup = (global::Windows.UI.Xaml.Controls.Primitives.Popup)(target);
                    #line 41 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Primitives.Popup)this.ppup).Closed += this.ppup_Closed;
                    #line default
                }
                break;
            case 6:
                {
                    this.pbrProgress = (global::Windows.UI.Xaml.Controls.ProgressRing)(target);
                }
                break;
            case 7:
                {
                    this.popupContent = (global::Windows.UI.Xaml.Controls.Frame)(target);
                }
                break;
            case 8:
                {
                    this.pnlFormMain = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 9:
                {
                    this.btnFireEvent = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 22 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnFireEvent).Tapped += this.btnFireEvent_Tapped;
                    #line default
                }
                break;
            case 10:
                {
                    this.btnSaveInstance = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 23 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSaveInstance).Tapped += this.btnSaveInstance_Tapped;
                    #line default
                }
                break;
            case 11:
                {
                    this.btnSaveDBInstance = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 24 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnSaveDBInstance).Tapped += this.btnSaveDBInstance_Tapped;
                    #line default
                }
                break;
            case 12:
                {
                    this.btnBackToFormSelection = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 25 "..\..\..\XForms\XFormMaster.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.btnBackToFormSelection).Tapped += this.btnBackToFormSelection_Tapped;
                    #line default
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
            return returnValue;
        }
    }
}

