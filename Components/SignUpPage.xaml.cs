// Written by Grace and Eric
namespace MauiApp3;
using MauiApp3.Components;

// Create UI page for setting password for first time users
public partial class SignUpPage : ContentPage
{
    //initialize xaml components (UI)
    //set the BindingContext to bind data from API responses in MVVM pattern, connecting UI with SignUpPage
    public SignUpPage()
    {
        InitializeComponent();
        BindingContext = new SignUpViewModel(this);
    }
}