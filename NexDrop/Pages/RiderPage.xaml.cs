using Microsoft.Maui.Controls;
using QuickDrop.Models;
using QuickDrop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickDrop.Pages
{
    public partial class RiderPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private List<Order> _allOrders = new();
        private string _activeFilter = "All";

        public RiderPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (App.CurrentUser != null)
            {
                RiderNameLabel.Text = $"{App.CurrentUser.FullName} ({App.CurrentUser.Role})";
            }
            await LoadRiderDataAsync();
        }

        private async Task LoadRiderDataAsync()
        {
            try
            {
                _allOrders = await _dbService.GetAllOrdersAsync();
                UpdateStats();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load dispatch orders: {ex.Message}", "OK");
            }
        }

        private void UpdateStats()
        {
            int inProgress = _allOrders.Count(o => o.Status == "Preparing" || o.Status == "On The Way" || o.Status == "Pending");
            int delivered = _allOrders.Count(o => o.Status == "Delivered");
            decimal estimatedRiderEarnings = delivered * 4.50m; // standard $4.50 per completed drop

            ActiveDeliveriesCountLabel.Text = inProgress.ToString();
            CompletedDeliveriesCountLabel.Text = delivered.ToString();
            ShiftEarningsLabel.Text = $"${estimatedRiderEarnings:F2}";
        }

        private void ApplyFilter()
        {
            IEnumerable<Order> filtered = _allOrders;
            if (_activeFilter == "Active")
            {
                filtered = _allOrders.Where(o => o.Status == "Preparing" || o.Status == "On The Way" || o.Status == "Pending");
            }
            else if (_activeFilter == "Completed")
            {
                filtered = _allOrders.Where(o => o.Status == "Delivered");
            }

            var list = filtered.ToList();
            OrdersCollectionView.ItemsSource = list;
            EmptyOrdersView.IsVisible = list.Count == 0;
            TotalOrdersBannerLabel.Text = $"Showing {list.Count} of {_allOrders.Count}";
        }

        private void OnFilterAllTapped(object sender, TappedEventArgs e)
        {
            _activeFilter = "All";
            HighlightFilter(FilterAllBtn, FilterAllText);
            ApplyFilter();
        }

        private void OnFilterActiveTapped(object sender, TappedEventArgs e)
        {
            _activeFilter = "Active";
            HighlightFilter(FilterActiveBtn, FilterActiveText);
            ApplyFilter();
        }

        private void OnFilterCompletedTapped(object sender, TappedEventArgs e)
        {
            _activeFilter = "Completed";
            HighlightFilter(FilterCompletedBtn, FilterCompletedText);
            ApplyFilter();
        }

        private void HighlightFilter(Border activeBtn, Label activeLabel)
        {
            FilterAllBtn.BackgroundColor = Color.FromArgb("#1E293B");
            FilterAllBtn.Stroke = new SolidColorBrush(Color.FromArgb("#334155"));
            FilterAllText.TextColor = Color.FromArgb("#94A3B8");

            FilterActiveBtn.BackgroundColor = Color.FromArgb("#1E293B");
            FilterActiveBtn.Stroke = new SolidColorBrush(Color.FromArgb("#334155"));
            FilterActiveText.TextColor = Color.FromArgb("#94A3B8");

            FilterCompletedBtn.BackgroundColor = Color.FromArgb("#1E293B");
            FilterCompletedBtn.Stroke = new SolidColorBrush(Color.FromArgb("#334155"));
            FilterCompletedText.TextColor = Color.FromArgb("#94A3B8");

            activeBtn.BackgroundColor = Color.FromArgb("#38BDF8");
            activeBtn.Stroke = new SolidColorBrush(Colors.Transparent);
            activeLabel.TextColor = Color.FromArgb("#0F172A");
        }

        private async void OnPickUpOrderTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is Order order)
            {
                bool confirm = await DisplayAlert("Pick Up Order", $"Set Order #{order.OrderNumber} to 'On The Way'?", "Yes, Start Delivery", "Cancel");
                if (confirm)
                {
                    await _dbService.UpdateOrderStatusAsync(order.Id, "On The Way");
                    await DisplayAlert("Dispatched", $"Order #{order.OrderNumber} is now marked as On The Way!", "OK");
                    await LoadRiderDataAsync();
                }
            }
        }

        private async void OnCompleteDeliveryTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is Order order)
            {
                bool confirm = await DisplayAlert("Complete Delivery", $"Confirm delivery of Order #{order.OrderNumber} to customer?", "Yes, Delivered", "Cancel");
                if (confirm)
                {
                    await _dbService.UpdateOrderStatusAsync(order.Id, "Delivered");
                    await DisplayAlert("Great Job!", $"Order #{order.OrderNumber} marked as Delivered! +$4.50 added to shift earnings.", "OK");
                    await LoadRiderDataAsync();
                }
            }
        }

        private async void OnCallCustomerTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is Order order)
            {
                await DisplayAlert("Customer Contact", $"Dialing customer for Order #{order.OrderNumber}...\n\nPhone: +1 (555) 019-2834\nAddress: {order.OrderSummary}", "OK");
            }
        }

        private async void OnRefreshTapped(object sender, TappedEventArgs e)
        {
            await LoadRiderDataAsync();
            await ProfessionalNotificationPopup.ShowAsync(
                Navigation,
                "Dispatch Refreshed",
                "Active delivery queue and shift earnings updated with the latest dispatch data.",
                NotificationType.Success,
                "Continue");
        }

        private async void OnRiderLogoutTapped(object sender, TappedEventArgs e)
        {
            bool answer = await ProfessionalNotificationPopup.ShowConfirmAsync(
                Navigation,
                "Log Out of Rider Console?",
                "Are you sure you want to end your courier duty shift and log out?",
                NotificationType.Logout,
                "Yes, End Duty",
                "Stay On Duty");

            if (answer)
            {
                App.CurrentUser = null;
                App.CurrentActiveAddress = null;
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
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
    }
}

