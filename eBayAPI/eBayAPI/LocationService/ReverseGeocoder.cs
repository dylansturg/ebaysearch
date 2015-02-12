using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace eBayAPI.LocationService
{
    public static class ReverseGeocoder
    {
        public const int PRECISION_DIGITS = 3;

        private static Dictionary<Location, String> CachedPostals = new Dictionary<Location, string>();

        public static async Task<String> FindPostalCode(double Latitude, double Longitude)
        {
            Location searchLoc = new Location()
            {
                Latitude = (decimal)Latitude,
                Longitude = (decimal)Longitude,
            };

            if (CachedPostals.ContainsKey(searchLoc))
            {
                return CachedPostals[searchLoc];
            }

            var response = await QueryGeocodeService(searchLoc);
            if (response == null || response.address_components == null)
            {
                return null;
            }

            String postal = null;
            foreach (var address in response.address_components)
            {
                if (address.types != null && address.types.Contains("postal_code"))
                {
                    postal = address.short_name;
                }
            }

            if (!String.IsNullOrWhiteSpace(postal))
            {
                CachedPostals.Add(searchLoc, postal);
            }

            return postal;
        }

        private static async Task<MapsResponse> QueryGeocodeService(Location search)
        {
            // https://maps.googleapis.com/maps/api/geocode/json?latlng=40.714,-73.961

            UriBuilder builder = new UriBuilder();
            builder.Host = ConfigurationManager.AppSettings["GeocoderService"];
            builder.Path = ConfigurationManager.AppSettings["GeocoderServicePath"];

            var query = HttpUtility.ParseQueryString(builder.Query);
            query["latlng"] = String.Format("{0},{1}", search.Latitude, search.Longitude);

            builder.Query = query.ToString();

            Uri serviceEndpoint = builder.Uri;

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(serviceEndpoint);
                var mapResponse = await JsonConvert.DeserializeObjectAsync<MapsResponse>(response);

                return mapResponse;
            }

            
        }
    }

    

    public class Location
    {
        private decimal _latitude;
        public decimal Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                _latitude = Math.Round(value, ReverseGeocoder.PRECISION_DIGITS);
            }
        }

        private decimal _longitude;
        public decimal Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                _longitude = Math.Round(value, ReverseGeocoder.PRECISION_DIGITS);
            }
        }

        public override bool Equals(object obj)
        {
            var otherLocation = obj as Location;
            if (otherLocation == null)
            {
                return false;
            }
            return Latitude == otherLocation.Latitude && Longitude == otherLocation.Longitude;
        }
    }

    public class MapsResponse
    {
        public List<AddressComponents> address_components;
        public String formatted_address;
    }

    public class AddressComponents
    {
        public String long_name;
        public String short_name;
        public List<String> types;
    }
}