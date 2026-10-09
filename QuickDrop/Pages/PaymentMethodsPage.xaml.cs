using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using QuickDrop.Services;
using QuickDrop.Models;

namespace QuickDrop.Pages
{
    public partial class PaymentMethodsPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<PaymentMethod> PaymentMethods { get; set; } = new ObservableCollection<PaymentMethod>();

        private decimal _totalSpentAmount;
        public decimal TotalSpentAmount
        {
            get => _totalSpentAmount;
            set { _totalSpentAmount = value; OnPropertyChanged(); }
        }

        private int _cardsCount;
        public int CardsCount
        {
            get => _cardsCount;
            set { _cardsCount = value; OnPropertyChanged(); }
        }

        private const decimal DeliveryFee = 14.90m;
        private const decimal BagFee = 0.50m;
        private const decimal FreeDeliveryThreshold = 200.00m;

        private int _uniqueCartItemCount = 0;
        public int UniqueCartItemCount
        {
            get => _uniqueCartItemCount;
            set { _uniqueCartItemCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasCartItems)); }
        }

        public bool HasCartItems => UniqueCartItemCount > 0;

        private string _totalCartText = "";
        public string TotalCartText
        {
            get => _totalCartText;
            set { _totalCartText = value; OnPropertyChanged(); }
        }

        public PaymentMethodsPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPaymentDataAsync();
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

        private async Task LoadPaymentDataAsync()
        {
            if (App.CurrentUser == null) return;
            int userId = App.CurrentUser.UserId;

            var cards = await _dbService.GetPaymentMethodsAsync(userId);
            PaymentMethods.Clear();
            foreach (var card in cards)
            {
                PaymentMethods.Add(card);
            }
            CardsCount = PaymentMethods.Count;

            TotalSpentAmount = 45.00m;
        }

        // KARTI VARSAYILAN YAP
        private async void OnMakeDefaultCardTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var selectedCard = (PaymentMethod)element.BindingContext;

            foreach (var card in PaymentMethods)
            {
                card.IsDefault = (card.Id == selectedCard.Id);
                await _dbService.UpdatePaymentMethodAsync(card);
            }

            await LoadPaymentDataAsync();
        }

        // EDIT CARD
        private async void OnEditCardTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var selectedCard = (PaymentMethod)element.BindingContext;
            await DisplayAlert("Edit", $"{selectedCard.CardName} edit card screen will be added soon.", "OK");
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAddCardTapped(object sender, EventArgs e)
        {
            await DisplayAlert("New Card", "Secure Masterpass payment screen will open.", "OK");
        }

        private async void OnDeleteCardTapped(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            var selectedCard = (PaymentMethod)element.BindingContext;

            bool answer = await DisplayAlert("Delete Card", $"Are you sure you want to delete {selectedCard.CardName}?", "Yes", "Cancel");

            if (answer)
            {
                PaymentMethods.Remove(selectedCard);
                CardsCount = PaymentMethods.Count;
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
    }
}