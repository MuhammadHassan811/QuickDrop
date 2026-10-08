using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Pages
{
    public class CategoryChipModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(TextColor));
                OnPropertyChanged(nameof(CountColor));
                OnPropertyChanged(nameof(CountOpacity));
                OnPropertyChanged(nameof(BorderThickness));
            }
        }

        public string BackgroundColor => IsSelected ? "#f97316" : "#ffffff";
        public string TextColor => IsSelected ? "#ffffff" : "#575e70";
        public string CountColor => IsSelected ? "#ffffff" : "#575e70";
        public double CountOpacity => IsSelected ? 0.9 : 0.75;
        public int BorderThickness => IsSelected ? 0 : 1;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class FavoriteDisplayModel : INotifyPropertyChanged
    {
        public FavoriteProduct Product { get; set; }

        public int Id => Product.Id;
        public string Name => Product.Name;
        public string Brand => Product.Brand;
        public decimal Price => Product.Price;
        public decimal? OldPrice => Product.OldPrice;
        public bool HasOldPrice => Product.HasOldPrice;
        public string ImageUrl => Product.ImageUrl;
        public string BadgeText => Product.BadgeText;
        public string BadgeColor => Product.BadgeColor;
        public string BadgeTextColor => Product.BadgeTextColor;
        public bool HasBadge => Product.HasBadge;

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class FavoritesPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private List<FavoriteProduct> _allFavorites = new List<FavoriteProduct>();

        private List<CartItemDisplayModel> _currentCartItems = new List<CartItemDisplayModel>();

        public ObservableCollection<FavoriteDisplayModel> Products { get; set; } = new ObservableCollection<FavoriteDisplayModel>();
        public ObservableCollection<CategoryChipModel> Categories { get; set; } = new ObservableCollection<CategoryChipModel>();

        private string _totalFavoritesPriceText = "$0.00";
        public string TotalFavoritesPriceText { get => _totalFavoritesPriceText; set { _totalFavoritesPriceText = value; OnPropertyChanged(); } }

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public FavoritesPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadFavoritesAsync();
            await SyncCartAsync();
        }

        private async Task SyncCartAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;

            _currentCartItems = await _dbService.GetCartItemsAsync(App.CurrentUser.UserId);

            UniqueCartItemCount = _currentCartItems.Count;
            if (UniqueCartItemCount > 0)
            {
                decimal subtotal = _currentCartItems.Sum(c => c.Subtotal);
                decimal currentDeliveryFee = subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;
                decimal grandTotal = subtotal + BagFee + currentDeliveryFee;

                TotalCartText = $"${grandTotal:N2}";
            }
            else
            {
                TotalCartText = "$0.00";
            }

            foreach (var prod in Products)
            {
                var cartItem = _currentCartItems.FirstOrDefault(c => c.ProductId == prod.Id);
                prod.CartQuantity = cartItem != null ? cartItem.Quantity : 0;
            }
        }

        private async Task LoadFavoritesAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;

            _allFavorites = await _dbService.GetUserFavoritesAsync(App.CurrentUser.UserId);

            Categories.Clear();
            Categories.Add(new CategoryChipModel { Id = 0, Name = "All", Count = _allFavorites.Count, IsSelected = true });

            var grouped = _allFavorites.GroupBy(x => new { x.CategoryId, x.CategoryName });
            foreach (var group in grouped)
            {
                Categories.Add(new CategoryChipModel
                {
                    Id = group.Key.CategoryId,
                    Name = group.Key.CategoryName,
                    Count = group.Count(),
                    IsSelected = false
                });
            }

            FilterProducts();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void OnCategoryTapped(object sender, EventArgs e)
        {
            var tappedCat = (CategoryChipModel)((TappedEventArgs)e).Parameter;
            foreach (var cat in Categories)
            {
                cat.IsSelected = (cat.Id == tappedCat.Id);
            }
            FilterProducts();
        }

        private void FilterProducts()
        {
            var searchText = SearchEntry.Text?.ToLower() ?? "";
            var selectedCat = Categories.FirstOrDefault(c => c.IsSelected);

            var filtered = _allFavorites.AsEnumerable();

            if (selectedCat != null && selectedCat.Id != 0)
            {
                filtered = filtered.Where(p => p.CategoryId == selectedCat.Id);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(p => p.Name.ToLower().Contains(searchText) || p.Brand.ToLower().Contains(searchText));
            }

            Products.Clear();
            decimal totalFilteredPrice = 0;

            foreach (var item in filtered)
            {
                var displayModel = new FavoriteDisplayModel { Product = item };

                var cartItem = _currentCartItems.FirstOrDefault(c => c.ProductId == item.Id);
                displayModel.CartQuantity = cartItem != null ? cartItem.Quantity : 0;

                Products.Add(displayModel);
                totalFilteredPrice += item.Price;
            }

            TotalFavoritesPriceText = $"${totalFilteredPrice:N2}";
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnBatchAddTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null || Products.Count == 0) return;

            foreach (var displayModel in Products)
            {
                try
                {
                    var cartItem = new CartItem
                    {
                        UserId = App.CurrentUser.UserId,
                        ProductId = displayModel.Id, 
                        Quantity = 1
                    };
                    await _dbService.AddCartItemAsync(cartItem);
                }
                catch { }
            }

            await SyncCartAsync();
        }

        private async void OnFavoriteTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var displayModel = (FavoriteDisplayModel)element.BindingContext;

            bool answer = await DisplayAlert("Remove from Favorites", $"Remove '{displayModel.Name}' from favorites?", "Yes", "No");
            if (answer)
            {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                if (App.CurrentUser != null)
                {
                    await _dbService.RemoveFavoriteAsync(App.CurrentUser.UserId, displayModel.Id);
                }
                _allFavorites.RemoveAll(f => f.Id == displayModel.Id);
                FilterProducts();
            }
        }

        private async void OnIncreaseQuantityTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var displayModel = (FavoriteDisplayModel)element.BindingContext;

            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser != null)
            {
                try
                {
                    var cartItem = new CartItem
                    {
                        UserId = App.CurrentUser.UserId,
                        ProductId = displayModel.Id, 
                        Quantity = 1
                    };
                    await _dbService.AddCartItemAsync(cartItem);
                }
                catch { }

                await SyncCartAsync(); 
            }
        }

        private async void OnDecreaseQuantityTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var displayModel = (FavoriteDisplayModel)element.BindingContext;

            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser != null)
            {
                try
                {
                    await _dbService.DecreaseCartItemAsync(App.CurrentUser.UserId, displayModel.Id);
                }
                catch { }

                await SyncCartAsync(); 
            }
        }

        private async void OnAddToCartTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var displayModel = (FavoriteDisplayModel)element.BindingContext;

            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser != null)
            {
                try
                {
                    var cartItem = new CartItem
                    {
                        UserId = App.CurrentUser.UserId,
                        ProductId = displayModel.Id,
                        Quantity = 1
                    };
                    await _dbService.AddCartItemAsync(cartItem);
                }
                catch { }

                await SyncCartAsync();
            }
        }

        // NAVBAR NAVIGATION
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

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}