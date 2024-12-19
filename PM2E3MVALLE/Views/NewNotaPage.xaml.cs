using Firebase.Database;
using Firebase.Storage;
using Firebase.Database.Query;
using PM2E3MVALLE.Models;
using System.Globalization;
using Plugin.Maui.Audio;
namespace PM2E3MVALLE.Views;

using CommunityToolkit.Maui.Views;
using Plugin.AudioRecorder;

public partial class NewNotaPage : ContentPage
{
    FirebaseClient client = new FirebaseClient("https://examenluisc-default-rtdb.firebaseio.com/");
    //control para audio
  ///  private readonly IAudioRecorder _audioRecorder;
    private AudioRecorderService audioRecorderService;
    private bool isRecording = false;
    private byte[] audioBytes;
    //
    private string UrlFoto { get; set; }
    private string UrlAudio { get; set; }
    private string filename;

    public NewNotaPage()
    {
        InitializeComponent();
        BindingContext = this;
        audioRecorderService = new AudioRecorderService();
///        _audioRecorder = AudioManager.Current.CreateRecorder();
    }

    private async void guardarButton_Clicked(object sender, EventArgs e)
    {
        //Validaciones importantes
        if (string.IsNullOrEmpty(UrlFoto))
        {
            await DisplayAlert("Alerta", "Por favor grabe una fotografia de la nota", "OK");
            return;
        }

        if (string.IsNullOrEmpty(UrlAudio))
        {
            await DisplayAlert("Alerta", "falta audio para la nota", "OK");
            return;
        }

        if (string.IsNullOrEmpty(descripcionEntry.Text))
        {
            await DisplayAlert("Alerta", "Por favor ingrese un título para el lugar", "OK");
            return;
        }
        // Crear nota

        DateTime fechaIngresada = fechaEntry.Date;

        // Convierte la fecha a string con el formato deseado
        string fechaString = fechaIngresada.ToString("dd/MM/yyyy HH:mm:ss");
        var nuevaNota = new Nota
        {
            Descripcion = descripcionEntry.Text,
            Fecha = fechaString,
            Foto = UrlFoto,
            Audio = UrlAudio
        };

        // guardar nota en Firebase
        await client.Child("Notas").PostAsync(nuevaNota);

        // Notify the main page about the new nota
        MessagingCenter.Send(this, "NotaAgregada");

        // Navigate back to the previous page
        await Shell.Current.GoToAsync("..");

    }

    private async void seleccionarButton_Clicked(object sender, EventArgs e)
    {
        FileResult foto = null;
        bool confirm = await Shell.Current.DisplayAlert("Confirmar", "Usar Cámara del dispositivo?, sino, seleccionara imágen", "Sí", "No");
        if (confirm)
        {
            foto = await MediaPicker.Default.CapturePhotoAsync();
        }
        else 
        {
            foto = await MediaPicker.PickPhotoAsync();
        }

        
        if (foto != null)
        {
            var stream = await foto.OpenReadAsync();
            UrlFoto = await new FirebaseStorage("examenluisc.appspot.com")
                            .Child("Photos")
                            .Child(DateTime.Now.ToString("ddMMyyhhmmss") + foto.FileName)
                            .PutAsync(stream);
            fotoImage.Source = UrlFoto;
        }
    }

    private async void btnGrabarAudio_Pressed(object sender, EventArgs e)
    {
        if (!isRecording)
        {
            await StartRecordingAsync();
            isRecording = true;
        }
        else
        {
            await StopRecordingAsync();
            isRecording = false;

        }

    }

    private async Task StartRecordingAsync()
    {
        try
        {
            // Solicita permiso para usar el micrófono
            var status2 = await Permissions.RequestAsync<Permissions.Microphone>();

            // Verifica si el permiso fue otorgado
            if (status2 != PermissionStatus.Granted)
            {
                Console.WriteLine("Permisos no concedidos.");
                return;
            }

            // Inicia la grabación de audio
            var audioRecordTask = await audioRecorderService.StartRecording();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error iniciando grabación..: {ex.Message}");
        }
    }

    /*  private async Task StopRecordingAsync()
      {
          try
          {
              await audioRecorderService.StopRecording();
              string audioFilePath = audioRecorderService.GetAudioFilePath();
              audioBytes = File.ReadAllBytes(audioFilePath);
              filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DateTime.Now.ToString("ddMMyyyymmss") + "_VoiceNote.wav");
              File.WriteAllBytes(filename, audioBytes);

              using (var fileStorage = new FileStream(filename, FileMode.Open, FileAccess.Read))
              {
                  UrlAudio = await new FirebaseStorage("examenluisc.appspot.com") //CAMBIE ESTO !!!!!!!! SUAZO
                      .Child("Audios")
                      .Child($"{Path.GetFileName(filename)}")
                      //                            .Child($"{DateTime.Now:ddMMyyHHmmssfff}_{Path.GetFileName(filename)}")
                      .PutAsync(fileStorage);
              }
              mediaElementAudio.Source = MediaSource.FromFile(filename);

          }
          catch (Exception ex)
          {
              // Maneja cualquier excepción que ocurra
              await DisplayAlert("Error", $"Error al detener la grabación: {ex.Message}", "OK");
          }
      }*/

    private async Task StopRecordingAsync()
    {
        try
        {
            await audioRecorderService.StopRecording();

            // Obtener la ruta del archivo grabado
            string audioFilePath = audioRecorderService.GetAudioFilePath();
            if (string.IsNullOrEmpty(audioFilePath))
            {
                await DisplayAlert("Error", "La ruta del archivo de audio no es válida.", "OK");
                return;
            }

            // Leer los bytes del archivo grabado
            audioBytes = File.ReadAllBytes(audioFilePath);

            // Crear un nombre válido para el archivo
            filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        DateTime.Now.ToString("ddMMyyyyHHmmss") + "_VoiceNote.wav");

            // Guardar los bytes en el archivo
            File.WriteAllBytes(filename, audioBytes);

            // Subir el archivo a Firebase Storage
            if (!File.Exists(filename))
            {
                await DisplayAlert("Error", "El archivo no existe para ser subido.", "OK");
                return;
            }

            using (var fileStorage = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                UrlAudio = await new FirebaseStorage("examenluisc.appspot.com")
                    .Child("Audios")
                    .Child($"{Path.GetFileName(filename)}")
                    .PutAsync(fileStorage);
            }

            // Asignar la fuente del archivo grabado
            mediaElementAudio.Source = MediaSource.FromFile(filename);
        }
        catch (Exception ex)
        {
            // Maneja cualquier excepción que ocurra
            await DisplayAlert("Error", $"Error al detener la grabación: {ex.Message}", "OK");
        }
    }
}