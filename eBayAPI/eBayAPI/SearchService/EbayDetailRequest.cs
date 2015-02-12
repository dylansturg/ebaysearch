using eBayAPI.Models;
using Newtonsoft.Json;
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
    public class EbayDetailRequest
    {
        private EbayItem _search;
        private String _key;

        public EbayDetailRequest()
        {
        }

        public async Task<EbayItem> QueryItemDetails(EbayItem searchItem, String APIKey)
        {
            if (searchItem == null)
            {
                throw new ArgumentNullException("EbayDetailsRequest requires a valid EbayItem to search with");
            }
            _search = searchItem;
            _key = APIKey;

            await PopulateItemDetails();
            return _search;
        }

        private async Task PopulateItemDetails()
        {
            var serviceEndpoint = BuildRequestUri();
            var requestBody = await CreateRequestContent();
            var reqcon = JsonConvert.SerializeObject(requestBody);

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsJsonAsync(serviceEndpoint, requestBody);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!String.IsNullOrWhiteSpace(responseContent))
                    {
                        await HandleResponse(responseContent);
                    }
                }
            }
            catch (Exception e)
            {

            }


        }

        private async Task HandleResponse(String content)
        {
            var response = await JsonConvert.DeserializeObjectAsync<Dictionary<String, List<ProductDetailsModel>>>(content);
            if (content.Contains("UPC"))
            {
                var hasupc = true;
            }
            try
            {
                var productDetails = response["getProductDetailsResponse"];
                // Holy shit eBay, screw you too
                var upc = productDetails.First().product.First().productDetails.First().value.First().text.First().value.First();
                _search.UPC = upc;

                var thumbnail = productDetails.First().product.First().stockPhotoURL.First().thumbnail.First().value.First();
                _search.stockThumbnailURL = thumbnail;
            }
            catch (Exception e)
            {

            }

            /*
            Object responseDetails;
            if (response.TryGetValue("getProductDetailsResponse", out responseDetails))
            {
                var productDetails = responseDetails as List<Dictionary<String, Object>>;
                Object responseProduct;
                if (productDetails[0].TryGetValue("product", out responseProduct))
                {
                    var productList = productDetails as List<Dictionary<String, Object>>;
                    var product = productList[0];

                    if (product.ContainsKey("stockPhotoURL"))
                    {
                        var photoDetails = product["stockPhotoURL"] as List<Dictionary<String, Object>>;
                        if (photoDetails[0].ContainsKey("thumbnail"))
                        {
                            var thumbnailDetails = photoDetails[0]["thumbnail"] as List<Dictionary<String, Object>>;
                            if (thumbnailDetails[0].ContainsKey("value"))
                            {
                                var thumbnailUrls = thumbnailDetails[0]["value"] as List<String>;
                                if (thumbnailUrls != null && thumbnailUrls.Count > 0)
                                {
                                    _search.stockThumbnailURL = thumbnailUrls[0];
                                }
                            }
                        }
                    }


                    if (product.ContainsKey("productDetails"))
                    {
                        var productDetailsContent = (product["productDetails"] as List<Dictionary<String, Object>>)[0];
                        if (productDetailsContent.ContainsKey("value"))
                        {
                            var productCodeValues = (productDetailsContent["value"] as List<Dictionary<String, Object>>)[0];
                            var productCodeValuesContents = (productCodeValues["text"] as List<Dictionary<String, Object>>)[0];
                            var productCodes = productCodeValuesContents["value"] as List<String>;
                            if (productCodes != null && productCodes.Count > 0)
                            {
                                _search.UPC = productCodes[0];
                            }

                        }
                    }
                }
            }
             * */
        }

        private async Task<Dictionary<String, Object>> CreateRequestContent()
        {
            var productDetailsRequest = new Dictionary<String, Object>();
            productDetailsRequest["productIdentifier"] = new Dictionary<String, String>(){
                {"ePID", _search.productId.First().__value__},
            };


            var datasetProperties = new String[]{
                "UPC", "ISBN", "EAN",
            };

            productDetailsRequest["datasetPropertyName"] = datasetProperties;

            var request = new Dictionary<String, Object>();
            request["getProductDetailsRequest"] = new Dictionary<String, Object>(){
                {"productDetailsRequest", productDetailsRequest},
            };

            return request;
        }

        private Uri BuildRequestUri()
        {
            var baseEndpoint = new StringBuilder();
            baseEndpoint.Append(ConfigurationManager.AppSettings["EbayService"]);

            String[] servicePaths = new String[]{
                "services",
                "marketplacecatalog",
                "ProductService",
                "v1",
            };

            foreach (String path in servicePaths)
            {
                baseEndpoint.Append("/" + path);
            }

            UriBuilder builder = new UriBuilder(baseEndpoint.ToString());

            var queryString = HttpUtility.ParseQueryString(builder.Query);
            queryString["OPERATION-NAME"] = "getProductDetails";
            queryString["SECURITY-APPNAME"] = _key;
            queryString["REQUEST-DATA-FORMAT"] = "JSON";
            queryString["RESPONSE-DATA-FORMAT"] = "JSON";

            builder.Query = queryString.ToString();

            return builder.Uri;
        }
    }

    public class ProductDetailsModel
    {
        public List<Product> product { get; set; }
    }

    public class Product
    {
        public List<ProductDetails> productDetails { get; set; }
        public List<StockPhotos> stockPhotoURL { get; set; }
    }

    public class ProductDetails
    {
        public List<String> propertyName { get; set; }
        public List<ProductPropertyName> value { get; set; }
    }

    public class ProductPropertyName
    {
        public List<ProductPropertyValue> text { get; set; }
    }

    public class ProductPropertyValue
    {
        public List<String> value;
    }

    public class StockPhotos
    {
        public List<ImageContainer> standard { get; set; }
        public List<ImageContainer> thumbnail { get; set; }
    }

    public class ImageContainer
    {
        public List<String> value { get; set; }
    }
}