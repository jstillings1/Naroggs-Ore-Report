﻿#pragma checksum "C:\Users\jstil\documents\visual studio 2017\Projects\Ore Report\Ore Report\Scenario1_Start.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E42E8186C6353CA44A6323D8FE42C55A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ore_Report
{
    partial class Scenario1 : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.16.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // Scenario1_Start.xaml line 17
                {
                    this.InputTextBlock1 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 2: // Scenario1_Start.xaml line 24
                {
                    this.ServiceNameForListener = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 3: // Scenario1_Start.xaml line 27
                {
                    this.InboundBufferSize = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 4: // Scenario1_Start.xaml line 28
                {
                    this.BindToAny = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.BindToAny).Checked += this.BindToAny_Checked;
                    ((global::Windows.UI.Xaml.Controls.RadioButton)this.BindToAny).Unchecked += this.BindToAny_Unchecked;
                }
                break;
            case 5: // Scenario1_Start.xaml line 29
                {
                    this.BindToAddress = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                }
                break;
            case 6: // Scenario1_Start.xaml line 30
                {
                    this.BindToAdapter = (global::Windows.UI.Xaml.Controls.RadioButton)(target);
                }
                break;
            case 7: // Scenario1_Start.xaml line 31
                {
                    this.AdapterList = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                }
                break;
            case 8: // Scenario1_Start.xaml line 32
                {
                    this.StartListener = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.StartListener).Click += this.StartListener_Click;
                }
                break;
            case 9: // Scenario1_Start.xaml line 36
                {
                    this.DefaultLayout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 10: // Scenario1_Start.xaml line 37
                {
                    this.Below768Layout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.16.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

