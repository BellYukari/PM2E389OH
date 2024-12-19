using PM2E3MVALLE.Models;
using PM2E3MVALLE.ViewModels;
namespace PM2E3MVALLE.Views
{
    [QueryProperty(nameof(NoteName), "name")]
    public partial class EditNotaPage : ContentPage
    {
        private readonly EditNotaViewModel _viewModel;

        public string NoteName
        {
            get => _noteName;
            set
            {
                _noteName = value;
                OnPropertyChanged();
                LoadNota();
            }
        }

        private string _noteName;

        public EditNotaPage()
        {
            InitializeComponent();
            _viewModel = new EditNotaViewModel();
            BindingContext = _viewModel;
        }

        private async void LoadNota()
        {
            if (!string.IsNullOrEmpty(NoteName))
            {
                await _viewModel.LoadNotaAsync(NoteName);
            }
        }

        private void btnGrabarAudio_Pressed(object sender, EventArgs e)
        {

        }
    }
}