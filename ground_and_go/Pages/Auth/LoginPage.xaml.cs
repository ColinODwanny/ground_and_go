// FILE: ground_and_go/Pages/Auth/LoginPage.xaml.cs
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using ground_and_go.Models;
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.Auth;

public partial class LoginPage : ContentPage
{
    readonly BusinessLogic businessLogic = MauiProgram.BusinessLogic;
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void CreateAccountClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;
            await Shell.Current.GoToAsync("//signup");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }
    
    private async void ForgotPasswordClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;
            var popup = new ForgotPasswordPopup();
            object? result = await this.ShowPopupAsync(popup);

            if (result != null)
            {
                await DisplayAlert("Success", "If an account exists, a recovery email will be sent.", "OK");
                //TODO Create new page for changing password for current account, if result == null
            }
        } catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void LoginClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;

            //Returns null if successful and an error message otherwise
            String? result = await businessLogic.LogIn(UsernameENT.Text, PasswordENT.Text);
            if (result == null)
            {
                await Shell.Current.GoToAsync("//home");
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
}