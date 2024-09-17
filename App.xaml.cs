namespace MauiApp3;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Set the WelcomePage as the starting page
        MainPage = new NavigationPage(new WelcomePage());
    }
}