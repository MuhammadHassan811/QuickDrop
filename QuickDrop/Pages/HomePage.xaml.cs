using QuickDrop.Models;
using QuickDrop.Services;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;

namespace QuickDrop.Pages
{
    public class HomeProductDisplayModel : INotifyPropertyChanged
    {
        public Product Product { get; set; }

        public int ProductId => Product.ProductId;
        public string Name => Product.Name;
        public string Brand => Product.Brand;
        public decimal Price => Product.Price;
        public decimal? OldPrice => Product.OldPrice;
        public bool HasDiscount => Product.HasDiscount;
        public string MainImage => Product.MainImage;
        public string Unit => Product.Unit;

        public string FormattedPrice => $"${Price:N2}";
        public string FormattedOldPrice => OldPrice.HasValue ? $"${OldPrice.Value:N2}" : string.Empty;
        public string FormattedDiscount => HasDiscount && OldPrice.HasValue ? $"-{((OldPrice.Value - Price) / OldPrice.Value):P0}" : string.Empty;

        private int _cartQuantity;
        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                _cartQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInCart));
                OnPropertyChanged(nameof(IsNotInCart));
            }
        }
        public bool IsInCart => CartQuantity > 0;
        public bool IsNotInCart => CartQuantity == 0;

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class HomeFlashDealModel : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int DiscountPercent { get; set; }
        public bool HasDiscount => OldPrice > Price;

        private int _cartQuantity;
        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                _cartQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInCart));
                OnPropertyChanged(nameof(IsNotInCart));
            }
        }
        public bool IsInCart => CartQuantity > 0;
        public bool IsNotInCart => CartQuantity == 0;

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class HomeCampaignDisplayModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string BadgeText { get; set; }
    }

    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private IDispatcherTimer _flashTimer;
        private TimeSpan _timeLeft;

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private string _activeAddressText = "Searching Address...";
        public string ActiveAddressText { get => _activeAddressText; set { _activeAddressText = value; OnPropertyChanged(); } }

        private string _userProfileImage = "default_avatar.png";
        public string UserProfileImage { get => _userProfileImage; set { _userProfileImage = value; OnPropertyChanged(); } }

        private string _countdownText = "03:41:09";
        public string CountdownText { get => _countdownText; set { _countdownText = value; OnPropertyChanged(); } }

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }

        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public ObservableCollection<HomeCampaignDisplayModel> Campaigns { get; set; } = new ObservableCollection<HomeCampaignDisplayModel>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<HomeFlashDealModel> FlashDeals { get; set; } = new ObservableCollection<HomeFlashDealModel>();
        public ObservableCollection<HomeProductDisplayModel> PopularProducts { get; set; } = new ObservableCollection<HomeProductDisplayModel>();

        public ICommand GoToCategoryCommand { get; }
        public ICommand GoToProductDetailCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand DecreaseCartCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand AddFlashDealToCartCommand { get; }
        public ICommand DecreaseFlashDealCartCommand { get; }

        public HomePage(DatabaseService databaseService)
        {
            InitializeComponent();
            _dbService = databaseService;

            GoToCategoryCommand = new Command<Category>(async (cat) => await Navigation.PushAsync(new ProductsPage(cat, _dbService)));

            GoToProductDetailCommand = new Command<HomeProductDisplayModel>(async (model) => {
                if (model?.Product != null)
                    await Navigation.PushAsync(new ProductDetailPage(model.Product, _dbService));
            });

            AddToCartCommand = new Command<object>(async (item) => {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                if (App.CurrentUser == null || item == null) return;

                int productId = 0;
                if (item is HomeProductDisplayModel p) productId = p.ProductId;

                if (productId > 0)
                {
                    await _dbService.AddToCartAsync(App.CurrentUser.UserId, productId);
                    await SyncCartAsync();
                }
            });

            DecreaseCartCommand = new Command<object>(async (item) => {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                if (App.CurrentUser == null || item == null) return;

                int productId = 0;
                if (item is HomeProductDisplayModel p) productId = p.ProductId;

                if (productId > 0)
                {
                    await _dbService.DecreaseCartItemAsync(App.CurrentUser.UserId, productId);
                    await SyncCartAsync();
                }
            });

            AddFlashDealToCartCommand = new Command<object>(async (item) => {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                if (App.CurrentUser == null || item == null) return;

                int productId = 0;
                if (item is HomeFlashDealModel f) productId = f.ProductId;

                if (productId > 0)
                {
                    await _dbService.AddToCartAsync(App.CurrentUser.UserId, productId);
                    await SyncCartAsync();
                }
            });

            DecreaseFlashDealCartCommand = new Command<object>(async (item) => {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                if (App.CurrentUser == null || item == null) return;

                int productId = 0;
                if (item is HomeFlashDealModel f) productId = f.ProductId;

                if (productId > 0)
                {
                    await _dbService.DecreaseCartItemAsync(App.CurrentUser.UserId, productId);
                    await SyncCartAsync();
                }
            });

            ToggleFavoriteCommand = new Command<object>(async (item) => {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                if (App.CurrentUser == null || item == null) return;

                int productId = 0;

                if (item is HomeProductDisplayModel p)
                {
                    productId = p.ProductId;
                    p.IsFavorite = !p.IsFavorite;
                }
                else if (item is HomeFlashDealModel f)
                {
                    productId = f.ProductId;
                    f.IsFavorite = !f.IsFavorite;
                }

                if (productId > 0)
                {
                    await _dbService.ToggleFavoriteAsync(App.CurrentUser.UserId, productId);
                }
            });

            BindingContext = this;
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

            foreach (var prod in PopularProducts)
            {
                var cartItem = cartItems.FirstOrDefault(c => c.ProductId == prod.ProductId);
                prod.CartQuantity = cartItem != null ? cartItem.Quantity : 0;
            }

            foreach (var flash in FlashDeals)
            {
                var cartItem = cartItems.FirstOrDefault(c => c.ProductId == flash.ProductId);
                flash.CartQuantity = cartItem != null ? cartItem.Quantity : 0;
            }
        }

        private async Task LoadHeaderDataAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;

            if (!string.IsNullOrWhiteSpace(App.CurrentUser.ProfileImage))
                UserProfileImage = App.CurrentUser.ProfileImage;

            try
            {
                var addresses = await _dbService.GetAddressesAsync(App.CurrentUser.UserId);
                if (addresses == null || addresses.Count == 0 || !addresses.Any(a => a.IsActive))
                {
                    var resolved = await LocationService.Instance.ResolveAndSetActiveLocationForUserAsync(_dbService, App.CurrentUser.UserId);
                    if (resolved != null)
                    {
                        addresses = await _dbService.GetAddressesAsync(App.CurrentUser.UserId);
                    }
                }

                if (addresses != null && addresses.Count > 0)
                {
                    var activeAddress = addresses.FirstOrDefault(a => a.IsActive) ?? addresses.First();
                    ActiveAddressText = $"{activeAddress.Title} • {activeAddress.CityAndDistrict.Split(',')[0].Trim()}";
                }
                else ActiveAddressText = "Add Address";
            }
            catch { ActiveAddressText = "Address Not Found"; }
        }

        private async Task LoadDynamicDataAsync()
        {
            var dbCategories = await _dbService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var cat in dbCategories) Categories.Add(cat);

            var dbCampaigns = await _dbService.GetActiveCampaignsAsync();
            Campaigns.Clear();
            foreach (var camp in dbCampaigns)
            {
                Campaigns.Add(new HomeCampaignDisplayModel
                {
                    Title = camp.Title,
                    Description = camp.Description,
                    ImageUrl = camp.ImageUrl,
                    BadgeText = camp.BadgeText
                });
            }

            var userFavorites = App.CurrentUser != null ? await _dbService.GetUserFavoritesAsync(App.CurrentUser.UserId) : new List<FavoriteProduct>();
            var favProductIds = userFavorites.Select(f => f.Id).ToList();

            FlashDeals.Clear();
            FlashDeals.Add(new HomeFlashDealModel { ProductId = 101, Brand = "Ferrero", Name = "Rocher Chocolate", Description = "200g", Price = 82.50m, OldPrice = 110.00m, DiscountPercent = 25, ImageUrl = "rocher.png", IsFavorite = favProductIds.Contains(101) });
            FlashDeals.Add(new HomeFlashDealModel { ProductId = 102, Brand = "Nescafe", Name = "Gold Coffee", Description = "100g Jar", Price = 101.50m, OldPrice = 145.00m, DiscountPercent = 30, ImageUrl = "nescafe.png", IsFavorite = favProductIds.Contains(102) });
            FlashDeals.Add(new HomeFlashDealModel { ProductId = 103, Brand = "Pringles", Name = "Sour Cream Cips", Description = "165g", Price = 60.00m, OldPrice = 0, DiscountPercent = 0, ImageUrl = "pringles.png", IsFavorite = favProductIds.Contains(103) });

            var allProducts = await _dbService.GetProductsAsync();
            var popular = allProducts.Take(6).ToList();
            PopularProducts.Clear();
            foreach (var p in popular)
            {
                PopularProducts.Add(new HomeProductDisplayModel { Product = p, IsFavorite = favProductIds.Contains(p.ProductId) });
            }
        }

        private void StartFlashTimer()
        {
            _timeLeft = new TimeSpan(3, 41, 9);

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

        private async void OnSearchBoxTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProductsPage(_dbService));
        }

        private async void OnCampaignBannerTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DiscountsPage(_dbService));
        }

        private async void OnNavCategoriesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CategoryPage(_dbService));
        }
        private async void OnNavProductsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProductsPage(_dbService));
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

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void OnNavAiAssistantTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AiAssistantPage(_dbService));
        }

        private async void OnAiPulseLoaded(object sender, EventArgs e)
        {
            if (sender is BoxView dot)
            {
                try
                {
                    while (true)
                    {
                        await dot.FadeTo(0.1, 800);
                        await dot.FadeTo(1.0, 800);
                    }
                }
                catch { }
            }
        }
    }
}