﻿

#pragma checksum "C:\Users\Bogdan\Documents\Visual Studio 2012\Projects\TheShoppingList\TheShoppingList\GroupedProducts.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "95A2ED564D2F8A48C0B82A551F2BFAA0"
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
    partial class GroupedProducts : global::TheShoppingList.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 29 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.AppBar)(target)).Closed += this.AppBarClosed;
                 #line default
                 #line hidden
                #line 29 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.AppBar)(target)).Opened += this.AppBarOpened;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 43 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.AddProduct;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 44 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.RemoveProduct;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 32 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.EditProduct;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 33 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.RemoveProduct;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 34 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnAddRemoveFromCart;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 65 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Popup)(target)).Closed += this.PopupClosed;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 256 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.GoBack;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 219 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).RightTapped += this.ItemRightTapped;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 87 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.SemanticZoom)(target)).ViewChangeStarted += this.Zoom_ViewChangeStarted;
                 #line default
                 #line hidden
                #line 87 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.SemanticZoom)(target)).ViewChangeCompleted += this.Zoom_ViewChangeCompleted;
                 #line default
                 #line hidden
                break;
            case 11:
                #line 201 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.AddProduct;
                 #line default
                 #line hidden
                break;
            case 12:
                #line 183 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.OnClickDiscard;
                 #line default
                 #line hidden
                #line 183 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Drop += this.ItemDropDelete;
                 #line default
                 #line hidden
                break;
            case 13:
                #line 186 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.TextBox)(target)).TextChanged += this.OnTextBalanceTextChanged;
                 #line default
                 #line hidden
                #line 186 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).DoubleTapped += this.OnBalanceTextDoubleTapped;
                 #line default
                 #line hidden
                #line 186 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).LostFocus += this.OnTxtBalanceLostFocus;
                 #line default
                 #line hidden
                break;
            case 14:
                #line 96 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.OnSelectionChanged;
                 #line default
                 #line hidden
                #line 97 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).RightTapped += this.OnRightTappedGridView;
                 #line default
                 #line hidden
                #line 98 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.Controls.ListViewBase)(target)).DragItemsStarting += this.ItemDragStarting;
                 #line default
                 #line hidden
                break;
            case 15:
                #line 102 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).RightTapped += this.ItemRightTapped;
                 #line default
                 #line hidden
                break;
            case 16:
                #line 115 "..\..\GroupedProducts.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Drop += this.ItemsPanelDrop;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


