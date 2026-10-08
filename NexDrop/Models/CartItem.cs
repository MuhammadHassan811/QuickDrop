using SQLite;
using System;

namespace QuickDrop.Models
{
    public class CartItem
    {
        [PrimaryKey, AutoIncrement]
        public int CartItemId { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }

    public class CartItemDisplayModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Unit { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public bool HasDiscount { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal => Price * Quantity;

        public string FormattedPrice => $"${Price:N2}";
        public string FormattedOldPrice => OldPrice.HasValue ? $"${OldPrice.Value:N2}" : string.Empty;
        public string FormattedSubtotal => $"${Subtotal:N2}";
    }
}