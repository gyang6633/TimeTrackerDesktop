using Microsoft.Maui.Controls;

namespace MauiApp3
{
    //create UI page for adding and viewing Peer Review Questions
    public partial class CreatePRQPage : ContentPage
    {
        //initialize xaml components (UI)
        //set the BindingContext to bind data from API responses in MVVM pattern, connecting UI with CreatePRQPageViewModel
        public CreatePRQPage()
        {
            InitializeComponent();
            BindingContext = new CreatePRQPageViewModel();
        }
    }
}