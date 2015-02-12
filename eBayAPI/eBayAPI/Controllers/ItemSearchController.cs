using eBayAPI.Models;
using eBayAPI.SearchService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace eBayAPI.Controllers
{
    public class ItemSearchController : ApiController
    {

        
        public async Task<List<Item>> Post([FromBody] ItemQuery query)
        {
            var searchResults = new List<Item>();
            
            if (query != null)
            {
                var searchSource = new EbayService();
                var queryResult = await searchSource.SearchWithQuery(query);
                if (queryResult != null)
                {
                    searchResults.AddRange(queryResult);
                }
            }

            return searchResults;
        }
    }
}
