using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using QuickDrop.Models;

namespace QuickDrop.Services
{
    public class UserLocationInfo
    {
        public string City { get; set; } = "New York";
        public string District { get; set; } = "Downtown";
        public string Country { get; set; } = "United States";
        public string FullAddress { get; set; } = "Current Location, Downtown";
        public string Title { get; set; } = "Current Location";
        public double Latitude { get; set; } = 40.7128;
        public double Longitude { get; set; } = -74.0060;
        public string Source { get; set; } = "Dynamic Location";
    }

    internal class IpApiResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; }
        [JsonPropertyName("country")] public string Country { get; set; }
        [JsonPropertyName("regionName")] public string RegionName { get; set; }
        [JsonPropertyName("city")] public string City { get; set; }
        [JsonPropertyName("lat")] public double Lat { get; set; }
        [JsonPropertyName("lon")] public double Lon { get; set; }
    }

    public class LocationService
    {
        private static LocationService _instance;
        public static LocationService Instance => _instance ??= new LocationService();

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(4)
        };

        /// <summary>
        /// Dynamically retrieves the user's location via GPS or network IP fallback.
        /// </summary>
        public async Task<UserLocationInfo> GetCurrentLocationAsync()
        {
            // 1. Try Hardware / Platform Geolocation (GPS)
            try
            {
                var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (permission != PermissionStatus.Granted)
                {
                    permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (permission == PermissionStatus.Granted)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(4));
                    var location = await Geolocation.Default.GetLocationAsync(request);
                    if (location == null)
                    {
                        location = await Geolocation.Default.GetLastKnownLocationAsync();
                    }

                    if (location != null)
                    {
                        var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
                        var placemark = placemarks?.FirstOrDefault();
                        if (placemark != null)
                        {
                            string city = !string.IsNullOrWhiteSpace(placemark.Locality)
                                ? placemark.Locality
                                : (!string.IsNullOrWhiteSpace(placemark.AdminArea) ? placemark.AdminArea : "Central Area");

                            string district = !string.IsNullOrWhiteSpace(placemark.SubLocality)
                                ? placemark.SubLocality
                                : (!string.IsNullOrWhiteSpace(placemark.SubAdminArea) ? placemark.SubAdminArea : "Downtown");

                            string street = $"{placemark.SubThoroughfare} {placemark.Thoroughfare}".Trim();
                            if (string.IsNullOrWhiteSpace(street))
                            {
                                street = placemark.FeatureName ?? "Current Location";
                            }

                            string country = placemark.CountryName ?? "United States";
                            string fullAddress = $"{street}, {district}, {city}, {country}".Trim(',', ' ');

                            return new UserLocationInfo
                            {
                                City = city,
                                District = district,
                                Country = country,
                                FullAddress = fullAddress,
                                Title = "Current Location",
                                Latitude = location.Latitude,
                                Longitude = location.Longitude,
                                Source = "GPS"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocationService] GPS Geolocation bypassed: {ex.Message}");
            }

            // 2. Try High-Accuracy Dynamic IP Geolocation Fallback
            try
            {
                var json = await _httpClient.GetStringAsync("http://ip-api.com/json");
                var ipData = JsonSerializer.Deserialize<IpApiResponse>(json);

                if (ipData != null && string.Equals(ipData.Status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    string city = string.IsNullOrWhiteSpace(ipData.City) ? "Metropolitan" : ipData.City;
                    string district = string.IsNullOrWhiteSpace(ipData.RegionName) ? "Central Hub" : ipData.RegionName;
                    string country = string.IsNullOrWhiteSpace(ipData.Country) ? "Global" : ipData.Country;

                    return new UserLocationInfo
                    {
                        City = city,
                        District = district,
                        Country = country,
                        FullAddress = $"{city} Delivery Hub, {district}, {country}",
                        Title = "Current Location",
                        Latitude = ipData.Lat,
                        Longitude = ipData.Lon,
                        Source = "Network IP"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocationService] IP Geolocation bypassed: {ex.Message}");
            }

            // 3. Graceful fallback
            return new UserLocationInfo
            {
                City = "New York",
                District = "Downtown",
                Country = "United States",
                FullAddress = "742 Evergreen Terrace, Downtown, New York",
                Title = "Current Location",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Source = "System Default"
            };
        }

        /// <summary>
        /// Automatically resolves the user's current location upon login,
        /// registers/updates it in SQLite, and sets it as the active default delivery address.
        /// </summary>
        public async Task<Address> ResolveAndSetActiveLocationForUserAsync(DatabaseService dbService, int userId)
        {
            var locationInfo = await GetCurrentLocationAsync();
            var addresses = await dbService.GetAddressesAsync(userId);

            Address currentLocAddress = addresses.FirstOrDefault(a => a.IsCurrentLocation || a.Title == "Current Location");

            if (currentLocAddress == null)
            {
                currentLocAddress = new Address
                {
                    UserId = userId,
                    Title = "Current Location",
                    CityAndDistrict = $"{locationInfo.District}, {locationInfo.City}",
                    FullAddress = locationInfo.FullAddress,
                    Building = "Express Hub",
                    Floor = "1",
                    Apartment = "1",
                    Latitude = locationInfo.Latitude,
                    Longitude = locationInfo.Longitude,
                    IsCurrentLocation = true,
                    IsActive = true
                };

                await dbService.AddAddressAsync(currentLocAddress);
            }
            else
            {
                currentLocAddress.CityAndDistrict = $"{locationInfo.District}, {locationInfo.City}";
                currentLocAddress.FullAddress = locationInfo.FullAddress;
                currentLocAddress.Latitude = locationInfo.Latitude;
                currentLocAddress.Longitude = locationInfo.Longitude;
                currentLocAddress.IsCurrentLocation = true;
                currentLocAddress.IsActive = true;

                await dbService.UpdateAddressAsync(currentLocAddress);
            }

            // Deactivate all other addresses for this user so current location is exclusively active
            foreach (var addr in addresses.Where(a => a.Id != currentLocAddress.Id && a.IsActive))
            {
                addr.IsActive = false;
                await dbService.UpdateAddressAsync(addr);
            }

            // Update user profile login location
            if (App.CurrentUser != null && App.CurrentUser.UserId == userId)
            {
                App.CurrentUser.LastLoginCity = locationInfo.City;
                App.CurrentUser.LastLoginAddress = locationInfo.FullAddress;
                App.CurrentUser.LastLoginAt = DateTime.Now;
                await dbService.UpdateUserAsync(App.CurrentUser);
            }

            App.CurrentActiveAddress = currentLocAddress;
            return currentLocAddress;
        }
    }
}

