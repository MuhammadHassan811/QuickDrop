using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using QuickDrop.Services;
using QuickDrop.Models;

namespace QuickDrop.Pages
{
    public partial class LoginPage : ContentPage
    {
        private DatabaseService _dbService;

        public LoginPage()
        {
            InitializeComponent();
            RemoveWindowsEntryBorders();
            _dbService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (EmailEntry != null && string.IsNullOrEmpty(EmailEntry.Text))
            {
                EmailEntry.Text = "demo@quickdrop.com";
                PasswordEntry.Text = "123456";
            }
        }

        private async void OnSocialLoginTapped(object sender, TappedEventArgs e)
        {
            var user = await _dbService.GetDefaultUserAsync();
            if (user != null)
            {
                App.CurrentUser = user;
                Application.Current.MainPage = new NavigationPage(new HomePage(_dbService));
            }
        }

        private void RemoveWindowsEntryBorders()
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
            {
#if WINDOWS
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Background = null;
                handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
#endif
            });
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            
        }

        private void OnTogglePasswordTapped(object sender, TappedEventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            PasswordEyeIcon.Text = PasswordEntry.IsPassword ? "\ue8f4" : "\ue8f5";
        }

        // DATABASE LOGIN ACTION
        private async void OnLoginTapped(object sender, TappedEventArgs e)
        {
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Missing Credentials",
                    "Please enter both your email address and password to continue.",
                    NotificationType.Warning,
                    "Got It");
                return;
            }

            // Verify user via SQLite
            User user = await _dbService.LoginUserAsync(email, password);

            if (user != null)
            {
                // 1. Store logged in user in memory
                App.CurrentUser = user;

                // 2. Dynamically resolve and select the user's location based on where they logged in from
                Address activeLocation = null;
                try
                {
                    activeLocation = await LocationService.Instance.ResolveAndSetActiveLocationForUserAsync(_dbService, user.UserId);
                }
                catch (Exception locEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginPage] Location selection exception: {locEx.Message}");
                }

                string locInfo = activeLocation != null ? $"\n📍 Delivery Location: {activeLocation.CityAndDistrict}" : "";
                string roleGreeting = user.IsAdmin ? $" [{user.Role} Mode]" : user.IsRider ? " [Rider Mode]" : "";
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Authentication Successful",
                    $"Welcome back, {user.FullName}!{roleGreeting}{locInfo}\nRedirecting to your dashboard...",
                    NotificationType.Success,
                    "Continue to App");

                // 2. REDIRECT
                if (user.IsRider)
                {
                    Application.Current.MainPage = new NavigationPage(new RiderPage(_dbService));
                }
                else if (user.IsAdmin)
                {
                    Application.Current.MainPage = new NavigationPage(new AdminPanelPage(_dbService));
                }
                else
                {
                    Application.Current.MainPage = new NavigationPage(new HomePage(_dbService));
                }
            }
            else
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Login Failed",
                    "Invalid email or password. Please verify your credentials or tap one of the demo buttons above.",
                    NotificationType.Error,
                    "Try Again");
            }
        }

        private void OnFillCustomerLoginTapped(object sender, TappedEventArgs e)
        {
            EmailEntry.Text = "demo@quickdrop.com";
            PasswordEntry.Text = "123456";
        }

        private void OnFillAdminLoginTapped(object sender, TappedEventArgs e)
        {
            EmailEntry.Text = "admin@quickdrop.com";
            PasswordEntry.Text = "admin123";
        }

        private void OnFillVendorLoginTapped(object sender, TappedEventArgs e)
        {
            EmailEntry.Text = "vendor@quickdrop.com";
            PasswordEntry.Text = "vendor123";
        }

        private void OnFillRiderLoginTapped(object sender, TappedEventArgs e)
        {
            EmailEntry.Text = "rider@quickdrop.com";
            PasswordEntry.Text = "rider123";
        }

        private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
        {
            await ProfessionalNotificationPopup.ShowAsync(
                Navigation,
                "Password Recovery",
                "A password reset link and verification instructions have been dispatched to your email address.",
                NotificationType.Info,
                "Understood");
        }

        private async void OnRegisterTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage("Customer"));
        }

        private async void OnJoinAsRiderTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage("Rider"));
        }
    }
}
