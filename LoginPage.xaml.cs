using SoccerLinkPlayer.Services;

namespace SoccerLinkPlayer;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public LoginPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("B³¹d", "WprowadŸ e-mail i has³o.", "OK");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Logowanie...";

        try
        {
            var zawodnik = await _databaseService.LoginAsync(email, password);

            if (zawodnik != null)
            {
                // Zapisujemy dane sesji
                Preferences.Set("LoggedUserId", zawodnik.ZawodnikID);
                Preferences.Set("LoggedUserName", $"{zawodnik.Imie} {zawodnik.Nazwisko}");
                Preferences.Set("LoggedUserTeam", "FC Or³y"); // Przyk³adowo, jeœli nie ma tego w tabeli

                // Przejœcie do g³ównego ekranu
                await Shell.Current.GoToAsync($"//MainPage");
            }
            else
            {
                await DisplayAlert("B³¹d", "Nieprawid³owy e-mail lub has³o.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"Problem z po³¹czeniem: {ex.Message}", "OK");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Text = "ZALOGUJ SIÊ";
        }
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Info", "Skontaktuj siê z trenerem w celu resetu has³a.", "OK");
    }
}