﻿

#pragma checksum "C:\Users\Bogdan\Documents\Visual Studio 2012\Projects\TheShoppingList\TheShoppingList\NewShoppingList.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "274C7CBE9882574B9D28BEB726DDC06D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TheShoppingList
{
    partial class NewShoppingList : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 27 "..\..\NewShoppingList.xaml"
                ((global::Windows.UI.Xaml.Controls.TextBox)(target)).TextChanged += this.txtListName_TextChanged;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 30 "..\..\NewShoppingList.xaml"
                ((global::Windows.UI.Xaml.Controls.TextBox)(target)).TextChanged += this.txtListName_TextChanged;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 33 "..\..\NewShoppingList.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnSaveListName;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 34 "..\..\NewShoppingList.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnClosePopup;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


