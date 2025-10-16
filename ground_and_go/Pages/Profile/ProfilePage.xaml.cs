//Aidan Trusky
using System.Net.Security;

namespace ground_and_go.Pages.Profile;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}

	private async void OnMyWorkoutsTapped(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new MyWorkoutsPage());
	}

	private async void OnMyJournalEntriesTapped(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new MyJournalEntriesPage());
	}

	private async void OnLogoutTapped(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Auth.LoginPage());
	}
	

}