using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using QuickDrop.Models;
using QuickDrop.Services;
using System;
using System.Threading.Tasks;

namespace QuickDrop.Pages
{
    public partial class ReviewOrderPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private readonly Order _order;
        private int _selectedRiderRating = 5;
        private int _selectedFoodRating = 5;

        public ReviewOrderPage(DatabaseService dbService, Order order)
        {
            InitializeComponent();
            _dbService = dbService;
            _order = order;

            OrderNumberHeaderLabel.Text = $"Order #{order.OrderNumber}";
            UpdateRiderStarsDisplay();
            UpdateFoodStarsDisplay();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                var existing = await _dbService.GetReviewForOrderAsync(_order.Id);
                if (existing != null)
                {
                    _selectedRiderRating = existing.RiderRating;
                    _selectedFoodRating = existing.FoodQualityRating;
                    RiderCommentEditor.Text = existing.RiderComment;
                    FoodCommentEditor.Text = existing.FoodQualityComment ?? existing.VendorComment;
                    UpdateRiderStarsDisplay();
                    UpdateFoodStarsDisplay();
                }
            }
            catch { }
        }

        private void OnRiderStar1Tapped(object sender, TappedEventArgs e) { _selectedRiderRating = 1; UpdateRiderStarsDisplay(); }
        private void OnRiderStar2Tapped(object sender, TappedEventArgs e) { _selectedRiderRating = 2; UpdateRiderStarsDisplay(); }
        private void OnRiderStar3Tapped(object sender, TappedEventArgs e) { _selectedRiderRating = 3; UpdateRiderStarsDisplay(); }
        private void OnRiderStar4Tapped(object sender, TappedEventArgs e) { _selectedRiderRating = 4; UpdateRiderStarsDisplay(); }
        private void OnRiderStar5Tapped(object sender, TappedEventArgs e) { _selectedRiderRating = 5; UpdateRiderStarsDisplay(); }

        private void OnFoodStar1Tapped(object sender, TappedEventArgs e) { _selectedFoodRating = 1; UpdateFoodStarsDisplay(); }
        private void OnFoodStar2Tapped(object sender, TappedEventArgs e) { _selectedFoodRating = 2; UpdateFoodStarsDisplay(); }
        private void OnFoodStar3Tapped(object sender, TappedEventArgs e) { _selectedFoodRating = 3; UpdateFoodStarsDisplay(); }
        private void OnFoodStar4Tapped(object sender, TappedEventArgs e) { _selectedFoodRating = 4; UpdateFoodStarsDisplay(); }
        private void OnFoodStar5Tapped(object sender, TappedEventArgs e) { _selectedFoodRating = 5; UpdateFoodStarsDisplay(); }

        private void UpdateRiderStarsDisplay()
        {
            var activeColor = Color.FromArgb("#FBBF24");
            var inactiveColor = Color.FromArgb("#475569");

            RiderStar1.TextColor = _selectedRiderRating >= 1 ? activeColor : inactiveColor;
            RiderStar2.TextColor = _selectedRiderRating >= 2 ? activeColor : inactiveColor;
            RiderStar3.TextColor = _selectedRiderRating >= 3 ? activeColor : inactiveColor;
            RiderStar4.TextColor = _selectedRiderRating >= 4 ? activeColor : inactiveColor;
            RiderStar5.TextColor = _selectedRiderRating >= 5 ? activeColor : inactiveColor;
        }

        private void UpdateFoodStarsDisplay()
        {
            var activeColor = Color.FromArgb("#FBBF24");
            var inactiveColor = Color.FromArgb("#475569");

            FoodStar1.TextColor = _selectedFoodRating >= 1 ? activeColor : inactiveColor;
            FoodStar2.TextColor = _selectedFoodRating >= 2 ? activeColor : inactiveColor;
            FoodStar3.TextColor = _selectedFoodRating >= 3 ? activeColor : inactiveColor;
            FoodStar4.TextColor = _selectedFoodRating >= 4 ? activeColor : inactiveColor;
            FoodStar5.TextColor = _selectedFoodRating >= 5 ? activeColor : inactiveColor;
        }

        private async void OnSubmitReviewClicked(object sender, EventArgs e)
        {
            try
            {
                var review = new Review
                {
                    OrderId = _order.Id,
                    OrderNumber = _order.OrderNumber,
                    CustomerUserId = App.CurrentUser?.UserId ?? 1,
                    CustomerName = App.CurrentUser?.FullName ?? "Valued Customer",
                    RiderRating = _selectedRiderRating,
                    RiderComment = RiderCommentEditor.Text?.Trim() ?? "Great delivery!",
                    RiderName = "Alex Rivers",
                    FoodQualityRating = _selectedFoodRating,
                    FoodQualityComment = FoodCommentEditor.Text?.Trim() ?? "Delicious quality!",
                    VendorComment = FoodCommentEditor.Text?.Trim() ?? "Delicious quality!",
                    VendorName = "QuickDrop Fresh",
                    CreatedAt = DateTime.Now
                };

                await _dbService.SaveReviewAsync(review);
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Review Submitted",
                    "Thank you for your feedback! Your ratings for the courier and vendor help us keep QuickDrop quality high.",
                    NotificationType.Success,
                    "Done");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Submission Error",
                    $"Could not save your review: {ex.Message}",
                    NotificationType.Error,
                    "OK");
            }
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}

