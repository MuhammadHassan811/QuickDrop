using SQLite;

namespace QuickDrop.Models
{
    public class CategoryFilter
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}