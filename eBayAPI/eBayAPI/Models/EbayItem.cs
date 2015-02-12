using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBayAPI.Models
{
    public class EbayItem
    {
        public List<String> title { get; set; }
        public List<String> globalId { get; set; }
        public List<String> galleryURL { get; set; }
        public List<String> viewItemURL { get; set; }
        public List<ProductId> productId { get; set; }
        public List<SellingStatus> sellingStatus { get; set; }

        public String UPC { get; set; }
        public String stockThumbnailURL { get; set; }
    }

    public class ProductId
    {
        public String __value__ { get; set; }
    }

    public class SellingStatus
    {
        public List<Price> currentPrice { get; set; }
    }

    public class Price
    {
        public String __value__ { get; set; }
    }
        

}