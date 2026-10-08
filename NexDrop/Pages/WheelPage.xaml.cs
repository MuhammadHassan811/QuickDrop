using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using QuickDrop.Services;
using QuickDrop.Models;

namespace QuickDrop.Pages
{
    public partial class WheelPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private bool _isSpinning = false;
        private int _currentSpins = 1;
        private double _currentRotation = 0;

        private TimeSpan _timeLeft = new TimeSpan(4, 18, 22);
        private bool _isTimerRunning = false;

        private readonly string[] _rewardTitles = { "$50 DISCOUNT!", "FREE DELIVERY", "20% DISCOUNT", "$25 QUICKPOINTS", "FREE WAFER", "$75 DISCOUNT", "TRY AGAIN", "SURPRISE BOX" };
        private readonly string[] _rewardCodes = { "QD50", "FREE", "QD20", "PUAN", "CHOCO", "QD75", "", "MYSTERY" };
        private readonly string[] _rewardDescriptions = { "Valid for all items", "Fast Delivery to Your Door", "Net 20% Off Cart", "Added to QuickPoints Wallet", "Surprise Snack", "Orders $300 and above", "", "Mystery Box" };
        private readonly decimal[] _rewardAmounts = { 50, 0, 20, 25, 0, 75, 0, 0 };
        public WheelPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartTimer();
        }

        private void StartTimer()
        {
            if (_isTimerRunning) return;
            _isTimerRunning = true;

            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (_timeLeft.TotalSeconds > 0)
                {
                    _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                    TimerLabel.Text = _timeLeft.ToString(@"hh\:mm\:ss");
                    return true;
                }
                _isTimerRunning = false;
                return false;
            });
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSpinTapped(object sender, EventArgs e)
        {
            if (_isSpinning) return;
            if (_currentSpins <= 0)
            {
                await DisplayAlert("Info", "Your daily spin limit is reached! Place an order to earn more spins.", "OK");
                return;
            }

            _isSpinning = true;
            _currentSpins--;

            SpinCountText.Text = _currentSpins.ToString();
            RemainText.Text = $"{_currentSpins} Spins Available";

            SpinActionBtn.Opacity = 0.5;

            int winningIndex = new Random().Next(0, 8);
            int extraRounds = 5 + new Random().Next(0, 3);
            double segmentCenter = (winningIndex * 45) + 22.5;
            double targetAngle = (extraRounds * 360) + (360 - segmentCenter);

            _currentRotation += targetAngle;
            await WheelContainer.RotateTo(_currentRotation, 4600, Easing.CubicOut);

            await ProcessReward(winningIndex);

            _isSpinning = false;
            SpinActionBtn.Opacity = 1.0;
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task ProcessReward(int index)
        {
            RewardTitleText.Text = _rewardTitles[index];

            if (index == 6) 
            {
                CongratTitleText.Text = "SORRY!";
                CongratTitleText.TextColor = Color.FromArgb("#b81d27");
                RewardIconText.Text = "\ue5d5";
                RewardIconText.TextColor = Color.FromArgb("#b81d27");
                RewardIconBorder.BackgroundColor = Color.FromArgb("#ffe8e8");
                RewardSubText.Text = "Unfortunately no prize this time. You can try again tomorrow or complete tasks to earn spins.";

                CouponCodeContainer.IsVisible = false;
                ShopBtn.IsVisible = false;
            }
            else 
            {
                CongratTitleText.Text = "CONGRATULATIONS!";
                CongratTitleText.TextColor = Color.FromArgb("#f97316");
                RewardIconText.Text = "\ue8f6";
                RewardIconText.TextColor = Color.FromArgb("#f97316");
                RewardIconBorder.BackgroundColor = Color.FromArgb("#fff7ed");
                RewardSubText.Text = "Coupon applied to your cart instantly. Valid for 24 hours.";

                string finalCode = $"{_rewardCodes[index]}-{GenerateRandomCode(5)}";
                RewardCodeText.Text = finalCode;

                CouponCodeContainer.IsVisible = true;
                ShopBtn.IsVisible = true;

                // DYNAMIC DATABASE RECORD
                if (App.CurrentUser != null)
                {
                    var newDiscount = new Discount
                    {
                        UserId = App.CurrentUser.UserId,
                        Code = finalCode,
                        Title = _rewardTitles[index],
                        Description = _rewardDescriptions[index],
                        DiscountAmount = _rewardAmounts[index],
                        CreatedAt = DateTime.Now,
                        ExpirationDate = DateTime.Now.AddDays(1) 
                    };
                    await _dbService.AddDiscountAsync(newDiscount);
                }
            }

            RewardModal.IsVisible = true;
            await RewardCard.ScaleTo(1.0, 250, Easing.SpringOut);
        }
    

        private async void OnCloseModalTapped(object sender, EventArgs e)
        {
            await RewardCard.ScaleTo(0.9, 150, Easing.CubicIn);
            RewardModal.IsVisible = false;
        }

        private async void OnCopyRewardTapped(object sender, EventArgs e)
        {
            string code = RewardCodeText.Text;
            await Clipboard.Default.SetTextAsync(code);
            await DisplayAlert("Copied", $"{code} code copied to clipboard.", "OK");
        }
    }
}