using PM2E3MVALLE.Views;
namespace PM2E3MVALLE
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Rutas de acceso
            Routing.RegisterRoute(nameof(NewNotaPage), typeof(NewNotaPage));
            Routing.RegisterRoute(nameof(EditNotaPage), typeof(EditNotaPage));
        }
    }
}
