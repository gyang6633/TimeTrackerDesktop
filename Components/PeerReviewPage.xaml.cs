
using Microsoft.Maui.Controls;
using System;
using MauiApp3.Components;
namespace MauiApp3
{
    public partial class PeerReviewPage : ContentPage{
        private readonly PeerReviewPageViewModel _viewModel;
        public PeerReviewPage()
        {
            InitializeComponent();
            // Initialize ViewModel and bind it to the page
            _viewModel = new PeerReviewPageViewModel(this);
            BindingContext = _viewModel;
        }
    }
}
