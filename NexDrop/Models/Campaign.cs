using SQLite;
using System;

namespace QuickDrop.Models
{
    public class Campaign
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public string BadgeText { get; set; }

        public string SecondaryBadgeText { get; set; }

        public string ProductCountText { get; set; }

        public bool IsActive { get; set; }
        public DateTime EndDate { get; set; }
    }
}