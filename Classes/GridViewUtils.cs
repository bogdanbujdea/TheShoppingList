using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TheShoppingList.Classes
{
    public class GridViewUtils
    {
        public void AddCategories(ref ProductsPageViewModel viewModel)
        {
            viewModel.Items.Add(new ProductsCategory { Title = "Remaining Products" });
            viewModel.Items.Add(new ProductsCategory { Title = "In Cart" });
        }

        public void FixViewModel(ref ProductsPageViewModel viewModel)
        {
            if (viewModel.Items.Count == 0)
                AddCategories(ref viewModel);

            if (viewModel.Items.Count == 1)
            {
                string title = "Remaining Products";
                if (viewModel.Items[0].Title == "Remaining Products") //if the first category is remaining products
                {
                    title = "In Cart"; //add "In Cart" as the last one
                    viewModel.Items.Add(new ProductsCategory { Title = title });
                }
                else viewModel.Items.Insert(0, new ProductsCategory { Title = title }); //insert the "Remaining Products" as the first one
            }

            if(viewModel.Items.Count == 2 && viewModel.Items[0].Title == "In Cart")
            {
                var tmp = viewModel.Items[0];
                viewModel.Items[0] = viewModel.Items[1];
                viewModel.Items[1] = tmp;
            }

        }
        
        public void CleanShoppingList(ref ShoppingList list)
        {
            for (int i = 0; i < list.Products.Count; i++)
            {
                if (list.Products[i].Title.Length == 0)
                {
                    list.Products.RemoveAt(i);
                    i--;
                }
            }
        }

        public Visibility ListIsEmpty(ProductsPageViewModel viewModel)
        {
            if (viewModel.Items == null)
                return Visibility.Visible;
            int sum1 = 1, sum2 = 1;
            if (viewModel.Items[0].Items == null || viewModel.Items[0].Items.Count == 0)
                sum1 = 0;

            if (viewModel.Items[1].Items == null || viewModel.Items[1].Items.Count == 0)
                sum2 = 0;
            if (sum1 + sum2 == 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        

    }
}
