using SQLite;

namespace QuickDrop.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string IconCode { get; set; }

        public string Description { get; set; }

        [Ignore]
        public int ProductCount { get; set; }
    }
}