using SQLite;
namespace QuickDrop.Models
{
    public class Favorite
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}