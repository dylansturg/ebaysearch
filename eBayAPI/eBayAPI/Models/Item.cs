using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBayAPI.Models
{
    public class Item
    {
        protected String displayName { get; set; }
        protected String productCode { get; set; }
        protected List<ItemPrice> priceData { get; set; }
        protected String imageUrl { get; set; }
    }

    public class ItemPrice
    {
        protected Item mItem { get; set; }

        protected Seller mSeller { get; set; }

        protected DateTime mFoundDate { get; set; }
        protected String mType { get; set; }
        protected String mBuyLocation { get; set; }

        protected double mPrice { get; set; }
    }

    public class Seller
    {
        protected String mName { get; set; }
    }
}