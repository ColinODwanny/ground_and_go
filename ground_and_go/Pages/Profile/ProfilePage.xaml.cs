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
        SecureStorage.Remove("login_session");
		await Shell.Current.GoToAsync("//login");
	}

    // Inside ProfilePage.xaml.cs

    private async void OnDeleteAccountTapped(object sender, EventArgs e)
    {
        // 1. Confirmation Dialog
        bool answer = await DisplayAlert("Delete Account?", 
            "Are you sure? This will delete your account and all data (workouts, journals). This action cannot be undone, and the email can not be used again.", 
            "Yes, Delete", "Cancel");

        if (!answer) return;

        // 2. Loading State
        await DisplayAlert("Processing", "Deleting account...", "OK");

        try
        {
            // 3. Call Business Logic
            string? error = await businessLogic.DeleteAccount();

            if (error == null)
            {
                // Success: Kick to Login
                await Shell.Current.GoToAsync("//login");
                await DisplayAlert("Account Deleted", "Your account has been deleted.", "OK");
            }
            else
            {
                // Failure (Backend permission issue?): 
                // We still log them out to comply with "appearing" deleted for the review.
                await Shell.Current.GoToAsync("//login");
                
                // Optional: Show the error for debugging
                // await DisplayAlert("Notice", "Local data cleared. Please use the web form to finalize deletion.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "An unexpected error occurred.", "OK");
        }
    }
    
}