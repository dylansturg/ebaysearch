using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBayAPI.Models
{
    public class EbayItem
    {
        protected String title { get; set; }
        protected String globalId { get; set; }
        protected String galleryURL { get; set; }
        protected String viewItemURL { get; set; }
        protected String productIdType { get; set; }
        protected String productIdValue { get; set; }
        protected String location { get; set; }
        protected String currentPrice { get; set; }
        protected String startTime { get; set; }
        protected String endTime { get; set; }

        protected String UPC { get; set; }
        protected String stockThumbnailURL { get; set; }
    }
}