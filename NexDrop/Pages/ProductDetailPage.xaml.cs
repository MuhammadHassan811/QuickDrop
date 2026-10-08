using QuickDrop.Models;
using QuickDrop.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace QuickDrop.Pages
{
    public partial class ProductDetailPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        private Product _currentProduct;
        public Product CurrentProduct
        {
            get => _currentProduct;
            set { _currentProduct = value; OnPropertyChanged(); }
        }

        private string _userProfileImage = "default_avatar.png";
        public string UserProfileImage { get => _userProfileImage; set { _userProfileImage = value; OnPropertyChanged(); } }

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        private int _productCartQuantity = 1;
        public int ProductCartQuantity
        {
            get => _productCartQuantity;
            set
            {
                _productCartQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductTotalPriceText));
            }
        }

        public string ProductTotalPriceText => $"${(CurrentProduct?.Price * ProductCartQuantity ?? 0):N2}";

        public ObservableCollection<Product> SimilarProducts { get; set; } = new();

        public ProductDetailPage(Product selectedProduct, DatabaseService databaseService)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _dbService = databaseService;
            CurrentProduct = selectedProduct;

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (App.CurrentUser != null && !string.IsNullOrWhiteSpace(App.CurrentUser.ProfileImage))
            {
                UserProfileImage = App.CurrentUser.ProfileImage;
            }

            await LoadSimilarProductsAsync();
            await SyncCartAsync();
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
        }

        private async Task LoadSimilarProductsAsync()
        {
            var allProducts = await _dbService.GetProductsAsync();

            var similarProducts = allProducts
                .Where(p => p.CategoryId == CurrentProduct.CategoryId && p.ProductId != CurrentProduct.ProductId)
                .Take(5)
                .ToList();

            SimilarProducts.Clear();
            foreach (var p in similarProducts)
            {
                SimilarProducts.Add(p);
            }
        }

        // --- Quantity Selector Actions (Bottom Bar) ---
        private void OnIncreaseProductQtyTapped(object sender, EventArgs e)
        {
            ProductCartQuantity++;
        }

        private void OnDecreaseProductQtyTapped(object sender, EventArgs e)
        {
            if (ProductCartQuantity > 1)
            {
                ProductCartQuantity--;
            }
        }

        private async void OnAddToCartTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null || CurrentProduct == null) return;

            for (int i = 0; i < ProductCartQuantity; i++)
            {
                await _dbService.AddToCartAsync(App.CurrentUser.UserId, CurrentProduct.ProductId);
            }

            ProductCartQuantity = 1; 
            await SyncCartAsync();
            await DisplayAlert("Added to Cart", $"{CurrentProduct.Name} added to cart successfully.", "OK");
        }

        private async void OnSimilarProductTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.BindingContext is Product clickedProduct)
            {
                await Navigation.PushAsync(new ProductDetailPage(clickedProduct, _dbService));
            }
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnAccordionTapped(object sender, TappedEventArgs e)
        {
            var param = e.Parameter as string;

            if (param == "Desc")
            {
                DescContent.IsVisible = !DescContent.IsVisible;
                DescIcon.Text = DescContent.IsVisible ? "\ue5ce" : "\ue5cf";
            }
            else if (param == "Nutr")
            {
                NutrContent.IsVisible = !NutrContent.IsVisible;
                NutrIcon.Text = NutrContent.IsVisible ? "\ue5ce" : "\ue5cf";
            }
            else if (param == "Storage")
            {
                StorageContent.IsVisible = !StorageContent.IsVisible;
                StorageIcon.Text = StorageContent.IsVisible ? "\ue5ce" : "\ue5cf";
            }
        }

        // --- Navbar Navigation ---
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