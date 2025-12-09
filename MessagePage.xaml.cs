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
        // Pobierz ID zalogowanego u¿ytkownika
        int playerId = Preferences.Get("LoggedUserId", 0);

        if (playerId == 0)
        {
            await DisplayAlert("B³¹d", "Nie znaleziono zalogowanego u¿ytkownika.", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        MessagesCollectionView.IsVisible = false;

        try
        {
            var messages = await _databaseService.GetMessagesForPlayerAsync(playerId);
            MessagesCollectionView.ItemsSource = messages;
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", "Nie uda³o siê pobraæ wiadomoœci.", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            MessagesCollectionView.IsVisible = true;
        }
    }
}