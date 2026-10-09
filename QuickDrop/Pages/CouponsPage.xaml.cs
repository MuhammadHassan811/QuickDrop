using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using QuickDrop.Services;
using QuickDrop.Models;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickDrop.Pages
{
    public partial class CouponsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<Discount> Coupons { get; set; } = new ObservableCollection<Discount>();

        private int _couponCount;
        public int CouponCount
        {
            get => _couponCount;
            set { _couponCount = value; OnPropertyChanged(); }
        }

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public CouponsPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCouponsAsync();
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

        private async Task LoadCouponsAsync()
        {
            try
            {
                var discounts = await _dbService.GetAllDiscountsAsync();
                Coupons.Clear();
                foreach (var discount in discounts)
                {
                    Coupons.Add(discount);
                }
                CouponCount = Coupons.Count;
            }
            catch (Exception ex)
            {
                CouponCount = 0;
            }
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnCopyCodeTapped(object sender, EventArgs e)
        {
            var label = (Label)sender;
            var selectedDiscount = (Discount)label.BindingContext;
            string code = selectedDiscount.Code;

            if (!string.IsNullOrEmpty(code))
            {
                await Clipboard.Default.SetTextAsync(code);
            await DisplayAlert("Copied", $"{code} coupon code copied to clipboard.", "OK");
            }
        }

        private async void OnGoToWheelTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WheelPage(_dbService));
        }

        private async void OnApplyCouponClicked(object sender, EventArgs e)
        {
            string code = CouponEntry.Text?.Trim().ToUpper();

            if (string.IsNullOrEmpty(code))
            {
                await DisplayAlert("Error", "Please enter a discount code.", "OK");
                return;
            }

            if (code == "QUICK100" || code == "NEX100" || code == "CEVDET75")
            {
                await DisplayAlert("Congratulations", $"{code} code successfully applied to your account!", "OK");
                CouponEntry.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Invalid Code", "The coupon code you entered is invalid or expired.", "OK");
            }
        }

        private async void OnShareTapped(object sender, EventArgs e)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "QuickDrop Discount",
                Text = "Order with QuickDrop and get an instant $75 discount with my code AUGY-75!",
                Uri = "https://quickdrop.app/invite/AUGY-75"
            });
        }

        private async void OnNavCategoriesTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CategoryPage(_dbService));
        }

        private async void OnNavHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_dbService));
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