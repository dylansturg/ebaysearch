using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBayAPI.Models
{
    public class Item
    {
        public String DisplayName { get; set; }
        public String ProductCode { get; set; }
        public List<ItemPrice> PriceData { get; set; }
        public String ImageUrl { get; set; }
    }

    public class ItemPrice
    {
        public Seller Seller { get; set; }

        public DateTime FoundDate { get; set; }
        public String Type { get; set; }
        public String BuyLocation { get; set; }

        public double Price { get; set; }
    }

    public class Seller
    {
        public String Name { get; set; }
    }
}