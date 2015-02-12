using eBayAPI.LocationService;
using eBayAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace eBayAPI.SearchService
{
    public class EbayRequest
    {
        enum SearchType
        {
            Keyword, Product
        }

        private static Dictionary<SearchType, String> API_OPERATIONS = new Dictionary<SearchType, string>(){
            {SearchType.Keyword, "findItemsByKeywords"},
            {SearchType.Product, "findItemsByProduct"},
        };

        private const int PAGE_SIZE = 100;
        private const int MAX_PAGE_REQUESTS = 5;

        // In the privateSettings.config file
        private static String APIKey = ConfigurationManager.AppSettings["EbayAPIKey"];

        private ItemQuery _searchParams;
        private SearchType Type { get; set; }
        private int RequestCount = 1;

        public EbayRequest(ItemQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("Cannot perform EbayRequest on null ItemQuery");
            }

            _searchParams = query;
        }

        public async Task<List<EbayItem>> performRequest()
        {
            List<EbayItem> results = new List<EbayItem>();
            while (RequestCount < MAX_PAGE_REQUESTS)
            {
                try
                {
                    var endpoint = BuildRequestUri();
                    var content = await createRequestBody();

                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsJsonAsync(endpoint, content);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        var ebayParseResponse = new EbayResponse(responseContent);
                        var ebayResults = await ebayParseResponse.GetResults(APIKey, getOperation());

                        if (ebayResults != null)
                        {
                            results.AddRange(ebayResults);
                        }
                    }
                }
                catch (Exception e)
                {
                    // WTF
                }

                RequestCount++;
            }

            return results;
        }

        private String getOperation()
        {
            String code = _searchParams.ProductCode;
            if (String.IsNullOrWhiteSpace(code))
            {
                Type = SearchType.Keyword;
            }
            else
            {
                Type = SearchType.Product;
            }

            return API_OPERATIONS[Type];
        }

        private Uri BuildRequestUri()
        {

            var uriBase = new StringBuilder();
            uriBase.Append(ConfigurationManager.AppSettings["EbayService"]);
            var servicePaths = new String[]{
                "services", "search", "FindingService", "v1"
            };


            foreach (var path in servicePaths)
            {
                uriBase.Append("/" + path);
            }

            UriBuilder builder = new UriBuilder(uriBase.ToString());

            var queryParams = HttpUtility.ParseQueryString(builder.Query);
            queryParams["OPERATION-NAME"] = getOperation();
            queryParams["SECURITY-APPNAME"] = APIKey;
            queryParams["REQUEST-DATA-FORMAT"] = "JSON";
            queryParams["RESPONSE-DATA-FORMAT"] = "JSON";

            queryParams["REST-PAYLOAD"] = "";

            builder.Query = queryParams.ToString();

            return builder.Uri;
        }


        private async Task<Dictionary<String, Object>> createRequestBody()
        {
            Dictionary<String, Object> body = new Dictionary<String, Object>();
            body.Add("jsonns.xsi", "http://www.w3.org/2001/XMLSchema-instance");
            body.Add("jsonns.xs", "http://www.w3.org/2001/XMLSchema");
            body.Add("jsonns.tns",
                    "http://www.ebay.com/marketplace/search/v1/services");

            Dictionary<String, Object> encodedRequest = new Dictionary<String, Object>();
            body.Add("tns." + getOperation() + "Request", encodedRequest);

            appendSearchCriteria(encodedRequest);
            appendPaginationCriteria(encodedRequest);
            await appendLocalSearchParameters(encodedRequest);

            return body;
        }

        private void appendPaginationCriteria(Dictionary<String, Object> request)
        {
            Dictionary<String, Object> paginationObj = new Dictionary<String, Object>();
            paginationObj.Add("pageNumber", RequestCount.ToString());
            paginationObj.Add("entriesPerPage", PAGE_SIZE.ToString());

            request.Add("paginationInput", paginationObj);
        }

        private void appendSearchCriteria(Dictionary<String, Object> request)
        {

            switch (Type)
            {
                case SearchType.Keyword:
                    appendKeywordSearchCriteria(request);
                    break;
                case SearchType.Product:
                    appendProductSearchCriteria(request);
                    break;
            }

            request.Add("sortOrder", "BestMatch");

            var itemFilter = new List<Object>();
            Dictionary<String, Object> nonAuctionFilter = new Dictionary<String, Object>();
            nonAuctionFilter.Add("name", "ListingType");
            nonAuctionFilter.Add("value", "FixedPrice");
            itemFilter.Add(nonAuctionFilter);

            request.Add("itemFilter", itemFilter);
        }

        private void appendKeywordSearchCriteria(Dictionary<String, Object> request)
        {
            request.Add("keywords", _searchParams.Keywords);
        }

        private void appendProductSearchCriteria(Dictionary<String, Object> request)
        {
            String codeType = "";
            if (!String.IsNullOrWhiteSpace(_searchParams.ProductCodeType))
            {
                codeType = _searchParams.ProductCodeType;
            }
            else
            {
                codeType = estimateProductCodeType(_searchParams.ProductCode);
            }

            Dictionary<String, Object> productFilter = new Dictionary<String, Object>();
            productFilter.Add("@type", codeType);
            productFilter.Add("__value__", _searchParams.ProductCode);

            request.Add("productId", productFilter);
        }

        private String estimateProductCodeType(String productCode)
        {
            if (String.IsNullOrWhiteSpace(productCode))
            {
                throw new ArgumentNullException(
                        "EbayReqeust attemping to guess a product code type for a null/empty product code");
            }

            int length = productCode.Length;
            switch (length)
            {
                case 12:
                    return "UPC";
                case 13:
                    return "EAN";
            }

            // TODO revert to a keyword search
            return "";
        }

        private async Task appendLocalSearchParameters(Dictionary<String, Object> request)
        {
            if (_searchParams == null || !_searchParams.LimitedToLocal)
            {
                return;
            }

            var searchPostal = await ReverseGeocoder.FindPostalCode(_searchParams.LocationLatitude, _searchParams.LocationLongitude);
            var searchDistance = _searchParams.SearchRadiusMiles;

            if (String.IsNullOrEmpty(searchPostal))
            {
                return; // can't do anything
            }

            Dictionary<String, Object> localSearchFilter = new Dictionary<String, Object>();
            localSearchFilter.Add("name", "MaxDistance");
            localSearchFilter.Add("value",
                    searchDistance.ToString());

            request.Add("buyerPostalCode", searchPostal);
            List<Object> itemFilters = request.ContainsKey("itemFilter") ? request["itemFilter"] as List<Object> : new List<Object>();
            itemFilters.Add(localSearchFilter);
            request.Add("itemFilter", itemFilters);
        }
    }
}