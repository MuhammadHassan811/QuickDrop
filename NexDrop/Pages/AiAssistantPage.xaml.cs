using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuickDrop.Models;
using QuickDrop.Services;

namespace QuickDrop.Pages
{
    public class AiProductDisplayModel : INotifyPropertyChanged
    {
        public Product Product { get; set; }

        public int ProductId => Product.ProductId;
        public string Name => Product.Name;
        public decimal Price => Product.Price;
        public string MainImage => Product.MainImage;
        public string Unit => Product.Unit;

        public string FormattedPrice => $"${Price:N2}";

        private int _cartQuantity;
        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                _cartQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInCart));
                OnPropertyChanged(nameof(IsNotInCart));
            }
        }
        public bool IsInCart => CartQuantity > 0;
        public bool IsNotInCart => CartQuantity == 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ChatMessage
    {
        public bool IsUser { get; set; }
        public bool IsBot => !IsUser;
        public string Text { get; set; }
        public string Time { get; set; }
        public string ChefTip { get; set; }
        public bool HasChefTip => !string.IsNullOrEmpty(ChefTip);

        public List<AiProductDisplayModel> DisplayProducts { get; set; }
        public bool HasProducts => DisplayProducts != null && DisplayProducts.Count > 0;
        public decimal TotalPrice => DisplayProducts?.Sum(p => p.Price * (p.CartQuantity > 0 ? p.CartQuantity : 1)) ?? 0;
        public string FormattedTotalPrice => $"{DisplayProducts?.Count ?? 0} items • ${TotalPrice:N2}";
    }

    public partial class AiAssistantPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private readonly GeminiService _geminiService;

        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new();

        public ICommand AddToCartCommand { get; }
        public ICommand DecreaseCartCommand { get; }
        public ICommand AddAllToCartCommand { get; }
        public ICommand SendQuickPromptCommand { get; }

        public string WelcomeTitle => $"Hello {App.CurrentUser?.FullName?.Split(' ')[0] ?? "there"}!";

        public AiAssistantPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            _geminiService = new GeminiService();

            AddToCartCommand = new Command<AiProductDisplayModel>(async (p) => await OnIncreaseQtyAsync(p));
            DecreaseCartCommand = new Command<AiProductDisplayModel>(async (p) => await OnDecreaseQtyAsync(p));
            AddAllToCartCommand = new Command<ChatMessage>(async (msg) => await AddAllProductsToCart(msg));
            SendQuickPromptCommand = new Command<string>((prompt) => SendMessage(prompt));

            BindingContext = this;
            ChatMessages.Clear();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await SyncCartAsync();
        }

        private async Task SyncCartAsync()
        {
            if (App.CurrentUser == null) return;
            var cartItems = await _dbService.GetCartItemsAsync(App.CurrentUser.UserId);

            foreach (var msg in ChatMessages)
            {
                if (msg.HasProducts)
                {
                    foreach (var dp in msg.DisplayProducts)
                    {
                        var itemInCart = cartItems.FirstOrDefault(c => c.ProductId == dp.ProductId);
                        dp.CartQuantity = itemInCart != null ? itemInCart.Quantity : 0;
                    }
                }
            }
        }

        private async Task OnIncreaseQtyAsync(AiProductDisplayModel item)
        {
            if (App.CurrentUser == null || item == null) return;
            await _dbService.AddToCartAsync(App.CurrentUser.UserId, item.ProductId);
            await SyncCartAsync();
        }

        private async Task OnDecreaseQtyAsync(AiProductDisplayModel item)
        {
            if (App.CurrentUser == null || item == null) return;
            await _dbService.DecreaseCartItemAsync(App.CurrentUser.UserId, item.ProductId);
            await SyncCartAsync();
        }

        private async Task AddAllProductsToCart(ChatMessage msg)
        {
            if (App.CurrentUser == null || msg?.DisplayProducts == null) return;
            foreach (var p in msg.DisplayProducts)
            {
                if (p.CartQuantity == 0)
                {
                    await _dbService.AddToCartAsync(App.CurrentUser.UserId, p.ProductId);
                }
            }
            await SyncCartAsync();
            await DisplayAlert("Added to Cart", $"{msg.DisplayProducts.Count} products added to cart!", "Great");
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_dbService));
        }

        private void OnClearChatTapped(object sender, EventArgs e)
        {
            ChatMessages.Clear();
        }

        private void OnSendMessageTapped(object sender, EventArgs e)
        {
            string userText = MessageEntry.Text?.Trim();
            if (string.IsNullOrEmpty(userText)) return;
            MessageEntry.Text = string.Empty;
            SendMessage(userText);
        }

        private async void SendMessage(string userText)
        {
            if (string.IsNullOrEmpty(userText)) return;

            ChatMessages.Add(new ChatMessage { IsUser = true, Text = userText, Time = DateTime.Now.ToString("HH:mm") });
            ScrollToBottom();

            TypingIndicator.IsVisible = true;
            ScrollToBottom();
            AnimateTypingIndicator();

            var allProducts = await _dbService.GetProductsAsync();
            var aiResult = await _geminiService.GetRecipeAndProductsAsync(userText, allProducts);

            TypingIndicator.IsVisible = false;

            if (aiResult != null)
            {
                var displayList = new List<AiProductDisplayModel>();
                if (aiResult.MatchedProducts != null)
                {
                    foreach (var prod in aiResult.MatchedProducts)
                    {
                        displayList.Add(new AiProductDisplayModel { Product = prod, CartQuantity = 0 });
                    }
                }

                ChatMessages.Add(new ChatMessage
                {
                    IsUser = false,
                    Text = aiResult.RecipeText,
                    ChefTip = aiResult.ChefTip,
                    DisplayProducts = displayList,
                    Time = DateTime.Now.ToString("HH:mm")
                });

                await SyncCartAsync();
            }
            else
            {
                ChatMessages.Add(new ChatMessage
                {
                    IsUser = false,
                    Text = "Sorry, our servers are experiencing high traffic. Could you please ask for the recipe again?",
                    Time = DateTime.Now.ToString("HH:mm")
                });
            }

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (ChatMessages.Count > 0)
                ChatCollectionView.ScrollTo(ChatMessages.Last(), position: ScrollToPosition.End, animate: true);
        }

        private async void AnimateTypingIndicator()
        {
            try
            {
                while (TypingIndicator.IsVisible)
                {
                    await Dot1.FadeTo(0.2, 200); await Dot1.FadeTo(1.0, 200);
                    await Dot2.FadeTo(0.2, 200); await Dot2.FadeTo(1.0, 200);
                    await Dot3.FadeTo(0.2, 200); await Dot3.FadeTo(1.0, 200);
                    await Task.Delay(100);
                }
            }
            catch { }
        }
    }
}