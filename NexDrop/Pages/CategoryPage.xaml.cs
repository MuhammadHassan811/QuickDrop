using QuickDrop.Models;
using QuickDrop.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickDrop.Pages
{
    public partial class CategoryPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private List<Category> _allCategories;

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private string _activeAddressText = "Searching Address...";
        public string ActiveAddressText { get => _activeAddressText; set { _activeAddressText = value; OnPropertyChanged(); } }

        private string _userProfileImage = "default_avatar.png";
        public string UserProfileImage { get => _userProfileImage; set { _userProfileImage = value; OnPropertyChanged(); } }

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public CategoryPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHeaderDataAsync();
            await LoadFiltersAsync();
            await LoadCategoriesAsync();
            await SyncCartAsync();
        }

        private async Task LoadHeaderDataAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _databaseService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;

            if (!string.IsNullOrWhiteSpace(App.CurrentUser.ProfileImage))
                UserProfileImage = App.CurrentUser.ProfileImage;

            try
            {
                var addresses = await _databaseService.GetAddressesAsync(App.CurrentUser.UserId);
                if (addresses != null && addresses.Count > 0)
                {
                    var activeAddress = addresses.FirstOrDefault(a => a.IsActive) ?? addresses.First();
                    ActiveAddressText = $"{activeAddress.Title} • {activeAddress.CityAndDistrict.Split(',')[0].Trim()}";
                }
                else ActiveAddressText = "Add Address";
            }
            catch { ActiveAddressText = "Address Not Found"; }
        }

        private async Task SyncCartAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _databaseService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;

            var cartItems = await _databaseService.GetCartItemsAsync(App.CurrentUser.UserId);

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

        private async Task LoadFiltersAsync()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            var filterList = new List<CategoryFilter>
            {
                new CategoryFilter { Name = $"All ({categories.Count})", IsActive = true }
            };

            foreach (var category in categories)
            {
                filterList.Add(new CategoryFilter { Name = category.Name, IsActive = false });
            }

            FiltersCollectionView.ItemsSource = filterList;
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            var products = await _databaseService.GetProductsAsync();

            CategoryCountLabel.Text = $"All Aisles ({categories.Count} Categories)";

            foreach (var category in categories)
            {
                category.ProductCount = products.Count(p => p.CategoryId == category.CategoryId);
            }

            _allCategories = categories;
            CategoriesCollectionView.ItemsSource = _allCategories;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allCategories == null) return;

            string searchQuery = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                CategoriesCollectionView.ItemsSource = _allCategories;
            }
            else
            {
                var filteredCategories = _allCategories.Where(c =>
                    !string.IsNullOrEmpty(c.Name) &&
                    c.Name.ToLowerInvariant().Contains(searchQuery)).ToList();

                CategoriesCollectionView.ItemsSource = filteredCategories;
            }
        }

        
        private async void OnCategoryTapped(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is Category selectedCategory)
            {
                await Navigation.PushAsync(new ProductsPage(selectedCategory, _databaseService));
            }
        }

        private async void OnNavHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_databaseService));
        }

        private async void OnNavCategoriesTapped(object sender, EventArgs e)
        {
          
            await Navigation.PushAsync(new ProductsPage(_databaseService));
        }

        private async void OnNavCartTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(_databaseService));
        }

        private async void OnNavOrdersTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage(_databaseService));
        }

        private async void OnNavProfileTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_databaseService));
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}