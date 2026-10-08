using System;
using SQLite;

namespace QuickDrop.Models
{
    public class Discount
    {
        [PrimaryKey, AutoIncrement] 
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public string BackgroundImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpirationDate { get; set; }

        [Ignore]
        public string DisplayAmountText
        {
            get
            {
                if (!string.IsNullOrEmpty(Title) && Title.Contains("%"))
                {
                    var parts = Title.Split(' ');
                return $"Provides a net {parts[0]} discount on cart.";
                }

                return DiscountAmount > 0
                ? $"Provides a ${DiscountAmount:G2} discount on cart."
                : "Automatically applied at checkout.";
            }
        }

        [Ignore]
        public string TimeLeftText
        {
            get
            {
                var timeLeft = ExpirationDate - DateTime.Now;

                if (timeLeft.TotalDays >= 1)
                return $"{Math.Floor(timeLeft.TotalDays)} Days Left";

                if (timeLeft.TotalHours >= 1)
                return $"{Math.Floor(timeLeft.TotalHours)} Hours Left";

                if (timeLeft.TotalMinutes > 0)
                    return $"{Math.Floor(timeLeft.TotalMinutes)} Mins Left";

                return "Expired";
            }
        }
    }
}