using Firebase.Database;
using Firebase.Storage;
using Firebase.Database.Query;
using PM2E3MVALLE.Models;
using System.Windows.Input;

namespace PM2E3MVALLE.ViewModels
{
    public class EditNotaViewModel : BindableObject
    {
        // Cliente de Firebase para interactuar con la base de datos
        private readonly FirebaseClient _client = new FirebaseClient("https://examenluisc-default-rtdb.firebaseio.com/");
        private Nota _nota;

        // Propiedad de la nota que se está editando
        public Nota Nota
        {
            get => _nota;
            set
            {
                _nota = value;
                OnPropertyChanged();
            }
        }

        // Comandos para guardar, cancelar y seleccionar
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand SeleccionarCommand { get; set; }
        private string UrlFoto { get; set; }
        private string UrlAudio { get; set; }

        public EditNotaViewModel()
        {
            // Inicialización de los comandos
            GuardarCommand = new Command(async () => await OnGuardarCommand());
            CancelarCommand = new Command(async () => await OnCancelarCommand());
            SeleccionarCommand = new Command(async () => await OnSeleccionarCommand());
        }

        // Cargar la nota desde Firebase
        public async Task LoadNotaAsync(string name)
        {
            // Obtener notas desde Firebase
            var notas = await _client.Child("Notas").OnceAsync<Nota>();
            // Encontrar la nota que coincida con la descripción
            var nota = notas.FirstOrDefault(p => p.Object.Descripcion == name);
            if (nota != null)
            {
                Nota = nota.Object;
            }
        }

        // Método para guardar la nota
        private async Task OnGuardarCommand()
        {
            try
            {
                if (Nota != null)
                {
                    var notas = await _client.Child("Notas").OnceAsync<Nota>();

                    // Verifica si la nota ya existe en Firebase
                    var notaFirebase = notas
                        .FirstOrDefault(p => p.Object.Descripcion?.Trim().Equals(Nota.Descripcion?.Trim(), StringComparison.OrdinalIgnoreCase) == true);

                    if (notaFirebase != null)
                    {
                        // Actualiza la nota existente
                        await _client.Child("Notas").Child(notaFirebase.Key).PutAsync(Nota);

                        // Enviar mensaje de notificación
                        MessagingCenter.Send(this, "NotaActualizada");

                        // Navegar hacia atrás
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Nota no encontrada en la base de datos", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "La nota es nula", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        // Método para cancelar la edición
        private async Task OnCancelarCommand()
        {
            await Shell.Current.GoToAsync(".."); // Navegar hacia atrás
        }

        // Método para seleccionar una foto
        private async Task OnSeleccionarCommand()
        {
            try
            {
                var foto = await MediaPicker.PickPhotoAsync();
                if (foto != null)
                {
                    var stream = await foto.OpenReadAsync();
                    // Subir la foto a Firebase Storage
                    UrlFoto = await new FirebaseStorage("examenluisc.appspot.com")
                                    .Child("Photos")
                                    .Child(DateTime.Now.ToString("ddMMyyhhmmss") + foto.FileName)
                                    .PutAsync(stream);

                    // Actualiza la URL de la foto en la nota
                    Nota.Foto = UrlFoto;

                    // Notificar que la propiedad nota ha cambiado
                    OnPropertyChanged(nameof(Nota));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error al seleccionar la foto: {ex.Message}", "OK");
            }
        }
    }
}
