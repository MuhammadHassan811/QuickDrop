using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Pages
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        public Product Product { get; set; }

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

        public string Name => Product.Name;
        public string Brand => Product.Brand;
        public string Unit => Product.Unit;
        public string MainImage => Product.MainImage;
        public decimal Price => Product.Price;
        public decimal? OldPrice => Product.OldPrice;
        public bool HasDiscount => Product.HasDiscount;
        public bool HasOldPrice => Product.HasOldPrice;
        public string FormattedPrice => Product.FormattedPrice;
        public string FormattedOldPrice => Product.FormattedOldPrice;
        public string FormattedDiscount => Product.FormattedDiscount;
        public string FavoriteIcon => Product.FavoriteIcon;

        public void ToggleFavorite()
        {
            Product.IsFavorite = !Product.IsFavorite;
            OnPropertyChanged(nameof(FavoriteIcon));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProductCategoryChipModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(TextColor));
                OnPropertyChanged(nameof(BorderThickness));
            }
        }

        public string BackgroundColor => IsSelected ? "#EA580C" : "#ffffff";
        public string TextColor => IsSelected ? "#ffffff" : "#191c1e";
        public int BorderThickness => IsSelected ? 0 : 1;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public partial class ProductsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly Category _category;
        private readonly DatabaseService _dbService;

        private List<ProductViewModel> _allProducts = new List<ProductViewModel>();

        private int _uniqueCartItemCount;
        public int UniqueCartItemCount
        {
            get => _uniqueCartItemCount;
            set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); }
        }

        private decimal _totalCartPrice;
        public decimal TotalCartPrice
        {
            get => _totalCartPrice;
            set { _totalCartPrice = value; OnPropertyChanged(); }
        }

        public bool HasCartItems => UniqueCartItemCount > 0;

        private int _displayedProductCount;
        public int DisplayedProductCount
        {
            get => _displayedProductCount;
            set { _displayedProductCount = value; OnPropertyChanged(); }
        }

        private string _activeAddressText = "Loading...";
        public string ActiveAddressText
        {
            get => _activeAddressText;
            set { _activeAddressText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductViewModel> Products { get; set; } = new ObservableCollection<ProductViewModel>();

        public ObservableCollection<ProductCategoryChipModel> Categories { get; set; } = new ObservableCollection<ProductCategoryChipModel>();

        public ProductsPage(Category category, DatabaseService databaseService)
        {
            InitializeComponent();
            _category = category;
            _dbService = databaseService;
            BindingContext = this;

            NavigationPage.SetHasNavigationBar(this, false);
            if (PageTitleLabel != null) PageTitleLabel.Text = _category != null ? _category.Name : "All Products";
        }

        public ProductsPage(DatabaseService databaseService) : this(null, databaseService)
        {
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHeaderDataAsync();
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await SyncCartFromDbAsync();
        }

        private async Task LoadHeaderDataAsync()
        {
            if (App.CurrentUser == null) return;
            if (HeaderProfileImage != null) HeaderProfileImage.Source = "default_avatar.png";

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

        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            Categories.Add(new ProductCategoryChipModel { Id = 0, Name = "All", IsSelected = true });

            try
            {
                var dbCategories = await _dbService.GetAllCategoriesAsync();
                foreach (var cat in dbCategories)
                {
                    Categories.Add(new ProductCategoryChipModel { Id = cat.CategoryId, Name = cat.Name, IsSelected = false });
                }

                if (_category != null)
                {
                    var targetCat = Categories.FirstOrDefault(c => c.Id == _category.CategoryId);
                    if (targetCat != null)
                    {
                        Categories.First().IsSelected = false;
                        targetCat.IsSelected = true;
                    }
                }
            }
            catch { }
        }

        private async Task LoadProductsAsync()
        {
            var products = await _dbService.GetProductsAsync();

            _allProducts.Clear();
            foreach (var p in products)
            {
                _allProducts.Add(new ProductViewModel { Product = p, CartQuantity = 0 });
            }

            FilterProducts();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void OnCategoryTapped(object sender, EventArgs e)
        {
            var tappedCat = (ProductCategoryChipModel)((TappedEventArgs)e).Parameter;
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

            var filtered = _allProducts.AsEnumerable();

            if (selectedCat != null && selectedCat.Id != 0)
            {
                filtered = filtered.Where(p => p.Product.CategoryId == selectedCat.Id);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(p => p.Name.ToLower().Contains(searchText) || (p.Brand != null && p.Brand.ToLower().Contains(searchText)));
            }

            Products.Clear();
            var finalList = filtered.ToList();
            foreach (var item in finalList)
            {
                Products.Add(item);
            }

            DisplayedProductCount = finalList.Count;
        }

        private async Task SyncCartFromDbAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;
            var dbCartItems = await _dbService.GetCartItemsAsync(App.CurrentUser.UserId);

            foreach (var vm in _allProducts)
            {
                var cartItem = dbCartItems.FirstOrDefault(c => c.ProductId == vm.Product.ProductId);
                vm.CartQuantity = cartItem != null ? cartItem.Quantity : 0;
            }
            UpdateCartTotals();
        }

        private async void OnAddInitialTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;
            var vm = (ProductViewModel)((BindableObject)sender).BindingContext;

            await _dbService.AddToCartAsync(App.CurrentUser.UserId, vm.Product.ProductId);
            vm.CartQuantity = 1;
            UpdateCartTotals();
        }

        private async void OnIncreaseTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;
            var vm = (ProductViewModel)((BindableObject)sender).BindingContext;

            await _dbService.AddToCartAsync(App.CurrentUser.UserId, vm.Product.ProductId);
            vm.CartQuantity++;
            UpdateCartTotals();
        }

        private async void OnDecreaseTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            if (App.CurrentUser == null) return;
            var vm = (ProductViewModel)((BindableObject)sender).BindingContext;

            if (vm.CartQuantity > 0)
            {
                await _dbService.RemoveFromCartAsync(App.CurrentUser.UserId, vm.Product.ProductId);
                vm.CartQuantity--;
            }
            UpdateCartTotals();
        }

        private void UpdateCartTotals()
        {
            UniqueCartItemCount = _allProducts.Count(p => p.CartQuantity > 0);
            TotalCartPrice = _allProducts.Sum(p => p.CartQuantity * p.Price);
        }

        private async void OnGoToCartTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(_dbService));
        }

        private async void OnFavoriteTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null)
            {
                await DisplayAlert("Warning", "You must log in.", "OK");
                return;
            }

            var vm = (ProductViewModel)((BindableObject)sender).BindingContext;
            vm.ToggleFavorite();

            if (vm.Product.IsFavorite)
            {
                var newFavorite = new Favorite { UserId = App.CurrentUser.UserId, ProductId = vm.Product.ProductId };
                await _dbService.AddFavoriteAsync(newFavorite);
            }
            else
            {
                await _dbService.RemoveFavoriteAsync(App.CurrentUser.UserId, vm.Product.ProductId);
            }
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnProductTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.BindingContext is ProductViewModel vm)
            {
                await Navigation.PushAsync(new ProductDetailPage(vm.Product, _dbService));
            }
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
        private async void OnNavHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_dbService));
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}