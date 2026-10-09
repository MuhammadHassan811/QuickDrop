using SQLite;

namespace QuickDrop.Models
{
    public class Address
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string CityAndDistrict { get; set; }
        public string FullAddress { get; set; }
        public bool IsActive { get; set; }

        public string Building { get; set; }
        public string Floor { get; set; }
        public string Apartment { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsCurrentLocation { get; set; }

        [Ignore] public bool HasBuilding => !string.IsNullOrEmpty(Building);
        [Ignore] public bool HasFloor => !string.IsNullOrEmpty(Floor);
        [Ignore] public bool HasApartment => !string.IsNullOrEmpty(Apartment);
    }
}