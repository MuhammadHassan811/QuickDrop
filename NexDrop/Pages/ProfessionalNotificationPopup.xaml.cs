using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace QuickDrop.Pages
{
    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info,
        Question,
        Logout
    }

    public partial class ProfessionalNotificationPopup : ContentPage
    {
        private readonly TaskCompletionSource<bool> _tcs = new();
        private readonly bool _canDismissWithBackdrop;

        public Task<bool> ResultTask => _tcs.Task;

        public ProfessionalNotificationPopup(
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            string confirmText = "Continue",
            string cancelText = null,
            bool allowBackdropDismiss = true)
        {
            InitializeComponent();
            _canDismissWithBackdrop = allowBackdropDismiss;

            TitleLabel.Text = title;
            MessageLabel.Text = message;
            ConfirmButtonText.Text = confirmText;

            ConfigureStyle(type);

            if (!string.IsNullOrEmpty(cancelText))
            {
                CancelButton.IsVisible = true;
                CancelButtonText.Text = cancelText;
                Grid.SetColumnSpan(ConfirmButton, 1);
            }
            else
            {
                CancelButton.IsVisible = false;
                Grid.SetColumn(ConfirmButton, 0);
                Grid.SetColumnSpan(ConfirmButton, 2);
            }
        }

        private void ConfigureStyle(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    IconGlyph.Text = "✓";
                    IconOuterRing.BackgroundColor = Color.FromArgb("#DCFCE7"); // green-100
                    IconInnerCircle.BackgroundColor = Color.FromArgb("#16A34A"); // green-600
                    ConfirmButton.BackgroundColor = Color.FromArgb("#16A34A");
                    break;

                case NotificationType.Error:
                    IconGlyph.Text = "✕";
                    IconOuterRing.BackgroundColor = Color.FromArgb("#FEE2E2"); // red-100
                    IconInnerCircle.BackgroundColor = Color.FromArgb("#DC2626"); // red-600
                    ConfirmButton.BackgroundColor = Color.FromArgb("#DC2626");
                    break;

                case NotificationType.Warning:
                    IconGlyph.Text = "!";
                    IconOuterRing.BackgroundColor = Color.FromArgb("#FEF3C7"); // amber-100
                    IconInnerCircle.BackgroundColor = Color.FromArgb("#D97706"); // amber-600
                    ConfirmButton.BackgroundColor = Color.FromArgb("#EA580C"); // brand orange
                    break;

                case NotificationType.Question:
                    IconGlyph.Text = "?";
                    IconOuterRing.BackgroundColor = Color.FromArgb("#E0E7FF"); // indigo-100
                    IconInnerCircle.BackgroundColor = Color.FromArgb("#4F46E5"); // indigo-600
                    ConfirmButton.BackgroundColor = Color.FromArgb("#EA580C");
                    break;

                case NotificationType.Logout:
                    IconGlyph.Text = "🚪";
                    IconOuterRing.BackgroundColor = Color.FromArgb("#FEE2E2"); // red-100
                    IconInnerCircle.BackgroundColor = Color.FromArgb("#EF4444"); // red-500
                    ConfirmButton.BackgroundColor = Color.FromArgb("#EF4444");
                    break;

                case NotificationType.Info:
                default:
                    IconGlyph.Text = "ℹ";
                    IconOuterRing.BackgroundColor = Color.FromArgb("#E0F2FE"); // sky-100
                    IconInnerCircle.BackgroundColor = Color.FromArgb("#0284C7"); // sky-600
                    ConfirmButton.BackgroundColor = Color.FromArgb("#0F172A"); // slate-900
                    break;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Smooth zoom-in scale animation
            PopupCard.Scale = 0.8;
            PopupCard.Opacity = 0;
            await Task.WhenAll(
                PopupCard.ScaleTo(1.0, 220, Easing.SpringOut),
                PopupCard.FadeTo(1.0, 200)
            );
        }

        private async Task CloseWithAnimation(bool result)
        {
            await Task.WhenAll(
                PopupCard.ScaleTo(0.85, 150, Easing.CubicIn),
                PopupCard.FadeTo(0, 150)
            );

            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync(false);
            }

            _tcs.TrySetResult(result);
        }

        private async void OnConfirmTapped(object sender, TappedEventArgs e)
        {
            await CloseWithAnimation(true);
        }

        private async void OnCancelTapped(object sender, TappedEventArgs e)
        {
            await CloseWithAnimation(false);
        }

        private async void OnBackdropTapped(object sender, TappedEventArgs e)
        {
            if (_canDismissWithBackdrop)
            {
                await CloseWithAnimation(false);
            }
        }

        // STATIC HELPERS FOR CONVENIENT USAGE ANYWHERE IN THE APP
        public static async Task ShowAsync(
            INavigation navigation,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            string confirmText = "OK")
        {
            var popup = new ProfessionalNotificationPopup(title, message, type, confirmText);
            await navigation.PushModalAsync(popup, false);
            await popup.ResultTask;
        }

        public static async Task<bool> ShowConfirmAsync(
            INavigation navigation,
            string title,
            string message,
            NotificationType type,
            string confirmText = "Yes",
            string cancelText = "Cancel")
        {
            var popup = new ProfessionalNotificationPopup(title, message, type, confirmText, cancelText);
            await navigation.PushModalAsync(popup, false);
            return await popup.ResultTask;
        }

        public static async Task<bool> ShowConfirmAsync(
            INavigation navigation,
            string title,
            string message,
            string confirmText = "Yes",
            string cancelText = "Cancel",
            NotificationType type = NotificationType.Question)
        {
            var popup = new ProfessionalNotificationPopup(title, message, type, confirmText, cancelText);
            await navigation.PushModalAsync(popup, false);
            return await popup.ResultTask;
        }
    }
}

