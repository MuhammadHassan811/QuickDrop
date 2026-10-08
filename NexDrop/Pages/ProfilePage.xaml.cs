using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using QuickDrop.Services;
using QuickDrop.Models;

namespace QuickDrop.Pages
{
    public partial class ProfilePage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        private int _totalOrderCount;
        public int TotalOrderCount { get => _totalOrderCount; set { _totalOrderCount = value; OnPropertyChanged(); } }

        private decimal _totalSpentAmount;
        public decimal TotalSpentAmount { get => _totalSpentAmount; set { _totalSpentAmount = value; OnPropertyChanged(); } }

        private int _favoriteCount;
        public int FavoriteCount { get => _favoriteCount; set { _favoriteCount = value; OnPropertyChanged(); } }

        private string _activeAddressText = "Address Not Found";
        public string ActiveAddressText { get => _activeAddressText; set { _activeAddressText = value; OnPropertyChanged(); } }

        private string _addressPreviewText = "No saved address.";
        public string AddressPreviewText { get => _addressPreviewText; set { _addressPreviewText = value; OnPropertyChanged(); } }

        private string _addressCountText = "0 Saved";
        public string AddressCountText { get => _addressCountText; set { _addressCountText = value; OnPropertyChanged(); } }

        private bool _hasAddresses = false;
        public bool HasAddresses { get => _hasAddresses; set { _hasAddresses = value; OnPropertyChanged(); } }

        private string _favoritePreviewText = "No favorites added yet.";
        public string FavoritePreviewText { get => _favoritePreviewText; set { _favoritePreviewText = value; OnPropertyChanged(); } }

        private string _paymentPreviewText = "No saved card.";
        public string PaymentPreviewText { get => _paymentPreviewText; set { _paymentPreviewText = value; OnPropertyChanged(); } }

        private string _activeCouponText = "0 Active Coupons";
        public string ActiveCouponText { get => _activeCouponText; set { _activeCouponText = value; OnPropertyChanged(); } }

        private bool _hasActiveCoupons = false;
        public bool HasActiveCoupons { get => _hasActiveCoupons; set { _hasActiveCoupons = value; OnPropertyChanged(); } }


        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public ProfilePage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (App.CurrentUser == null && _dbService != null)
                App.CurrentUser = await _dbService.GetDefaultUserAsync();

            LoadUserData();
            await LoadDynamicDataFromDatabaseAsync();
        }

        private void LoadUserData()
        {
            if (App.CurrentUser != null)
            {
                UserNameLabel.Text = App.CurrentUser.FullName;
                UserPhoneLabel.Text = App.CurrentUser.Phone;
            }
        }

        private async Task LoadDynamicDataFromDatabaseAsync()
        {
            if (_dbService == null || App.CurrentUser == null) return;
            int userId = App.CurrentUser.UserId;

            try
            {
                var orders = await _dbService.GetOrdersAsync(userId);
                TotalOrderCount = orders.Count;
                TotalSpentAmount = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
            }
            catch { TotalOrderCount = 0; TotalSpentAmount = 0m; }

            // 2. KUPONLAR 
            try
            {
                var discounts = await _dbService.GetAllDiscountsAsync();
                int couponCount = discounts.Count;
                if (couponCount > 0)
                {
                    ActiveCouponText = $"{couponCount} Active Coupons Available";
                    HasActiveCoupons = true;
                }
                else
                {
                    ActiveCouponText = "No active coupons";
                    HasActiveCoupons = false;
                }
            }
            catch { ActiveCouponText = "Could not load coupons"; HasActiveCoupons = false; }

            // 3. ADRESLER 
            try
            {
                var addresses = await _dbService.GetAddressesAsync(userId);
                    AddressCountText = $"{addresses.Count} Saved";
                HasAddresses = addresses.Count > 0;

                if (addresses.Count > 0)
                {
                    var activeAddress = addresses.FirstOrDefault(a => a.IsActive) ?? addresses.First();

                        ActiveAddressText = $"{activeAddress.Title} • {activeAddress.CityAndDistrict.Split(',')[0].Trim()}";

                    AddressPreviewText = string.Join(", ", addresses.Select(a => $"{a.Title} ({a.CityAndDistrict.Split(',')[0].Trim()})"));
                }
                else
                {
                    ActiveAddressText = "Add Address";
                        AddressPreviewText = "No saved address.";
                }
            }
            catch { ActiveAddressText = "Error"; AddressPreviewText = "Addresses could not be loaded."; }

            // 4. FAVORITES
            try
            {
                var favorites = await _dbService.GetFavoritesAsync(userId);
                FavoriteCount = favorites.Count;

                if (favorites.Count > 0)
                {
                    var allProducts = await _dbService.GetAllProductsAsync();
                    var favProductNames = allProducts
                                          .Where(p => favorites.Any(f => f.ProductId == p.ProductId))
                                          .Select(p => p.Name)
                                          .ToList();

                    FavoritePreviewText = string.Join(", ", favProductNames);
                    if (favProductNames.Count > 2) FavoritePreviewText += "...";
                }
                else
                {
                    FavoritePreviewText = "No favorites added yet.";
                }
            }
            catch { FavoriteCount = 0; FavoritePreviewText = "Favorites could not be loaded."; }

            // 5. PAYMENT METHODS
            try
            {
                var payments = await _dbService.GetPaymentMethodsAsync(userId);
                if (payments.Count > 0)
                {
                    PaymentPreviewText = string.Join(", ", payments.Select(p => $"{p.CardName} {p.MaskedNumber}"));
                }
                else
                {
                    PaymentPreviewText = "No saved card.";
                }
            }
            catch { PaymentPreviewText = "Payment info could not be loaded."; }
        }

        private async void OnLogoutTapped(object sender, EventArgs e)
        {
            bool answer = await ProfessionalNotificationPopup.ShowConfirmAsync(
                Navigation,
                "Log Out of QuickDrop",
                "Are you sure you want to end your active session? You can sign back in anytime.",
                NotificationType.Logout,
                "Yes, Log Out",
                "Stay Logged In");

            if (answer)
            {
                App.CurrentUser = null;
                App.CurrentActiveAddress = null;
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void OnOrdersCardTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage(_dbService));
        }

        private async void OnFavoritesCardTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FavoritesPage(_dbService));
        }

        private async void OnFavoritesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FavoritesPage(_dbService));
        }

        private async void OnAddressesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddressesPage(_dbService));
        }

        private async void OnPaymentsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PaymentMethodsPage(_dbService));
        }

        private async void OnCouponsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CouponsPage(_dbService));
        }
        private async void OnStatisticsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StatisticsPage(_dbService));
        }
        private async void OnDiscountTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DiscountsPage(_dbService));
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
        private async void OnNavOrdersTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage(_dbService));
        }

        private async void OnOpenAdminPanelTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdminPanelPage(_dbService));
        }

        private async void OnOpenRiderPanelTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RiderPage(_dbService));
        }
    }
}