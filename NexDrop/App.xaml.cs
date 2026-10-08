using QuickDrop.Pages;
using QuickDrop.Models; 

namespace QuickDrop
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static Address CurrentActiveAddress { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new NavigationPage(new SplashPage()));
            window.Width = 400;
            window.Height = 852;
            return window;
        }
    }
}