using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBayAPI.Models
{
    public class EbayResponseModel
    {
        public List<SearchResult> searchResult;
    }

    public class SearchResult {
        public List<EbayItem> item;
    }
}