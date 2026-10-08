using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using QuickDrop.Services;
using QuickDrop.Models;

namespace QuickDrop.Pages
{
    public partial class AddressesPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        public ObservableCollection<Address> Addresses { get; set; } = new ObservableCollection<Address>();

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount { get => _uniqueCartItemCount; set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); } }
        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText { get => _totalCartText; set { _totalCartText = value; OnPropertyChanged(); } }

        public AddressesPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAddressesAsync();
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

        private async Task LoadAddressesAsync()
        {
            if (App.CurrentUser == null) return;

            var addressList = await _dbService.GetAddressesAsync(App.CurrentUser.UserId);

            Addresses.Clear();
            foreach (var addr in addressList)
            {
                Addresses.Add(addr);
            }

            AddressCountTopLabel.Text = $"{Addresses.Count} Saved Delivery Addresses";
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAddAddressTapped(object sender, EventArgs e)
        {
            string title = await DisplayPromptAsync("New Address", "Enter address title (e.g. Home, Office):", "Next", "Cancel");
            if (string.IsNullOrWhiteSpace(title)) return;

            string detail = await DisplayPromptAsync("Full Address", "Enter detailed street address:", "Save", "Cancel");
            if (string.IsNullOrWhiteSpace(detail)) return;

            // E�er listede adres yoksa, otomatik varsay�lan (IsActive=true) olsun
            bool makeActive = Addresses.Count == 0;

            var newAddress = new Address
            {
                UserId = App.CurrentUser.UserId,
                Title = title,
                FullAddress = detail,
                CityAndDistrict = "Downtown, Center",
                IsActive = makeActive
            };

            await _dbService.AddAddressAsync(newAddress);
            await LoadAddressesAsync(); // Listeyi yenile
        }

        private async void OnUseCurrentLocationTapped(object sender, EventArgs e)
        {
            if (App.CurrentUser == null) return;

            try
            {
                var activeLoc = await LocationService.Instance.ResolveAndSetActiveLocationForUserAsync(_dbService, App.CurrentUser.UserId);
                await LoadAddressesAsync();

                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Location Updated",
                    $"Active delivery address dynamically updated to:\n📍 {activeLoc.CityAndDistrict}\n({activeLoc.FullAddress})",
                    NotificationType.Success,
                    "Great");
            }
            catch (Exception ex)
            {
                await ProfessionalNotificationPopup.ShowAsync(
                    Navigation,
                    "Location Error",
                    "Could not determine current location. Please verify device permissions.",
                    NotificationType.Error,
                    "OK");
            }
        }

        private async void OnSelectAddressTapped(object sender, EventArgs e)
        {
            var border = (Border)sender;
            var selectedAddress = (Address)border.BindingContext;

            foreach (var addr in Addresses)
            {
                addr.IsActive = (addr.Id == selectedAddress.Id);
                await _dbService.UpdateAddressAsync(addr); 
            }

            await LoadAddressesAsync(); 
        }

        private async void OnEditAddressTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var selectedAddress = (Address)element.BindingContext;

            string newDetail = await DisplayPromptAsync("Edit", $"Enter new address details for {selectedAddress.Title}:", "Update", "Cancel", initialValue: selectedAddress.FullAddress);

            if (!string.IsNullOrWhiteSpace(newDetail) && newDetail != selectedAddress.FullAddress)
            {
                selectedAddress.FullAddress = newDetail;
                await _dbService.UpdateAddressAsync(selectedAddress);
                await LoadAddressesAsync();
            }
        }

        private async void OnDeleteAddressTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var selectedAddress = (Address)element.BindingContext;

            bool answer = await DisplayAlert("Delete Address", $"Are you sure you want to delete {selectedAddress.Title}?", "Yes", "Cancel");
            if (answer)
            {
                await _dbService.DeleteAddressAsync(selectedAddress);
                await LoadAddressesAsync();
            }
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
    }
}