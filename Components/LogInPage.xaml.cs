namespace MauiApp3;
public partial class LogInPage : ContentPage
{
    public LogInPage()
    {
        InitializeComponent();  // This calls the generated code from the XAML file
    }
     // The correct event handler signature for TapGestureRecognizer
    private async void OnLoginLabelTapped(object sender, EventArgs e)
    {
        // Navigate to the SignUpPage when the label is tapped
        await Navigation.PushAsync(new SignUpPage());
    }
}