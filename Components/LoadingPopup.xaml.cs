using CommunityToolkit.Maui.Views;

namespace ground_and_go.Components
{
    public partial class LoadingPopup : Popup
    {
        public LoadingPopup(string message = "Loading...")
        {
            InitializeComponent();
            LoadingLabel.Text = message;
        }
        
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            
            // Start the loading animation when popup is shown
            if (Handler != null)
            {
                LoadingIndicator.IsRunning = true;
            }
        }
    }
}