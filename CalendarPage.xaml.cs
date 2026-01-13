using SoccerLinkPlayer.Services;
using SoccerLinkPlayer.Models;

namespace SoccerLinkPlayer;

public partial class CalendarPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    // Konstruktor z wstrzykiwaniem zale¿noœci
    public CalendarPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Pobieramy dane za ka¿dym razem gdy strona siê pojawia (aby mieæ œwie¿e dane)
        await LoadEvents();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadEvents();
    }

    private async Task LoadEvents()
    {
        // 1. Pobierz ID trenera przypisanego do zawodnika
        int coachId = Preferences.Get("LoggedUserCoachId", 0);

        if (coachId == 0)
        {
            // Opcjonalnie: Obs³uga sytuacji gdy nie ma ID (np. wylogowanie)
            return;
        }

        // 2. Zarz¹dzanie stanem ³adowania UI
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        EventsCollectionView.Opacity = 0.5; // Lekkie przyciemnienie listy

        try
        {
            // 3. Pobranie pe³nej, posortowanej listy (Mecze + Treningi + Wydarzenia)
            var events = await _databaseService.GetFullCalendarAsync(coachId);

            // 4. Przypisanie do widoku
            EventsCollectionView.ItemsSource = events;
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", "Nie uda³o siê pobraæ kalendarza. SprawdŸ po³¹czenie.", "OK");
            System.Diagnostics.Debug.WriteLine($"Calendar Error: {ex.Message}");
        }
        finally
        {
            // 5. Przywrócenie UI
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            EventsCollectionView.Opacity = 1;
        }
    }
}