﻿#pragma checksum "C:\Users\cj.GOGET\Documents\Projects\XForms\XForms\XForms.UWP\XForms\Widgets\Select.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FF55709F76D6FBFD151B100E1426F296"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XForms.UWP.XForms.Widgets
{
    partial class Select : 
        global::Windows.UI.Xaml.Controls.UserControl, 
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
                    this.cbxSelect = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    #line 27 "..\..\..\..\XForms\Widgets\Select.xaml"
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.cbxSelect).SelectionChanged += this.cbxSelect_SelectionChanged;
                    #line default
                }
                break;
            case 2:
                {
                    this.stkpnlItems = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 3:
                {
                    this.brdrValuePlaceholder = (global::Windows.UI.Xaml.Controls.Border)(target);
                }
                break;
            case 4:
                {
                    this.lblValuePlaceholder = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                    #line 39 "..\..\..\..\XForms\Widgets\Select.xaml"
                    ((global::Windows.UI.Xaml.Controls.TextBlock)this.lblValuePlaceholder).Tapped += this.lblValuePlaceholder_Tapped;
                    #line default
                }
                break;
            case 5:
                {
                    global::Windows.UI.Xaml.Controls.CheckBox element5 = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                    #line 30 "..\..\..\..\XForms\Widgets\Select.xaml"
                    ((global::Windows.UI.Xaml.Controls.CheckBox)element5).Checked += this.CheckBox_Checked;
                    #line 30 "..\..\..\..\XForms\Widgets\Select.xaml"
                    ((global::Windows.UI.Xaml.Controls.CheckBox)element5).Unchecked += this.CheckBox_Unchecked;
                    #line default
                }
                break;
            case 6:
                {
                    this.lblLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7:
                {
                    this.ellHint = (global::Windows.UI.Xaml.Shapes.Ellipse)(target);
                    #line 21 "..\..\..\..\XForms\Widgets\Select.xaml"
                    ((global::Windows.UI.Xaml.Shapes.Ellipse)this.ellHint).Tapped += this.ellHint_Tapped;
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

