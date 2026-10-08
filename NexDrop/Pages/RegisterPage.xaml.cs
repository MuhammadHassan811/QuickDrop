using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private bool _isTermsAccepted = false;
        private DatabaseService _dbService;
        private string _selectedRole = "Customer";

        public RegisterPage(string initialRole = "Customer")
        {
            InitializeComponent();
            RemoveWindowsEntryBorders();
            _dbService = new DatabaseService();
            SelectRole(initialRole ?? "Customer");
        }

        private void SelectRole(string role)
        {
            _selectedRole = role;

            // Reset cards to default
            RoleCustomerCard.BackgroundColor = Colors.White;
            RoleCustomerCard.Stroke = Color.FromArgb("#E2E8F0");
            RoleCustomerCard.StrokeThickness = 1;
            RoleCustomerTitle.TextColor = Color.FromArgb("#475569");

            RoleRiderCard.BackgroundColor = Colors.White;
            RoleRiderCard.Stroke = Color.FromArgb("#E2E8F0");
            RoleRiderCard.StrokeThickness = 1;
            RoleRiderTitle.TextColor = Color.FromArgb("#475569");

            RoleVendorCard.BackgroundColor = Colors.White;
            RoleVendorCard.Stroke = Color.FromArgb("#E2E8F0");
            RoleVendorCard.StrokeThickness = 1;
            RoleVendorTitle.TextColor = Color.FromArgb("#475569");

            if (role == "Rider")
            {
                RoleRiderCard.BackgroundColor = Color.FromArgb("#E0F2FE");
                RoleRiderCard.Stroke = Color.FromArgb("#0284C7");
                RoleRiderCard.StrokeThickness = 1.5;
                RoleRiderTitle.TextColor = Color.FromArgb("#0284C7");
            }
            else if (role == "Vendor")
            {
                RoleVendorCard.BackgroundColor = Color.FromArgb("#F3E8FF");
                RoleVendorCard.Stroke = Color.FromArgb("#9333EA");
                RoleVendorCard.StrokeThickness = 1.5;
                RoleVendorTitle.TextColor = Color.FromArgb("#9333EA");
            }
            else
            {
                _selectedRole = "Customer";
                RoleCustomerCard.BackgroundColor = Color.FromArgb("#FFF7ED");
                RoleCustomerCard.Stroke = Color.FromArgb("#EA580C");
                RoleCustomerCard.StrokeThickness = 1.5;
                RoleCustomerTitle.TextColor = Color.FromArgb("#EA580C");
            }
        }

        private void OnSelectCustomerRoleTapped(object sender, TappedEventArgs e) => SelectRole("Customer");
        private void OnSelectRiderRoleTapped(object sender, TappedEventArgs e) => SelectRole("Rider");
        private void OnSelectVendorRoleTapped(object sender, TappedEventArgs e) => SelectRole("Vendor");

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var animation = new Animation(v => PingDot.Opacity = v, 1, 0.2);
            animation.Commit(this, "PingAnim", length: 1000, easing: Easing.CubicInOut, repeat: () => true);
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnTogglePasswordTapped(object sender, TappedEventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            PasswordRepeatEntry.IsPassword = PasswordEntry.IsPassword;
            PasswordEyeIcon.Text = PasswordEntry.IsPassword ? "\ue8f4" : "\ue8f5";
        }

        private void OnTermsTapped(object sender, TappedEventArgs e)
        {
            _isTermsAccepted = !_isTermsAccepted;
            TermsCheckbox.BackgroundColor = _isTermsAccepted ? Color.FromArgb("#f97316") : Color.FromArgb("#e0e3e5");
            TermsCheckIcon.Opacity = _isTermsAccepted ? 1 : 0;
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            string password = e.NewTextValue ?? string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                SetStrengthUI("#e0e3e5", "#e0e3e5", "#e0e3e5", "\ue86c", "#64748b", "Enter password");
                return;
            }

            int score = 0;
            if (password.Length >= 6) score++;
            if (password.Length >= 8) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++; 
            if (Regex.IsMatch(password, @"[a-zA-Z]")) score++;

            if (score <= 1)
            {
                SetStrengthUI("#ff4d4f", "#e0e3e5", "#e0e3e5", "\ue002", "#ff4d4f", "Weak password");
            }
            else if (score == 2 || score == 3)
            {
                SetStrengthUI("#faad14", "#faad14", "#e0e3e5", "\ue88e", "#faad14", "Medium password");
            }
            else
            {
                SetStrengthUI("#f97316", "#f97316", "#f97316", "\ue86c", "#ea580c", "Strong password");
            }
        }

        private void SetStrengthUI(string b1, string b2, string b3, string iconText, string textColor, string text)
        {
            StrengthBar1.Color = Color.FromArgb(b1);
            StrengthBar2.Color = Color.FromArgb(b2);
            StrengthBar3.Color = Color.FromArgb(b3);
            StrengthIcon.Text = iconText;
            StrengthIcon.TextColor = Color.FromArgb(textColor);
            StrengthLabel.Text = text;
            StrengthLabel.TextColor = Color.FromArgb(textColor);
        }

        // DATABASE REGISTRATION ACTION
        private async void OnRegisterSubmitTapped(object sender, TappedEventArgs e)
        {
            if (!_isTermsAccepted)
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Terms & Conditions",
                    "Please accept the Terms of Service and Privacy Policy to create your account.",
                    NotificationType.Warning,
                    "Understood");
                return;
            }

            string name = NameEntry.Text?.Trim();
            string email = EmailEntry.Text?.Trim();
            string phone = PhoneEntry.Text?.Trim();
            string password = PasswordEntry.Text;
            string repeatPassword = PasswordRepeatEntry.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Incomplete Form",
                    "Please complete all required fields (Name, Email, and Password).",
                    NotificationType.Warning,
                    "OK");
                return;
            }

            if (password != repeatPassword)
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Password Mismatch",
                    "The passwords you entered do not match. Please re-enter them carefully.",
                    NotificationType.Error,
                    "Fix Password");
                return;
            }

            var newUser = new User
            {
                FullName = name,
                Email = email,
                Phone = phone,
                Password = password,
                Role = _selectedRole
            };

            bool isSuccess = await _dbService.RegisterUserAsync(newUser);

            if (isSuccess)
            {
                try
                {
                    await LocationService.Instance.ResolveAndSetActiveLocationForUserAsync(_dbService, newUser.UserId);
                }
                catch { }

                string roleMsg = _selectedRole switch
                {
                    "Rider" => "Welcome to the QuickDrop Fleet! Your rider account is active. Log in to start receiving delivery dispatches.",
                    "Vendor" => "Your Vendor account is active! Log in to manage your storefront, catalog, and inventory.",
                    _ => "Your QuickDrop account is active and ready! You can now log in."
                };

                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    $"{_selectedRole} Account Created",
                    roleMsg,
                    NotificationType.Success,
                    "Continue to Login");
                await Navigation.PopAsync();
            }
            else
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Registration Failed",
                    "This email address is already registered. Please sign in or use a different email.",
                    NotificationType.Error,
                    "Understood");
            }
        }

        private async void OnLoginLinkTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
