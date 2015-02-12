using eBayAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace eBayAPI.SearchService
{
    public class EbayResponse
    {
        private String Content { get; set; }
        public EbayResponse(String content)
        {
            Content = content;
        }

        public async Task<IEnumerable<EbayItem>> GetResults(String apiKey, String operation)
        {
            var parsedResponseDict = await JsonConvert.DeserializeObjectAsync<Dictionary<String, List<EbayResponseModel>>>(Content);
            var parsedResponse = parsedResponseDict[operation + "Response"];

            var resultItems = parsedResponse.First().searchResult.First().item.Where(item => item.productId != null);
            var queryTasks = new List<Task>();

            if (parsedResponse != null)
            {
                foreach (var result in resultItems)
                {
                    if (String.IsNullOrEmpty(result.UPC))
                    {
                        var details = new EbayDetailRequest();
                        queryTasks.Add(details.QueryItemDetails(result, apiKey));
                    }
                }
            }

            await Task.WhenAll(queryTasks.ToArray());
            return resultItems;
        }
    }
}