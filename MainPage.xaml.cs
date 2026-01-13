using SoccerLinkPlayer.Services;
using SoccerLinkPlayer.Models;

namespace SoccerLinkPlayer
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MainPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LoadUserData();
            await LoadNextMatch();
        }

        private void LoadUserData()
        {
            string userName = Preferences.Get("LoggedUserName", "Zawodniku");
            WelcomeLabel.Text = $"Cześć, {userName}!";
        }

        private async Task LoadNextMatch()
        {
            int coachId = Preferences.Get("LoggedUserCoachId", 0);
            System.Diagnostics.Debug.WriteLine($"[DEBUG MAINPAGE] Odczytane ID Trenera z Preferences: {coachId}");

            if (coachId == 0)
            {
                MatchDateLabel.Text = "Błąd: Brak przypisanego trenera (ID=0)";
                return;
            }

            try
            {
                var mecz = await _databaseService.GetNextMatchAsync(coachId);

                if (mecz != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG MAINPAGE] Wyświetlam mecz: {mecz.Przeciwnik}");
                    AwayTeamLabel.Text = mecz.Przeciwnik;
                    MatchDateLabel.Text = $"{mecz.Data}, {mecz.GodzinaRozpoczecia}";
                    MatchLocationLabel.Text = mecz.Miejsce;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG MAINPAGE] Serwis zwrócił null (brak meczów)");
                    AwayTeamLabel.Text = "---";
                    MatchDateLabel.Text = "Brak zaplanowanych meczów";
                    MatchLocationLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG MAINPAGE BŁĄD]: {ex.Message}");
                MatchDateLabel.Text = "Błąd połączenia";
            }
        }

        private async void OnMessagesTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(MessagesPage));
        }

        private async void OnCalendarTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CalendarPage));
        }
    }
}