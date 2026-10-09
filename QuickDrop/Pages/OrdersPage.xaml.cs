using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using QuickDrop.Models;
using QuickDrop.Services;
using System.Collections.Generic;

namespace QuickDrop.Pages
{
    public class OrderDisplayModel : Order
    {
        public bool IsActive => Status == "Active";
        public bool IsCompleted => Status == "Completed";
        public bool IsCancelled => Status == "Cancelled";

        public string OrderDateText
        {
            get
            {
                if (DateTime.TryParse(OrderDate, out DateTime dt))
                    return dt.ToString("dd MMM, HH:mm");
                return OrderDate;
            }
        }

        public List<string> DisplayImages
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProductImagesCsv))
                    return new List<string>();

                return ProductImagesCsv.Split(',')
                                       .Select(img => img.Trim())
                                       .Where(img => !string.IsNullOrEmpty(img))
                                       .Take(3)
                                       .ToList();
            }
        }

        public bool HasExtraImages => TotalItemsCount > 3;
        public string ExtraCountText => $"+{TotalItemsCount - 3}";
    }

    public partial class OrdersPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private List<OrderDisplayModel> _allOrders = new();

        public ObservableCollection<OrderDisplayModel> DisplayOrders { get; set; } = new();

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        private string _activeAddressText = "Searching Address...";
        public string ActiveAddressText { get => _activeAddressText; set { _activeAddressText = value; OnPropertyChanged(); } }

        public OrdersPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImage))
            {
                UserProfileImage.Source = App.CurrentUser.ProfileImage;
            }
            else
            {
                UserProfileImage.Source = "default_avatar.png";
            }

            await LoadHeaderDataAsync();
            await LoadOrdersAsync();
            await SyncCartAsync(); 
        }

        private async Task LoadHeaderDataAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;
            try
            {
                var addresses = await _dbService.GetAddressesAsync(App.CurrentUser.UserId);
                if (addresses != null && addresses.Count > 0)
                {
                    var activeAddress = addresses.FirstOrDefault(a => a.IsActive) ?? addresses.First();
                ActiveAddressText = $"{activeAddress.Title} • {activeAddress.CityAndDistrict.Split(',')[0].Trim()}";
                }
                else ActiveAddressText = "Add Address";
            }
            catch { ActiveAddressText = "Address Not Found"; }
        }

        // METHOD CHECKING CART
        private async Task SyncCartAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;

            var cartItems = await _dbService.GetCartItemsAsync(App.CurrentUser.UserId);

            UniqueCartItemCount = cartItems.Count;
            if (UniqueCartItemCount > 0)
            {
                decimal subtotal = cartItems.Sum(c => c.Subtotal);
                decimal currentDeliveryFee = subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;
                decimal grandTotal = subtotal + BagFee + currentDeliveryFee;

                TotalCartText = $"${grandTotal:N2}";
            }
            else
            {
                TotalCartText = "$0.00";
            }
        }

        private async Task LoadOrdersAsync()
        {
            int userId = App.CurrentUser?.UserId ?? 1;
            var dbOrders = await _dbService.GetOrdersAsync(userId);

            _allOrders.Clear();
            foreach (var o in dbOrders)
            {
                _allOrders.Add(new OrderDisplayModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    TotalItemsCount = o.TotalItemsCount,
                    OrderSummary = o.OrderSummary,
                    Status = o.Status,
                    EstimatedDeliveryTime = o.EstimatedDeliveryTime,
                    ProductImagesCsv = o.ProductImagesCsv
                });
            }

            TotalOrdersCountLabel.Text = $"Total {_allOrders.Count} orders recorded";
            FilterAllText.Text = $"All ({_allOrders.Count})";
            FilterActiveText.Text = $"Active ({_allOrders.Count(x => x.IsActive)})";
            FilterCompletedText.Text = $"Completed ({_allOrders.Count(x => x.IsCompleted || x.IsCancelled)})";

            ApplyFilter("All");
        }

        private async void OnPulseDotLoaded(object sender, EventArgs e)
        {
            if (sender is BoxView dot)
            {
                try
                {
                    while (true)
                    {
                        await dot.FadeTo(0.2, 500);
                        await dot.FadeTo(1.0, 500);
                    }
                }
                catch { }
            }
        }

        private async void OnBikeBadgeLoaded(object sender, EventArgs e)
        {
            if (sender is Border bike)
            {
                try
                {
                    while (true)
                    {
                        await bike.ScaleTo(1.15, 300);
                        await bike.ScaleTo(1.0, 300);
                        await Task.Delay(400);
                    }
                }
                catch { }
            }
        }

        private async void OnNavCategoriesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CategoryPage(_dbService));
        }

        private async void OnNavHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_dbService));
        }

        private async void OnNavCartTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(_dbService));
        }

        private void OnNavOrdersTapped(object sender, EventArgs e)
        {
        }

        private async void OnNavProfileTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_dbService));
        }

        private async void OnReviewOrderTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is OrderDisplayModel order)
            {
                await Navigation.PushAsync(new ReviewOrderPage(_dbService, order));
            }
        }

        private async void OnRefreshOrdersTapped(object sender, TappedEventArgs e)
        {
            await LoadOrdersAsync();
            await SyncCartAsync();
            await ProfessionalNotificationPopup.ShowAsync(
                Navigation,
                "Orders Refreshed",
                "Your order tracking statuses and order history have been refreshed.",
                NotificationType.Success,
                "OK");
        }

        private void OnFilterAllTapped(object sender, EventArgs e) => ApplyFilter("All");
        private void OnFilterActiveTapped(object sender, EventArgs e) => ApplyFilter("Active");
        private void OnFilterCompletedTapped(object sender, EventArgs e) => ApplyFilter("Completed");

        private void ApplyFilter(string filterType)
        {
            FilterAllBorder.BackgroundColor = Color.FromArgb("#e0e3e5");
            FilterAllText.TextColor = Color.FromArgb("#191c1e");

            FilterActiveBorder.BackgroundColor = Color.FromArgb("#e0e3e5");
            FilterActiveText.TextColor = Color.FromArgb("#191c1e");
            ActivePulseIndicator.IsVisible = false;

            FilterCompletedBorder.BackgroundColor = Color.FromArgb("#e0e3e5");
            FilterCompletedText.TextColor = Color.FromArgb("#191c1e");

            DisplayOrders.Clear();

            if (filterType == "All")
            {
                FilterAllBorder.BackgroundColor = Color.FromArgb("#f97316");
                FilterAllText.TextColor = Color.FromArgb("#ffffff");
                foreach (var o in _allOrders) DisplayOrders.Add(o);
            }
            else if (filterType == "Active")
            {
                FilterActiveBorder.BackgroundColor = Color.FromArgb("#f97316");
                FilterActiveText.TextColor = Color.FromArgb("#ffffff");
                ActivePulseIndicator.IsVisible = true;
                ActivePulseIndicator.Color = Color.FromArgb("#ffffff");

                foreach (var o in _allOrders.Where(x => x.IsActive)) DisplayOrders.Add(o);
            }
            else if (filterType == "Completed")
            {
                FilterCompletedBorder.BackgroundColor = Color.FromArgb("#f97316");
                FilterCompletedText.TextColor = Color.FromArgb("#ffffff");

                foreach (var o in _allOrders.Where(x => x.IsCompleted || x.IsCancelled)) DisplayOrders.Add(o);
            }
        }
    }
}