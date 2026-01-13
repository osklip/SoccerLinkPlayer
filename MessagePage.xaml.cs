using SoccerLinkPlayer.Services;
using SoccerLinkPlayer.Models;

namespace SoccerLinkPlayer;

public partial class MessagesPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public MessagesPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMessages();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadMessages();
    }

    private async Task LoadMessages()
    {
        // Pobieramy ID zalogowanego gracza
        int playerId = Preferences.Get("LoggedUserId", 0);

        if (playerId == 0)
        {
            await DisplayAlert("B³¹d", "Brak zalogowanego u¿ytkownika.", "OK");
            return;
        }

        // Kontrolki ³adowania (upewnij siê, ¿e masz je w XAML: LoadingIndicator i MessagesCollectionView)
        if (LoadingIndicator != null)
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
        }

        try
        {
            // Pobranie danych z bazy
            var messages = await _databaseService.GetMessagesForPlayerAsync(playerId);

            // Przypisanie do listy
            MessagesCollectionView.ItemsSource = messages;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching messages: {ex.Message}");
            await DisplayAlert("B³¹d", "Nie uda³o siê pobraæ wiadomoœci.", "OK");
        }
        finally
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}