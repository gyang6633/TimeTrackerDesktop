
using Microsoft.Maui.Controls;
using System;
using MauiApp3.Components;
namespace MauiApp3
{
    //defines UI page bound to XAML
    public partial class PeerReviewPage : ContentPage{
        //bind viewModel in MVVM pattern
        private readonly PeerReviewPageViewModel _viewModel;
        //constructor for homepage
        public PeerReviewPage()
        {
            InitializeComponent();
            //initialize ViewModel and bind it to the page
            _viewModel = new PeerReviewPageViewModel(this);
            BindingContext = _viewModel;
        }
    }
}
