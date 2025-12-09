using System.ComponentModel;
using System.Runtime.CompilerServices;
//using CommunityToolkit.Mvvm.ComponentModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


// Create a base class that combines BaseModel with INotifyPropertyChanged
namespace ground_and_go.Models
{
    public abstract class ObservableBaseModel : BaseModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}