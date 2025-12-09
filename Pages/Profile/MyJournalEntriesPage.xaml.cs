// FILE: ground_and_go/Pages/Profile/MyJournalEntriesPage.xaml.cs
using ground_and_go.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ground_and_go.Pages.Profile;

public partial class MyJournalEntriesPage : ContentPage, INotifyPropertyChanged 
{
    private readonly Database _database;
    private bool _isLoading = false;
    
    public ObservableCollection<WorkoutLogViewModel> JournalEntries { get; set; } = new();
    
    public bool IsLoading 
    { 
        get => _isLoading; 
        set 
        { 
            _isLoading = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(IsNotLoading));
        } 
    }
    
    public bool IsNotLoading => !IsLoading;

    public MyJournalEntriesPage(Database database)
    {
        InitializeComponent();
        _database = database;
        BindingContext = this;
        LoadJournalEntries();
    }

    private async void LoadJournalEntries()
    {
        try
        {
            IsLoading = true;
            
            // Get the real logged-in user's ID
            string? memberId = _database.GetAuthenticatedMemberId();
            
            // Pass the memberId to the query
            var workoutLogsWithDetails = await _database.GetWorkoutLogsWithDetails(memberId); 
            
            Console.WriteLine($"Total workout logs retrieved: {workoutLogsWithDetails.Count}");
            
            JournalEntries.Clear();
            foreach (var entry in workoutLogsWithDetails)
            {
                Console.WriteLine($"Entry {entry.WorkoutLog.LogId}: HasBefore={entry.HasBeforeJournal}, HasAfter={entry.HasAfterJournal}");
                Console.WriteLine($"Before: '{entry.WorkoutLog.BeforeJournal?.Substring(0, Math.Min(30, entry.WorkoutLog.BeforeJournal?.Length ?? 0))}...'");
                Console.WriteLine($"After: '{entry.WorkoutLog.AfterJournal?.Substring(0, Math.Min(30, entry.WorkoutLog.AfterJournal?.Length ?? 0))}...'");
                
                if (entry.HasBeforeJournal || entry.HasAfterJournal)
                {
                    JournalEntries.Add(entry);
                    Console.WriteLine($"Added entry {entry.WorkoutLog.LogId} to collection");
                }
            }
            
            Console.WriteLine($"Final journal entries count: {JournalEntries.Count}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load journal entries: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    // This is the corrected section
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    // End corrected section
    
    private void OnJournalEntryTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutLogViewModel journalEntry)
        {
            journalEntry.IsExpanded = !journalEntry.IsExpanded;
        }
    }
}