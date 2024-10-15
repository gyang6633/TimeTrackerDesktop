namespace MauiApp3;
using MauiApp3.Components;
public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
        BindingContext = new SignUpViewModel(this);
    }
}