// FILE: ground_and_go/Pages/Auth/SignupPage.xaml.cs
using ground_and_go.Models;
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.Auth;

public partial class SignupPage : ContentPage
{
    readonly BusinessLogic businessLogic = MauiProgram.BusinessLogic;
    public SignupPage()
    {
        InitializeComponent();
    }

    private async void SignupClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;
            String? result = await businessLogic.SignUp(UsernameENT.Text, PasswordENT.Text, RepeatPasswordENT.Text);
            if (result == null)
            {
                await DisplayAlert("Success", "An account was created for the given email", "OK");
                await Shell.Current.GoToAsync("//login");
            }
            else
            {
                await DisplayAlert("Error", result, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void LoginClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }
}