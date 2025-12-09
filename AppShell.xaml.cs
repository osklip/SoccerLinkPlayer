namespace SoccerLinkPlayer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rejestracja tras
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}