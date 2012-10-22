using System.Collections.Generic;
using TheShoppingList.Classes;

namespace TheShoppingList
{
    public class ProductsCategory
    {
        public string Title { get; set; }

        public List<Product> Items { get; set; }
    }
}