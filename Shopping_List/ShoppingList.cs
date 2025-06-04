using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopping_List
{
    public class ShoppingList
    {
        public DateTime Date { get; set; }
        public List<Product> Products { get; set; }

        public ShoppingList()
        {
            Date = DateTime.Now;
            Products = new List<Product>();
        }
    }

}
