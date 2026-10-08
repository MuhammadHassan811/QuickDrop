namespace QuickDrop.Models
{
    public class FavoriteProduct
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string BadgeText { get; set; }
        public string BadgeColor { get; set; }
        public string BadgeTextColor { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool HasOldPrice => OldPrice.HasValue && OldPrice > 0;
        public bool HasBadge => !string.IsNullOrEmpty(BadgeText);
    }
}