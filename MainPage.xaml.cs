namespace SoccerLinkPlayer;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserData();
    }

    private void LoadUserData()
    {
        string userName = Preferences.Get("LoggedUserName", "Zawodniku");
        WelcomeLabel.Text = $"Cześć, {userName}!";
    }
}