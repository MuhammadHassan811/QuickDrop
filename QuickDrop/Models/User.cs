using SQLite;
using System;

namespace QuickDrop.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100), Unique]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public string Password { get; set; }

        public string ProfileImage { get; set; } = "default_avatar.png";

        public string Role { get; set; } = "Customer";

        public string LastLoginCity { get; set; }
        public string LastLoginAddress { get; set; }
        public DateTime? LastLoginAt { get; set; }

        [Ignore]
        public bool IsAdmin => Role == "Admin" || Role == "Vendor" || Role == "SuperUser";

        [Ignore]
        public bool IsRider => Role == "Rider";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}