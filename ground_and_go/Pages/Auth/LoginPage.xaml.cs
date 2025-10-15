// FILE: ground_and_go/Pages/Auth/LoginPage.xaml.cs
namespace ground_and_go.Pages.Auth;

public partial class LoginPage : ContentPage 
{
    public LoginPage()
    {
        InitializeComponent();
    } 
    
    private async void LoginClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;

            //var username = button.
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }
}