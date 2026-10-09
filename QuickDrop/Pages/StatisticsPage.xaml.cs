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
    public class MonthlySpendingModel
    {
        public string MonthName { get; set; }
        public decimal TotalSpent { get; set; }

        public string TotalSpentFormatted => TotalSpent >= 1000 ? $"{TotalSpent / 1000m:0.#}k" : $"{TotalSpent:0}";

        public double BarHeight { get; set; }
        public string BarColor { get; set; }
        public string TextColor { get; set; }
        public bool IsCurrentMonth { get; set; }
    }

    public class CategoryStatModel
    {
        public string Name { get; set; }
        public string IconCode { get; set; }
        public string IconBackgroundColor { get; set; }
        public string IconColor { get; set; }
        public int Percentage { get; set; }
        public decimal Spent { get; set; }
        public string BarColor { get; set; }
        public double BarWidth => 300 * (Percentage / 100.0);
    }

    public class TopProductStatModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int OrderCount { get; set; }
        public decimal Price { get; set; }
    }

    public partial class StatisticsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        public string UserNameSubtitle { get; set; }

        private string _activeAddressText = "Searching Address...";
        public string ActiveAddressText { get => _activeAddressText; set { _activeAddressText = value; OnPropertyChanged(); } }

        private string _userProfileImage = "default_avatar.png";
        public string UserProfileImage { get => _userProfileImage; set { _userProfileImage = value; OnPropertyChanged(); } }

        private string _totalSpentText = "$0.00";
        public string TotalSpentText { get => _totalSpentText; set { _totalSpentText = value; OnPropertyChanged(); } }

        private string _totalOrdersText = "0 Orders";
        public string TotalOrdersText { get => _totalOrdersText; set { _totalOrdersText = value; OnPropertyChanged(); } }

        private string _averageSpendingText = "Avg: $0.00";
        public string AverageSpendingText { get => _averageSpendingText; set { _averageSpendingText = value; OnPropertyChanged(); } }

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public ObservableCollection<MonthlySpendingModel> MonthlySpendings { get; set; } = new ObservableCollection<MonthlySpendingModel>();
        public ObservableCollection<CategoryStatModel> CategoryStats { get; set; } = new ObservableCollection<CategoryStatModel>();
        public ObservableCollection<TopProductStatModel> TopProducts { get; set; } = new ObservableCollection<TopProductStatModel>();

        public StatisticsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _dbService = databaseService;

            if (App.CurrentUser != null)
            {
                UserNameSubtitle = $"{App.CurrentUser.FullName} • Spending & Order Analytics";
            }

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHeaderDataAsync();
            await LoadRealStatisticsAsync();
            await SyncCartAsync(); 
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
                else
                {
                    ActiveAddressText = "Add Address";
                }
            }
            catch
            {
                ActiveAddressText = "Address Not Found";
            }
        }

        private async Task LoadRealStatisticsAsync()
        {
            if (App.CurrentUser == null) return;
            int userId = App.CurrentUser.UserId;

            var orders = await _dbService.GetOrdersAsync(userId);
            TotalSpentText = $"${orders.Sum(o => o.TotalAmount):N2}";
                TotalOrdersText = $"{orders.Count} Orders";

            if (orders.Count == 0) return;

            var now = DateTime.Now;
            var tempMonths = new List<MonthlySpendingModel>();

                string[] trMonths = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);

                var monthlySpent = orders.Where(o =>
                {
                    if (DateTime.TryParse(o.OrderDate?.ToString(), out DateTime parsedDate))
                    {
                        return parsedDate.Year == monthDate.Year && parsedDate.Month == monthDate.Month;
                    }
                    return false;
                }).Sum(o => o.TotalAmount);

                tempMonths.Add(new MonthlySpendingModel
                {
                    MonthName = trMonths[monthDate.Month],
                    TotalSpent = monthlySpent,
                    IsCurrentMonth = (i == 0)
                });
            }

            var maxSpent = tempMonths.Max(m => m.TotalSpent);
            if (maxSpent == 0) maxSpent = 1;

            AverageSpendingText = $"Avg: ${tempMonths.Average(m => m.TotalSpent):0}";

            MonthlySpendings.Clear();
            foreach (var m in tempMonths)
            {
                m.BarHeight = (double)(m.TotalSpent / maxSpent) * 100;
                if (m.BarHeight < 8) m.BarHeight = 8;

                m.BarColor = m.IsCurrentMonth ? "#F97316" : "#E0E3E5";
                m.TextColor = m.IsCurrentMonth ? "#F97316" : "#575E70";
                MonthlySpendings.Add(m);
            }

            var orderItems = await _dbService.GetOrderItemsForUserAsync(userId);
            var allProducts = await _dbService.GetProductsAsync();
            var allCategories = await _dbService.GetAllCategoriesAsync();

            var joinedItems = from oi in orderItems
                              join p in allProducts on oi.ProductId equals p.ProductId
                              select new { p.CategoryId, oi.Subtotal };

            var categoryGroups = joinedItems.GroupBy(x => x.CategoryId)
                                            .Select(g => new {
                                                CategoryId = g.Key,
                                                Spent = g.Sum(x => x.Subtotal)
                                            })
                                            .OrderByDescending(x => x.Spent).ToList();

            decimal totalSpentCategories = categoryGroups.Sum(x => x.Spent);
            if (totalSpentCategories == 0) totalSpentCategories = 1;

            CategoryStats.Clear();
            foreach (var cg in categoryGroups)
            {
                var cat = allCategories.FirstOrDefault(c => c.CategoryId == cg.CategoryId);
                string name = cat != null ? cat.Name : "Other";
                int percentage = (int)Math.Round((cg.Spent / totalSpentCategories) * 100);

                var statModel = new CategoryStatModel { Name = name, Spent = cg.Spent, Percentage = percentage };
                SetCategoryStyle(statModel, cg.CategoryId);
                CategoryStats.Add(statModel);
            }

            var topItemGroups = orderItems.GroupBy(oi => oi.ProductId)
                                          .Select(g => new { ProductId = g.Key, OrderCount = g.Sum(x => x.Quantity) })
                                          .OrderByDescending(x => x.OrderCount).Take(4).ToList();

            TopProducts.Clear();
            foreach (var ti in topItemGroups)
            {
                var p = allProducts.FirstOrDefault(x => x.ProductId == ti.ProductId);
                if (p != null)
                {
                    TopProducts.Add(new TopProductStatModel
                    {
                        Name = p.Name,
                        OrderCount = ti.OrderCount,
                        Price = p.Price,
                        ImageUrl = p.MainImage
                    });
                }
            }
        }

        private void SetCategoryStyle(CategoryStatModel model, int categoryId)
        {
            switch (categoryId)
            {
                case 1:
                    model.IconCode = "\uea35"; model.IconBackgroundColor = "#FDBA74"; model.IconColor = "#431407"; model.BarColor = "#F97316"; break;
                case 2:
                    model.IconCode = "\ueacb"; model.IconBackgroundColor = "#D9DFF5"; model.IconColor = "#141b2b"; model.BarColor = "#575E70"; break;
                case 4:
                case 7:
                    model.IconCode = "\uea53"; model.IconBackgroundColor = "#E0E3E5"; model.IconColor = "#475569"; model.BarColor = "#64748b"; break;
                case 5:
                case 6:
                    model.IconCode = "\ue541"; model.IconBackgroundColor = "#FFDAD7"; model.IconColor = "#410004"; model.BarColor = "#B81D27"; break;
                default:
                    model.IconCode = "\ue8cc"; model.IconBackgroundColor = "#eceef0"; model.IconColor = "#575e70"; model.BarColor = "#cbd5e1"; break;
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