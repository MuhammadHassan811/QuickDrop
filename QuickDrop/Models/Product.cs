using SQLite;
using System.Linq;
using System.Collections.Generic;

namespace QuickDrop.Models
{
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int ProductId { get; set; }

        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public string Unit { get; set; }
        public string NutritionEnergy { get; set; }
        public string NutritionFat { get; set; }
        public string NutritionProtein { get; set; }
        public string NutritionCalcium { get; set; }
        public string NutritionCarbs { get; set; }
        public string NutritionSalt { get; set; }
        public string StorageConditions { get; set; }
        public string ImageUrl { get; set; }

        [Indexed]
        public int CategoryId { get; set; }

        public bool IsFavorite { get; set; }
        public int Stock { get; set; }


        [Ignore]
        public bool HasDiscount => DiscountPercentage.HasValue && DiscountPercentage.Value > 0;

        [Ignore]
        public bool HasOldPrice => OldPrice.HasValue && OldPrice.Value > 0;

        [Ignore]
        public string FormattedPrice => $"${Price:N2}";

        [Ignore]
        public string FormattedOldPrice => OldPrice.HasValue ? $"${OldPrice.Value:N2}" : string.Empty;

        [Ignore]
        public string FormattedDiscount => DiscountPercentage.HasValue ? $"%{DiscountPercentage.Value}" : string.Empty;

        [Ignore]
        public string FavoriteIcon => IsFavorite ? "❤️" : "🤍";
        [Ignore]
        public string MainImage => ImageList.FirstOrDefault();

        [Ignore]
        public List<string> ImageList
        {
            get
            {
                if (string.IsNullOrEmpty(ImageUrl))
                    return new List<string> { "product_placeholder.png" };

                return ImageUrl.Split(',').Select(x => x.Trim()).ToList();
            }
        }
    }
}