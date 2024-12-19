using Firebase.Database;
using Firebase.Database.Query;
using PM2E3MVALLE.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PM2E3MVALLE.Views;

namespace PM2E3MVALLE.ViewModels
{
    public class MainPageViewModel : BindableObject
    {
        // Cliente de Firebase para interactuar con la base de datos
        private FirebaseClient client = new FirebaseClient("https://examenluisc-default-rtdb.firebaseio.com/");
        private ObservableCollection<Nota> _notasOriginales;
        private string _filtro;
        // Colección observable para almacenar las notas
        public ObservableCollection<Nota> Lista { get; set; } = new ObservableCollection<Nota>();

        public string Filtro
        {
            get => _filtro;
            set
            {
                if (_filtro != value)
                {
                    _filtro = value;
                    OnPropertyChanged();
                    FiltrarLista();
                }
            }
        }

        // Comandos para nuevas notas, editar y eliminar
        public ICommand NuevoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public MainPageViewModel()
        {
            // Inicialización de los comandos
            NuevoCommand = new Command(async () => await OnNuevoCommand());
            EditarCommand = new Command<Nota>(async (nota) => await OnEditarCommand(nota));
            EliminarCommand = new Command<string>(async (id) => await OnEliminarCommand(id));

            // Suscribirse a los mensajes de actualización
            MessagingCenter.Subscribe<NewNotaPage>(this, "NotaAgregada", (sender) => CargarLista());
            MessagingCenter.Subscribe<EditNotaViewModel>(this, "NotaActualizada", (sender) => CargarLista());

            // Cargar la lista inicial de notas
            CargarLista();
        }

        // Método para cargar la lista de notas desde Firebase
        private async void CargarLista()
        {
            var notas = await client
                .Child("Notas")
                .OnceAsync<Nota>();

            // Ordenar las notas por fecha descendente y almacenarlas en _notasOriginales
            _notasOriginales = new ObservableCollection<Nota>(
                    notas.Select(p => p.Object)
                    .OrderByDescending(nota => nota.Fecha)
            );
            Lista.Clear();

            // Añadir las notas ordenadas a la lista observable
            foreach (var nota in _notasOriginales)
            {
                Lista.Add(nota);
            }
        }

        // Método para filtrar la lista de notas basado en el filtro ingresado
        private void FiltrarLista()
        {
            if (string.IsNullOrWhiteSpace(Filtro))
            {
                Lista.Clear();
                foreach (var nota in _notasOriginales)
                {
                    Lista.Add(nota);
                }
            }
            else
            {
                var filtro = Filtro.ToLower();
                var filteredList = _notasOriginales
                    .Where(x => x.Descripcion.ToLower().Contains(filtro)).ToList()
                    .OrderBy(x => x.Fecha);
                Lista.Clear();
                foreach (var item in filteredList)
                {
                    Lista.Add(item);
                }
            }
        }

        // Método para navegar a la página de nueva nota
        private async Task OnNuevoCommand()
        {
            await Shell.Current.GoToAsync(nameof(NewNotaPage));
        }

        // Método para navegar a la página de edición de nota con la descripción de la nota
        private async Task OnEditarCommand(Nota nota)
        {
            await Shell.Current.GoToAsync($"{nameof(EditNotaPage)}?name={nota.Descripcion}");
        }

        // Método para eliminar una nota por su ID
        private async Task OnEliminarCommand(string idNota)
        {
            bool confirm = await Shell.Current.DisplayAlert("Confirmar", "¿Estás seguro de que deseas eliminar esta nota?", "Sí", "No");
            if (confirm)
            {
                var notaParaEliminar = _notasOriginales.FirstOrDefault(p => p.Id == idNota);
                if (notaParaEliminar != null)
                {
                    var notas = await client
                        .Child("Notas")
                        .OnceAsync<Nota>();

                    var notaFirebase = notas.FirstOrDefault(p => p.Object.Id == idNota);
                    if (notaFirebase != null)
                    {
                        await client
                            .Child("Notas")
                            .Child(notaFirebase.Key)
                            .DeleteAsync();

                        // Remover la nota eliminada de las listas
                        Lista.Remove(notaParaEliminar);
                        _notasOriginales.Remove(notaParaEliminar);
                    }
                }
            }
        }
    }
}
