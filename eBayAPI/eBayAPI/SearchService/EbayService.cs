using eBayAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace eBayAPI.SearchService
{
    
    public class EbayService
    {
        private const String SellerName = "eBay";
        private static EbayService _instance = new EbayService();
        public static EbayService Instance
        {
            get
            {
                return _instance;
            }
        }

        public async Task<List<Item>> SearchWithQuery(ItemQuery query)
        {
            EbayRequest searchRequest = new EbayRequest(query);

            // Good place to kill the async-virus - need all results to continue
            List<EbayItem> apiResults = await searchRequest.performRequest();

            List<Item> searchResults = ConsolidateResults(apiResults);

            return searchResults;
        }

        private List<Item> ConsolidateResults(List<EbayItem> SearchResults)
        {
            List<Item> consolidatedResults = new List<Item>();
            foreach (EbayItem searchResult in SearchResults)
            {
                if (String.IsNullOrEmpty(searchResult.UPC))
                {
                    continue;
                }

                Item existing = null;
                existing = consolidatedResults.FirstOrDefault((item) =>
                {
                    return item.ProductCode == searchResult.UPC;
                });

                if (existing == null)
                {
                    existing = BuildItemFromEbayResult(searchResult);
                    consolidatedResults.Add(existing);
                }

                ItemPrice price = BuildItemPriceFromEbay(searchResult);

                existing.PriceData.Add(price);

            }

            return consolidatedResults;
        }

        private ItemPrice BuildItemPriceFromEbay(EbayItem Item)
        {
            ItemPrice result = new ItemPrice()
            {
                BuyLocation = Item.viewItemURL.FirstOrDefault(),
                FoundDate = DateTime.Now,
                Price = Double.Parse(Item.sellingStatus.First().currentPrice.First().__value__),
                Seller = new Seller()
                {
                    Name = SellerName
                },
                Type = "Online",
            };
            return result;
        }

        private Item BuildItemFromEbayResult(EbayItem EbayVersion)
        {
            Item result = new Item()
            {
                DisplayName = EbayVersion.title.FirstOrDefault(),
                ProductCode = EbayVersion.UPC,
                ImageUrl = EbayVersion.galleryURL.FirstOrDefault(),
            };

            result.PriceData = new List<ItemPrice>();
            return result;
        }

    }
}