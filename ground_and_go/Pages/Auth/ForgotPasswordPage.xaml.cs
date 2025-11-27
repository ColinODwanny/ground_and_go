// FILE: ground_and_go/Pages/Auth/ForgotPasswordPage.xaml.cs
using ground_and_go.Models;
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    readonly BusinessLogic businessLogic = MauiProgram.BusinessLogic;
    private readonly Database _database;
    public ForgotPasswordPage()
    {
        InitializeComponent();
        _database = IPlatformApplication.Current.Services.GetRequiredService<Database>();
    }

    private async void ChangePasswordClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;

            String? result = await businessLogic.ChangePassword(PasswordENT.Text, RepeatPasswordENT.Text);
            if (result == null)
            {
                await DisplayAlert("Success", "Your password has been successfully changed.", "OK");
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