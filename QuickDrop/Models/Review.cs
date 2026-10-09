using SQLite;
using System;

namespace QuickDrop.Models
{
    public class Review
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerUserId { get; set; }
        public string CustomerName { get; set; }

        // Rider Rating & Review
        public int RiderRating { get; set; } // 1 to 5 stars
        public string RiderComment { get; set; }
        public string RiderName { get; set; } = "Alex Rivers";

        // Vendor & Food Quality Rating & Review
        public int FoodQualityRating { get; set; } // 1 to 5 stars
        public string FoodQualityComment { get; set; }
        public string VendorComment { get; set; }
        public string VendorName { get; set; } = "QuickDrop Fresh";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Ignore]
        public string FormattedDate => CreatedAt.ToString("dd MMM yyyy, HH:mm");

        [Ignore]
        public string RiderStarsDisplay => new string('★', RiderRating) + new string('☆', Math.Max(0, 5 - RiderRating));

        [Ignore]
        public string FoodQualityStarsDisplay => new string('★', FoodQualityRating) + new string('☆', Math.Max(0, 5 - FoodQualityRating));

        [Ignore]
        public double AverageScore => Math.Round((RiderRating + FoodQualityRating) / 2.0, 1);
    }
}
