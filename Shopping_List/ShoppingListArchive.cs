using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopping_List
{
    public class ShoppingListArchive
    {
        public DateTime Date { get; set; }
        public List<Product> Products { get; set; }

        public string Summary
        {
            get
            {
                if (Products == null || Products.Count == 0)
                    return Date.ToShortDateString();

                var productNames = Products
                    .Select(p => p.Name)
                    .Take(3); // показываем первые 3 продукта

                var preview = string.Join(", ", productNames);
                if (Products.Count > 3)
                    preview += "...";

                return $"{Date:dd.MM.yyyy}: {preview}";
            }
        }
    }
}
