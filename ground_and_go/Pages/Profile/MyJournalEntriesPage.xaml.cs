// FILE: ground_and_go/Pages/Profile/MyJournalEntriesPage.xaml.cs
namespace ground_and_go.Pages.Profile;

public partial class MyJournalEntriesPage : ContentPage 
{
    public MyJournalEntriesPage()
    {
        InitializeComponent();
    } 
    
    private async void OnJournalEntryTapped(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new JournalEntryPage());
	}
}