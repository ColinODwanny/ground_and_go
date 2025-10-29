// FILE: ground_and_go/Pages/Auth/LoginPage.xaml.cs
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.Auth;

public partial class SignupPage : ContentPage 
{
    public SignupPage()
    {
        InitializeComponent();
    }

    private async void ForgotUsernameClicked(object sender, EventArgs e)
    {
        //TODO: This takes user to a pop-up, where they can provide their email
    }
    
    private async void ForgotPasswordClicked(object sender, EventArgs e)
    {
        //TODO: This takes user to a pop-up, where they can provide their email
    }

    private async void SignupClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;
            //TODO: This will bring the user to the login page
            
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }
}