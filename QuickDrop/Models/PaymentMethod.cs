using SQLite;

namespace QuickDrop.Models
{
    public class PaymentMethod
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public int UserId { get; set; }
        public string CardName { get; set; }
        public string MaskedNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardType { get; set; }
        public bool IsDefault { get; set; }

        [Ignore] public bool IsMastercard => CardType == "Mastercard";
        [Ignore] public bool IsVisa => CardType == "Visa";
    }
}