using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using QuickDrop.Models;
using QuickDrop.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace QuickDrop.Pages
{
    public class FlashDealModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int DiscountPercent { get; set; }
        public int StockSoldPercent { get; set; }
        public double StockSoldWidth => 136 * (StockSoldPercent / 100.0);
    }

    public class CampaignDisplayModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string BadgeText { get; set; }
        public string SecondaryBadgeText { get; set; }
        public string ProductCountText { get; set; }
        public bool HasSecondaryBadge => !string.IsNullOrEmpty(SecondaryBadgeText);
    }

    public class DiscountDisplayModel
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconCode { get; set; }

        public Color IconBgColor { get; set; }
        public Color IconTextColor { get; set; }
    }

    public class BogoProductModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
    }

    public partial class DiscountsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private IDispatcherTimer _flashTimer;
        private TimeSpan _timeLeft;

        private string _activeAddressText = "Searching Address...";
        public string ActiveAddressText { get => _activeAddressText; set { _activeAddressText = value; OnPropertyChanged(); } }

        private string _userProfileImage = "default_avatar.png";
        public string UserProfileImage { get => _userProfileImage; set { _userProfileImage = value; OnPropertyChanged(); } }

        private string _countdownText = "04:32:18";
        public string CountdownText { get => _countdownText; set { _countdownText = value; OnPropertyChanged(); } }

        private int _activeCouponCount = 0;
        public int ActiveCouponCount { get => _activeCouponCount; set { _activeCouponCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoCoupons)); } }
        public bool HasNoCoupons => ActiveCouponCount == 0;

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public ObservableCollection<FlashDealModel> FlashDeals { get; set; } = new ObservableCollection<FlashDealModel>();
        public ObservableCollection<CampaignDisplayModel> Campaigns { get; set; } = new ObservableCollection<CampaignDisplayModel>();
        public ObservableCollection<DiscountDisplayModel> UserDiscounts { get; set; } = new ObservableCollection<DiscountDisplayModel>();
        public ObservableCollection<BogoProductModel> BogoProducts { get; set; } = new ObservableCollection<BogoProductModel>();

        public ICommand CopyCouponCommand { get; }

        public DiscountsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _dbService = databaseService;

            CopyCouponCommand = new Command<string>(async (code) => await CopyCouponToClipboard(code));

            BindingContext = this;

            FlashDeals.Add(new FlashDealModel { Name = "Nutella Hazelnut Spread 750g", Price = 99.90m, OldPrice = 145.00m, DiscountPercent = 31, StockSoldPercent = 82, ImageUrl = "nutella.png" });
            FlashDeals.Add(new FlashDealModel { Name = "Lipton Yellow Label 1000g", Price = 139.00m, OldPrice = 189.00m, DiscountPercent = 26, StockSoldPercent = 65, ImageUrl = "lipton.png" });
            FlashDeals.Add(new FlashDealModel { Name = "Ariel Liquid Detergent 24 Washes", Price = 169.00m, OldPrice = 235.00m, DiscountPercent = 28, StockSoldPercent = 40, ImageUrl = "ariel.png" });

            BogoProducts.Add(new BogoProductModel { Name = "Doritos Taco Mega Size", Description = "158g • Corn Chips", Price = 36.50m, ImageUrl = "doritos.png" });
            BogoProducts.Add(new BogoProductModel { Name = "Fuse Tea Peach 1L", Description = "Iced Tea", Price = 29.90m, ImageUrl = "fusetea.png" });
            BogoProducts.Add(new BogoProductModel { Name = "Magnum Mini Classic 6-pack", Description = "Ice Cream Pack", Price = 149.90m, ImageUrl = "magnum.png" });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHeaderDataAsync();
            await LoadDynamicDataAsync();
            await SyncCartAsync(); 
            StartFlashTimer();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_flashTimer != null && _flashTimer.IsRunning)
            {
                _flashTimer.Stop();
            }
        }

        private async Task SyncCartAsync()
        {
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

        private async Task LoadDynamicDataAsync()
        {
            if (App.CurrentUser == null) return;

            var dbCampaigns = await _dbService.GetActiveCampaignsAsync();
            Campaigns.Clear();
            foreach (var camp in dbCampaigns)
            {
                Campaigns.Add(new CampaignDisplayModel
                {
                    Title = camp.Title,
                    Description = camp.Description,
                    ImageUrl = camp.ImageUrl,
                    BadgeText = camp.BadgeText,
                    SecondaryBadgeText = camp.SecondaryBadgeText,
                    ProductCountText = camp.ProductCountText
                });
            }

            var dbDiscounts = await _dbService.GetUserDiscountsAsync(App.CurrentUser.UserId);
            UserDiscounts.Clear();
            foreach (var disc in dbDiscounts)
            {
                string icon = "\ue54e";

                Color bg = Color.FromArgb("#f2f4f6");
                Color textCol = Color.FromArgb("#575e70");

                if (disc.Code.Contains("TESLIMAT") || disc.Title.Contains("Kargo") || disc.Title.Contains("Teslimat"))
                {
                    icon = "\ueb28";
                    bg = Color.FromArgb("#fff7ed");
                    textCol = Color.FromArgb("#ea580c");
                }
                else if (disc.DiscountAmount > 0 || disc.Code.Contains("50") || disc.Title.ToLower().Contains("discount") || disc.Title.ToLower().Contains("indirim"))
                {
                    icon = "\ueb58";
                    bg = Color.FromArgb("#d9dff5");
                    textCol = Color.FromArgb("#5c6274");
                }

                UserDiscounts.Add(new DiscountDisplayModel
                {
                    Code = disc.Code,
                    Title = disc.Title,
                    Description = disc.Description,
                    IconCode = icon,
                    IconBgColor = bg,
                    IconTextColor = textCol
                });
            }
            ActiveCouponCount = UserDiscounts.Count;
        }

        private async Task CopyCouponToClipboard(string code)
        {
            await Clipboard.Default.SetTextAsync(code);
            await DisplayAlert("Copied", $"{code} coupon code copied to clipboard.", "OK");
        }

        private async void OnNavHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_dbService));
        }

        private async void OnNavCategoriesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CategoryPage(_dbService));
        }

        private async void OnNavCartTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(_dbService));
        }

        private async void OnNavOrdersTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage(_dbService));
        }

        private async void OnNavProfileTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_dbService));
        }

        private void StartFlashTimer()
        {
            _timeLeft = new TimeSpan(4, 32, 18);

            _flashTimer = Application.Current.Dispatcher.CreateTimer();
            _flashTimer.Interval = TimeSpan.FromSeconds(1);
            _flashTimer.Tick += (s, e) =>
            {
                if (_timeLeft.TotalSeconds > 0)
                {
                    _timeLeft = _timeLeft.Add(TimeSpan.FromSeconds(-1));
                    CountdownText = _timeLeft.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    _flashTimer.Stop();
                    CountdownText = "TIME EXPIRED";
                }
            };
            _flashTimer.Start();
        }

        private async Task LoadHeaderDataAsync()
        {
            if (App.CurrentUser == null)
            {
                ActiveAddressText = "Not Logged In";
                return;
            }

            if (!string.IsNullOrWhiteSpace(App.CurrentUser.ProfileImage))
            {
                UserProfileImage = App.CurrentUser.ProfileImage;
            }

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

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}