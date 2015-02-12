using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBayAPI.Models
{
    public class ItemQuery
    {
        public String Keywords { get; set; }
        public String ProductCode { get; set; }
        public String ProductCodeType { get; set; }

        public Boolean LimitedToLocal { get; set; }
        public double SearchRadiusMiles { get; set; }
        public double LocationLatitude { get; set; }
        public double LocationLongitude { get; set; }
    }
}