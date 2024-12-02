using Microsoft.Maui.Controls;

namespace MauiApp3
{
    public partial class CreatePRQPage : ContentPage
    {
        public CreatePRQPage()
        {
            InitializeComponent();
            BindingContext = new CreatePRQPageViewModel();
        }
    }
}