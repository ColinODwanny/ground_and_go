using CommunityToolkit.Maui.Views;
using ground_and_go.Models;
using System.Linq;

namespace ground_and_go.Pages.Auth;

public partial class ForgotPasswordPopup : Popup
{
    readonly BusinessLogic businessLogic = MauiProgram.BusinessLogic;
    
    public ForgotPasswordPopup()
    {
        InitializeComponent();
    }

    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private async void OnSubmit_Clicked(object sender, EventArgs e)
    {
        string? result = await businessLogic.ForgotPassword(UsernameENT.Text);
        if (result != null) //An error occurred
        {
            await Shell.Current.DisplayAlert("Error", result, "OK");
            return;
        }
        Close("Success");
    }
}