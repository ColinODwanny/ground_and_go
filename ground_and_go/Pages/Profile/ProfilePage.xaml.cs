//Aidan Trusky
using System.Net.Security;
using ground_and_go.Models;

namespace ground_and_go.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    readonly BusinessLogic businessLogic = MauiProgram.BusinessLogic;
    public ProfilePage()
    {
        InitializeComponent();
    }

    private async void OnMyWorkoutsTapped(object sender, EventArgs e)
    {
        // use shell navigation
        await Shell.Current.GoToAsync(nameof(MyWorkoutsPage));
    }

    private async void OnMyJournalEntriesTapped(object sender, EventArgs e)
    {
        // use shell navigation
        await Shell.Current.GoToAsync(nameof(MyJournalEntriesPage));
    }

	private async void OnLogoutTapped(object sender, EventArgs e)
    {
        // navigate back to the login page
        await businessLogic.LogOut();
		await Shell.Current.GoToAsync("//login");
	}
    
}