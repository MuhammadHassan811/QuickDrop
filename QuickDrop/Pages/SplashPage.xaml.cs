using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace QuickDrop.Pages
{
    public partial class SplashPage : ContentPage
    {
        private const double MaxProgressBarWidth = 320;

        public SplashPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Shell.SetNavBarIsVisible(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            AnimateExpressHubDot();

            await Run10SecondLoadingSequenceAsync();
        }

        private void AnimateExpressHubDot()
        {
            var animation = new Animation(v => ExpressHubDot.Opacity = v, 1, 0.2);
            animation.Commit(this, "ExpressHubPulse", length: 700, easing: Easing.CubicInOut, repeat: () => true);
        }

        private async Task Run10SecondLoadingSequenceAsync()
        {
            // Stage 1: 0-35% (3 sec)
            StatusTitleLabel.Text = "Synchronizing catalog and warehouses";
            StatusLabel.Text = "Checking database...";
            var dbInitTask = Task.Run(async () =>
            {
                try
                {
                    var db = new QuickDrop.Services.DatabaseService();
                    await db.InitAsync();
                    var defaultUser = await db.GetDefaultUserAsync();
                    if (App.CurrentUser == null && defaultUser != null)
                    {
                        App.CurrentUser = defaultUser;
                    }
                }
                catch { }
            });
            _ = AnimateProgressPercentage(0, 35, 3000);
            await CustomProgressBar.AnimateWidth(MaxProgressBarWidth * 0.35, 3000, Easing.Linear);
            await dbInitTask;

            // Stage 2: 35-68% (3 sec)
            StatusTitleLabel.Text = "Scanning nearby couriers";
            StatusLabel.Text = "Updating location data...";
            _ = AnimateProgressPercentage(35, 68, 3000);
            await CustomProgressBar.AnimateWidth(MaxProgressBarWidth * 0.68, 3000, Easing.CubicOut);

            // Stage 3: 68-92% (3 sec)
            StatusTitleLabel.Text = "Performing final checks";
            StatusLabel.Text = "Loading assets...";
            _ = AnimateProgressPercentage(68, 92, 3000);
            await CustomProgressBar.AnimateWidth(MaxProgressBarWidth * 0.92, 3000, Easing.Linear);

            // Stage 4: 92-100% (1 sec)
            StatusTitleLabel.Text = "System Ready";
            StatusLabel.Text = "Welcome!";
            _ = AnimateProgressPercentage(92, 100, 1000);
            await CustomProgressBar.AnimateWidth(MaxProgressBarWidth, 1000, Easing.CubicIn);

            await Task.Delay(1000);

            this.AbortAnimation("ExpressHubPulse");

            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private async Task AnimateProgressPercentage(int start, int end, uint duration)
        {
            var animation = new Animation(v => ProgressPercentageLabel.Text = $"{(int)v}%", start, end);
            animation.Commit(this, "PercentAnim", length: duration, easing: Easing.Linear);
            await Task.Delay((int)duration);
        }
    }

    public static class ViewExtensions
    {
        public static Task<bool> AnimateWidth(this VisualElement view, double targetWidth, uint duration, Easing easing)
        {
            var tcs = new TaskCompletionSource<bool>();
            var animation = new Animation(v => view.WidthRequest = v, view.WidthRequest, targetWidth);
            animation.Commit(view, "WidthAnim", length: duration, easing: easing, finished: (v, c) => tcs.SetResult(c));
            return tcs.Task;
        }
    }
}
