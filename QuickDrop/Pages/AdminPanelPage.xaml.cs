using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Pages
{
    public partial class AdminPanelPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        private string _totalRevenueText = "$0.00";
        public string TotalRevenueText
        {
            get => _totalRevenueText;
            set { _totalRevenueText = value; OnPropertyChanged(); }
        }

        private int _totalOrdersCount;
        public int TotalOrdersCount
        {
            get => _totalOrdersCount;
            set { _totalOrdersCount = value; OnPropertyChanged(); }
        }

        private int _totalProductsCount;
        public int TotalProductsCount
        {
            get => _totalProductsCount;
            set { _totalProductsCount = value; OnPropertyChanged(); }
        }

        private int _totalUsersCount;
        public int TotalUsersCount
        {
            get => _totalUsersCount;
            set { _totalUsersCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Order> OrdersList { get; set; } = new();
        public ObservableCollection<Product> ProductsList { get; set; } = new();
        public ObservableCollection<User> UsersList { get; set; } = new();
        public ObservableCollection<Review> ReviewsList { get; set; } = new();

        private List<Product> _allProductsCache = new();
        private List<Review> _allReviewsCache = new();

        public AdminPanelPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // 1. Orders
                var orders = await _dbService.GetAllOrdersAsync();
                OrdersList.Clear();
                foreach (var o in orders)
                {
                    OrdersList.Add(o);
                }
                TotalOrdersCount = orders.Count;

                decimal revenue = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
                TotalRevenueText = $"${revenue:N2}";

                // 2. Products
                var products = await _dbService.GetAllProductsAsync();
                _allProductsCache = products.ToList();
                ProductsList.Clear();
                foreach (var p in products)
                {
                    ProductsList.Add(p);
                }
                TotalProductsCount = products.Count;

                // 3. Users
                var users = await _dbService.GetAllUsersAsync();
                UsersList.Clear();
                foreach (var u in users)
                {
                    UsersList.Add(u);
                }
                TotalUsersCount = users.Count;

                // 4. Reviews
                var reviews = await _dbService.GetAllReviewsAsync();
                _allReviewsCache = reviews.ToList();
                ApplyReviewsSort("All");

                if (_allReviewsCache.Count > 0)
                {
                    double avgScore = Math.Round(_allReviewsCache.Average(r => (r.RiderRating + r.FoodQualityRating) / 2.0), 1);
                    AverageRatingBannerLabel.Text = $"★ {avgScore:F1} / 5.0 ({_allReviewsCache.Count} reviews)";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load admin data: {ex.Message}", "OK");
            }
        }

        // ==========================================
        // TAB SWITCHING
        // ==========================================
        private void ResetTabs()
        {
            OrdersSection.IsVisible = false;
            ProductsSection.IsVisible = false;
            AddProductSection.IsVisible = false;
            UsersSection.IsVisible = false;
            ReviewsSection.IsVisible = false;

            TabOrdersBtn.BackgroundColor = Colors.Transparent;
            TabOrdersText.TextColor = Color.FromArgb("#64748B");

            TabProductsBtn.BackgroundColor = Colors.Transparent;
            TabProductsText.TextColor = Color.FromArgb("#64748B");

            TabAddProductBtn.BackgroundColor = Colors.Transparent;
            TabAddProductText.TextColor = Color.FromArgb("#64748B");

            TabUsersBtn.BackgroundColor = Colors.Transparent;
            TabUsersText.TextColor = Color.FromArgb("#64748B");

            TabReviewsBtn.BackgroundColor = Colors.Transparent;
            TabReviewsText.TextColor = Color.FromArgb("#64748B");
        }

        private void OnSelectOrdersTabTapped(object sender, EventArgs e)
        {
            ResetTabs();
            OrdersSection.IsVisible = true;
            TabOrdersBtn.BackgroundColor = Color.FromArgb("#EA580C");
            TabOrdersText.TextColor = Colors.White;
        }

        private void OnSelectProductsTabTapped(object sender, EventArgs e)
        {
            ResetTabs();
            ProductsSection.IsVisible = true;
            TabProductsBtn.BackgroundColor = Color.FromArgb("#EA580C");
            TabProductsText.TextColor = Colors.White;
        }

        private void OnSelectAddProductTabTapped(object sender, EventArgs e)
        {
            ResetTabs();
            AddProductSection.IsVisible = true;
            TabAddProductBtn.BackgroundColor = Color.FromArgb("#EA580C");
            TabAddProductText.TextColor = Colors.White;
        }

        private void OnSelectUsersTabTapped(object sender, EventArgs e)
        {
            ResetTabs();
            UsersSection.IsVisible = true;
            TabUsersBtn.BackgroundColor = Color.FromArgb("#EA580C");
            TabUsersText.TextColor = Colors.White;
        }

        private void OnSelectReviewsTabTapped(object sender, EventArgs e)
        {
            ResetTabs();
            ReviewsSection.IsVisible = true;
            TabReviewsBtn.BackgroundColor = Color.FromArgb("#EA580C");
            TabReviewsText.TextColor = Colors.White;
        }

        private void ApplyReviewsSort(string sortMode)
        {
            IEnumerable<Review> sorted = _allReviewsCache;
            if (sortMode == "Rider")
            {
                sorted = _allReviewsCache.OrderByDescending(r => r.RiderRating).ThenByDescending(r => r.CreatedAt);
            }
            else if (sortMode == "Food")
            {
                sorted = _allReviewsCache.OrderByDescending(r => r.FoodQualityRating).ThenByDescending(r => r.CreatedAt);
            }
            else
            {
                sorted = _allReviewsCache.OrderByDescending(r => r.CreatedAt);
            }

            ReviewsList.Clear();
            foreach (var r in sorted)
            {
                ReviewsList.Add(r);
            }
        }

        private void OnSortReviewsAllTapped(object sender, TappedEventArgs e)
        {
            HighlightReviewSort(SortAllBtn, SortAllText);
            ApplyReviewsSort("All");
        }

        private void OnSortReviewsRiderTapped(object sender, TappedEventArgs e)
        {
            HighlightReviewSort(SortRiderBtn, SortRiderText);
            ApplyReviewsSort("Rider");
        }

        private void OnSortReviewsFoodTapped(object sender, TappedEventArgs e)
        {
            HighlightReviewSort(SortFoodBtn, SortFoodText);
            ApplyReviewsSort("Food");
        }

        private void HighlightReviewSort(Border activeBtn, Label activeLabel)
        {
            SortAllBtn.BackgroundColor = Color.FromArgb("#F1F5F9");
            SortAllText.TextColor = Color.FromArgb("#475569");

            SortRiderBtn.BackgroundColor = Color.FromArgb("#F1F5F9");
            SortRiderText.TextColor = Color.FromArgb("#475569");

            SortFoodBtn.BackgroundColor = Color.FromArgb("#F1F5F9");
            SortFoodText.TextColor = Color.FromArgb("#475569");

            activeBtn.BackgroundColor = Color.FromArgb("#EA580C");
            activeLabel.TextColor = Colors.White;
        }

        // ==========================================
        // ORDER STATUS TRANSITIONS
        // ==========================================
        private async void OnSetStatusPreparing(object sender, EventArgs e) => await ChangeOrderStatus(sender, "Preparing");
        private async void OnSetStatusOnWay(object sender, EventArgs e) => await ChangeOrderStatus(sender, "On The Way");
        private async void OnSetStatusDelivered(object sender, EventArgs e) => await ChangeOrderStatus(sender, "Delivered");
        private async void OnSetStatusCancelled(object sender, EventArgs e) => await ChangeOrderStatus(sender, "Cancelled");

        private async Task ChangeOrderStatus(object sender, string newStatus)
        {
            if (sender is BindableObject bo && bo.BindingContext is Order order)
            {
                order.Status = newStatus;
                await _dbService.UpdateOrderStatusAsync(order.Id, newStatus);
                await LoadDashboardDataAsync();
            }
        }

        // ==========================================
        // PRODUCT INVENTORY ACTIONS
        // ==========================================
        private async void OnIncreaseStockTapped(object sender, EventArgs e)
        {
            if (sender is BindableObject bo && bo.BindingContext is Product product)
            {
                product.Stock++;
                await _dbService.UpdateProductAsync(product);
                await LoadDashboardDataAsync();
            }
        }

        private async void OnDecreaseStockTapped(object sender, EventArgs e)
        {
            if (sender is BindableObject bo && bo.BindingContext is Product product)
            {
                if (product.Stock > 0)
                {
                    product.Stock--;
                    await _dbService.UpdateProductAsync(product);
                    await LoadDashboardDataAsync();
                }
            }
        }

        private async void OnDeleteProductTapped(object sender, EventArgs e)
        {
            if (sender is BindableObject bo && bo.BindingContext is Product product)
            {
                bool answer = await DisplayAlert("Confirm Delete", $"Delete {product.Name} permanently from catalog?", "Yes, Delete", "Cancel");
                if (answer)
                {
                    await _dbService.DeleteProductAsync(product.ProductId);
                    await LoadDashboardDataAsync();
                }
            }
        }

        private void OnAdminProductSearchChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.ToLowerInvariant() ?? "";
            ProductsList.Clear();
            var matches = string.IsNullOrWhiteSpace(query)
                ? _allProductsCache
                : _allProductsCache.Where(p => p.Name.ToLowerInvariant().Contains(query) || (p.Brand != null && p.Brand.ToLowerInvariant().Contains(query)));

            foreach (var p in matches)
            {
                ProductsList.Add(p);
            }
        }

        // ==========================================
        // ADD NEW PRODUCT
        // ==========================================
        private async void OnSaveNewProductTapped(object sender, EventArgs e)
        {
            string name = NewProductNameEntry.Text?.Trim();
            string brand = NewProductBrandEntry.Text?.Trim();
            string priceStr = NewProductPriceEntry.Text?.Trim();
            string stockStr = NewProductStockEntry.Text?.Trim();
            string unit = NewProductUnitEntry.Text?.Trim();
            string catStr = NewProductCategoryEntry.Text?.Trim();
            string img = NewProductImageEntry.Text?.Trim();
            string desc = NewProductDescEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(priceStr))
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Incomplete Form",
                    "Please provide at least a product title, brand name, and unit price.",
                    NotificationType.Warning,
                    "OK");
                return;
            }

            if (!decimal.TryParse(priceStr, out decimal price))
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Invalid Price",
                    "Please enter a valid numeric price format (e.g. 19.99).",
                    NotificationType.Error,
                    "OK");
                return;
            }

            int stock = int.TryParse(stockStr, out int sVal) ? sVal : 25;
            int categoryId = int.TryParse(catStr, out int cVal) ? cVal : 1;
            if (string.IsNullOrWhiteSpace(img)) img = "muz1.png";
            if (string.IsNullOrWhiteSpace(unit)) unit = "1 pcs";

            var newProd = new Product
            {
                Name = name,
                Brand = brand,
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                ImageUrl = img,
                Unit = unit,
                Description = string.IsNullOrWhiteSpace(desc) ? $"{brand} {name}" : desc
            };

            await _dbService.AddProductAsync(newProd);
            await ProfessionalNotificationPopup.ShowAsync(
                Navigation,
                "Product Published",
                $"'{name}' has been successfully added to the active QuickDrop store catalog.",
                NotificationType.Success,
                "Done");

            // Reset inputs
            NewProductNameEntry.Text = "";
            NewProductBrandEntry.Text = "";
            NewProductPriceEntry.Text = "";
            NewProductStockEntry.Text = "";
            NewProductUnitEntry.Text = "";
            NewProductCategoryEntry.Text = "";
            NewProductImageEntry.Text = "";
            NewProductDescEntry.Text = "";

            await LoadDashboardDataAsync();
            OnSelectProductsTabTapped(this, EventArgs.Empty);
        }

        // ==========================================
        // USER ROLE SWITCHER
        // ==========================================
        private async void OnToggleRoleTapped(object sender, EventArgs e)
        {
            if (sender is BindableObject bo && bo.BindingContext is User user)
            {
                string nextRole = user.Role switch
                {
                    "Customer" => "Rider",
                    "Rider" => "Vendor",
                    "Vendor" => "Admin",
                    "Admin" => "Customer",
                    _ => "Customer"
                };

                user.Role = nextRole;
                await _dbService.UpdateUserRoleAsync(user.UserId, nextRole);
                await LoadDashboardDataAsync();
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Role Updated",
                    $"{user.FullName} is now designated as: {nextRole}",
                    NotificationType.Success,
                    "Done");
            }
        }

        private async void OnCreateUserPromptTapped(object sender, EventArgs e)
        {
            string fullName = await DisplayPromptAsync("Create User", "Enter Full Name:", "Next", "Cancel");
            if (string.IsNullOrWhiteSpace(fullName)) return;

            string email = await DisplayPromptAsync("Create User", "Enter Email Address:", "Next", "Cancel", keyboard: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(email)) return;

            string password = await DisplayPromptAsync("Create User", "Enter Password:", "Next", "Cancel", initialValue: "123456");
            if (string.IsNullOrWhiteSpace(password)) return;

            string roleChoice = await DisplayActionSheet("Assign User Role", "Cancel", null, "Customer", "Rider", "Vendor", "Admin");
            if (string.IsNullOrWhiteSpace(roleChoice) || roleChoice == "Cancel") return;

            var newUser = new User
            {
                FullName = fullName.Trim(),
                Email = email.Trim(),
                Password = password,
                Role = roleChoice,
                Phone = "+1 555-0199"
            };

            bool created = await _dbService.RegisterUserAsync(newUser);
            if (created)
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "User Created",
                    $"{newUser.FullName} has been added successfully with role: {newUser.Role}",
                    NotificationType.Success,
                    "OK");
                await LoadDashboardDataAsync();
            }
            else
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Creation Failed",
                    "A user with this email address already exists.",
                    NotificationType.Error,
                    "Try Again");
            }
        }

        // ==========================================
        // HEADER ACTIONS
        // ==========================================
        private async void OnBackToAppTapped(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new HomePage(_dbService));
            }
        }

        private async void OnAdminLogoutTapped(object sender, EventArgs e)
        {
            bool answer = await ProfessionalNotificationPopup.ShowConfirmAsync(
                Navigation,
                "Log Out of Admin Portal?",
                "Are you sure you want to exit your administrative session and log out?",
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

        private async void OnRefreshTapped(object sender, EventArgs e)
        {
            await LoadDashboardDataAsync();
            await ProfessionalNotificationPopup.ShowAsync(
                Navigation,
                "Dashboard Synchronized",
                "Real-time revenue, orders, catalog items, and vendor metrics have been updated successfully.",
                NotificationType.Success,
                "Done");
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
