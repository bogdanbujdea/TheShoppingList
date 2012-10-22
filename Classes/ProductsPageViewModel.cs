using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace TheShoppingList.Classes
{
    public class ProductsPageViewModel
    {
        public ProductsPageViewModel(ShoppingList list)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

            if (source != null)
            {
                var productsByCategories = list.Products.GroupBy(x => x.Category)
                    .Select(x => new ProductsCategory() { Title = x.Key, Items = x.ToList() });
                Items = productsByCategories.ToList();
            }
        }

        public List<ProductsCategory> Items { get; set; }
        
    }
}
