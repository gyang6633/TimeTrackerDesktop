// Written by Eric and Grace
namespace MauiApp3;
using MauiApp3.Components;
public partial class LogInPage : ContentPage
{
    public LogInPage()
    {
        InitializeComponent();  // This calls the generated code from the XAML file
        BindingContext = new LoginViewModel(this); // Pass the page itself
    }
     // The correct event handler signature for TapGestureRecognizer
    private async void OnLoginLabelTapped(object sender, EventArgs e)
    {
        // Navigate to the SignUpPage when the label is tapped
        await Navigation.PushAsync(new SignUpPage());
    }
}