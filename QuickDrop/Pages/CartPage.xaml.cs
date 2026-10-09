using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Pages
{
    public partial class CartPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<CartItemDisplayModel> CartItems { get; set; } = new();

        public ICommand IncreaseQtyCommand { get; }
        public ICommand DecreaseQtyCommand { get; }
        public ICommand RemoveItemCommand { get; }

        private Discount _appliedDiscount = null;
        private decimal _currentDiscountAmount = 0m;

        public CartPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            IncreaseQtyCommand = new Command<CartItemDisplayModel>(async (item) => await OnIncreaseQtyAsync(item));
            DecreaseQtyCommand = new Command<CartItemDisplayModel>(async (item) => await OnDecreaseQtyAsync(item));
            RemoveItemCommand = new Command<CartItemDisplayModel>(async (item) => await OnRemoveItemAsync(item));

            BindingContext = this;
        }

        public CartPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            IncreaseQtyCommand = new Command<CartItemDisplayModel>(async (item) => await OnIncreaseQtyAsync(item));
            DecreaseQtyCommand = new Command<CartItemDisplayModel>(async (item) => await OnDecreaseQtyAsync(item));
            RemoveItemCommand = new Command<CartItemDisplayModel>(async (item) => await OnRemoveItemAsync(item));

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImage))
            {
                UserProfileImage.Source = App.CurrentUser.ProfileImage;
            }
            else
            {
                UserProfileImage.Source = "default_avatar.png";
            }

            await LoadCartDataAsync();
        }

        private async void OnBackTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async Task LoadCartDataAsync()
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            int userId = App.CurrentUser?.UserId ?? 1;
            var items = await _dbService.GetCartItemsAsync(userId);

            CartItems.Clear();
            foreach (var item in items)
            {
                CartItems.Add(item);
            }

            BindableLayout.SetItemsSource(CartItemsContainer, null);
            BindableLayout.SetItemsSource(CartItemsContainer, CartItems);

            CalculateTotals();
        }

        private async Task OnIncreaseQtyAsync(CartItemDisplayModel item)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            await _dbService.AddToCartAsync(App.CurrentUser.UserId, item.ProductId);
            await LoadCartDataAsync();
        }

        private async Task OnDecreaseQtyAsync(CartItemDisplayModel item)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            await _dbService.RemoveFromCartAsync(App.CurrentUser.UserId, item.ProductId);
            await LoadCartDataAsync();
        }

        private async Task OnRemoveItemAsync(CartItemDisplayModel item)
        {
            bool answer = await DisplayAlert("Remove Item", $"Remove '{item.Name}' from cart?", "Yes", "No");
            if (answer)
            {
                await _dbService.DeleteFromCartAsync(item.CartItemId);
                await LoadCartDataAsync();
            }
        }

        private async void OnClearCartTapped(object sender, System.EventArgs e)
        {
            if (CartItems.Count == 0) return;

            bool answer = await DisplayAlert("Clear Cart", "All items in your cart will be removed. Are you sure?", "Clear", "Cancel");
            if (answer)
            {
                if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
                await _dbService.ClearCartAsync(App.CurrentUser.UserId);

                _appliedDiscount = null;
                _currentDiscountAmount = 0m;
                UpdateCouponUI();

                await LoadCartDataAsync();
            }
        }

        private async void OnApplyCouponClicked(object sender, System.EventArgs e)
        {
            string code = CouponCodeEntry.Text?.Trim();
            if (string.IsNullOrEmpty(code))
            {
                await DisplayAlert("Warning", "Please enter a coupon code.", "OK");
                return;
            }

            var discount = await _dbService.GetDiscountByCodeAsync(code);

            if (discount != null)
            {
                _appliedDiscount = discount;
                _currentDiscountAmount = discount.DiscountAmount;

                AppliedCouponCodeLabel.Text = discount.Code;
                AppliedCouponDescLabel.Text = $"-${_currentDiscountAmount:N2} applied";

                CouponInputGrid.IsVisible = false;
                AppliedCouponBorder.IsVisible = true;
                CouponCodeEntry.Text = string.Empty;

                CalculateTotals();
            }
            else
            {
                await DisplayAlert("Invalid Coupon", "Coupon code not found.", "OK");
            }
        }

        private void OnRemoveCouponTapped(object sender, System.EventArgs e)
        {
            _appliedDiscount = null;
            _currentDiscountAmount = 0m;

            AppliedCouponBorder.IsVisible = false;
            CouponInputGrid.IsVisible = true;

            CalculateTotals();
        }

        private void UpdateCouponUI()
        {
            bool isApplied = _appliedDiscount != null;
            AppliedCouponBorder.IsVisible = isApplied;
            CouponInputGrid.IsVisible = !isApplied;
        }

        private void CalculateTotals()
        {
            bool hasItems = CartItems.Any();
            EmptyCartView.IsVisible = !hasItems;
            CartItemsContainer.IsVisible = hasItems;
            ContactlessView.IsVisible = hasItems;
            SummaryView.IsVisible = hasItems;
            CheckoutButton.IsEnabled = hasItems;

            UpdateCouponUI();

            int rowCount = CartItems.Sum(x => x.Quantity);

            if (!hasItems)
            {
                CartCountBadge.Text = "0 items";
                CheckoutButton.Text = "Cart is Empty";
                FreeDeliveryText.Text = "Delivery fee cannot be calculated";
                FreeDeliveryPercentageLabel.Text = "%0";
                FreeDeliveryProgressBar.WidthRequest = 0;

                NavbarCartBadgeBorder.IsVisible = false;
                return;
            }

            decimal subtotal = CartItems.Sum(x => x.Subtotal);

            decimal bagFee = 0.50m;
            decimal deliveryFee = 14.90m;
            decimal freeDeliveryThreshold = 200.00m;
            decimal discount = _currentDiscountAmount;

            SummaryDiscountRow.IsVisible = _appliedDiscount != null;

            if (subtotal >= freeDeliveryThreshold)
            {
                deliveryFee = 0;
                OldDeliveryLabel.IsVisible = true;

                var formattedText = new FormattedString();
                formattedText.Spans.Add(new Span { Text = "Congratulations! ", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#ea580c") });
                formattedText.Spans.Add(new Span { Text = "You earned free delivery.", TextColor = Color.FromArgb("#191c1e") });
                FreeDeliveryText.FormattedText = formattedText;

                FreeDeliveryPercentageLabel.Text = "%100";
                FreeDeliveryProgressBar.WidthRequest = 300;
            }
            else
            {
                OldDeliveryLabel.IsVisible = false;
                decimal remaining = freeDeliveryThreshold - subtotal;
                int percentage = (int)((subtotal / freeDeliveryThreshold) * 100);

                var formattedText = new FormattedString();
                formattedText.Spans.Add(new Span { Text = "Add " });
                formattedText.Spans.Add(new Span { Text = $"${remaining:N2} ", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#ea580c") });
                formattedText.Spans.Add(new Span { Text = "for free delivery!" });

                FreeDeliveryText.FormattedText = formattedText;
                FreeDeliveryPercentageLabel.Text = $"{percentage}%";
                FreeDeliveryProgressBar.WidthRequest = percentage * 3;
            }

            decimal grandTotal = Math.Max(0, subtotal - discount + deliveryFee + bagFee);

            CartCountBadge.Text = $"{rowCount} items";

            NavbarCartBadgeBorder.IsVisible = rowCount > 0;
            NavbarCartBadgeText.Text = rowCount.ToString();

            SubtotalLabel.Text = $"${subtotal:N2}";
            DeliveryLabel.Text = deliveryFee == 0 ? "Free" : $"${deliveryFee:N2}";
            BagFeeLabel.Text = $"${bagFee:N2}";
            GrandTotalLabel.Text = $"${grandTotal:N2}";
            CheckoutButton.Text = $"Confirm Order (${grandTotal:N2})";
        }

        private async void OnCheckoutClicked(object sender, System.EventArgs e)
        {
            if (App.CurrentUser == null) App.CurrentUser = await _dbService.GetDefaultUserAsync();
            var addresses = await _dbService.GetAddressesAsync(App.CurrentUser.UserId);
            if (addresses == null || addresses.Count == 0)
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Address Required",
                    "Please save a delivery address in your profile before placing your order.",
                    NotificationType.Warning,
                    "Add Address");
                return;
            }

            CheckoutButton.Text = "Creating Order...";
            CheckoutButton.IsEnabled = false;

            await Task.Delay(1500);

            decimal subtotal = CartItems.Sum(x => x.Subtotal);
            decimal discount = _appliedDiscount != null ? _appliedDiscount.DiscountAmount : 0m;
            decimal deliveryFee = subtotal >= 200m ? 0 : 14.90m;
            decimal grandTotal = Math.Max(0, subtotal - discount + deliveryFee + 0.50m);

            // SAVE ORDER TO DATABASE
            await _dbService.PlaceOrderAsync(App.CurrentUser.UserId, grandTotal);

            await ProfessionalNotificationPopup.ShowAsync(
                Navigation,
                "Order Confirmed! 🚀",
                $"Your order of ${grandTotal:N2} has been placed successfully.\nOur courier is preparing to dispatch your items right now!",
                NotificationType.Success,
                "Track Order");

            _appliedDiscount = null;
            _currentDiscountAmount = 0m;

            // NAVIGATE TO ORDERS PAGE
            await Navigation.PushAsync(new OrdersPage(_dbService));

            CheckoutButton.Text = $"Confirm Order (${grandTotal:N2})";
            CheckoutButton.IsEnabled = true;
        }

        private async void OnNavCategoriesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CategoryPage(_dbService));
        }

        private async void OnNavProductsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProductsPage(_dbService));
        }

        private async void OnNavHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_dbService));
        }

        private void OnNavCartTapped(object sender, EventArgs e)
        {
        }

        private async void OnNavOrdersTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OrdersPage(_dbService));
        }

        private async void OnNavProfileTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_dbService));
        }
    }
}