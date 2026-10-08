using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace QuickDrop.Models
{
    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string OrderNumber { get; set; }
        public int UserId { get; set; }

        public string OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItemsCount { get; set; }
        public string OrderSummary { get; set; }
        public string Status { get; set; }
        public string EstimatedDeliveryTime { get; set; }
        public string ProductImagesCsv { get; set; }

        [Ignore]
        public List<string> DisplayImages
        {
            get
            {
                if (string.IsNullOrEmpty(ProductImagesCsv)) return new List<string>();
                return ProductImagesCsv.Split(',').Take(4).ToList();
            }
        }

        [Ignore]
        public bool HasExtraImages => !string.IsNullOrEmpty(ProductImagesCsv) && ProductImagesCsv.Split(',').Length > 4;

        [Ignore]
        public string ExtraCountText
        {
            get
            {
                if (string.IsNullOrEmpty(ProductImagesCsv)) return "";
                int extra = ProductImagesCsv.Split(',').Length - 4;
                return extra > 0 ? $"+{extra}" : "";
            }
        }
    }
}