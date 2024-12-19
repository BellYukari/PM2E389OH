using PM2E3MVALLE.ViewModels;
namespace PM2E3MVALLE.Views;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        BindingContext = new MainPageViewModel();
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {

    }

    private void OnMenuClicked(object sender, EventArgs e)
    {

    }

    private void OnEditUbicacion(object sender, EventArgs e)
    {

    }

    private void OnDeleteNota(object sender, EventArgs e)
    {

    }
}