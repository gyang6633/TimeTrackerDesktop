namespace MauiApp3{ // Ensure the namespace matches the one in XAML

    public partial class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        // This is the event handler method that handles the Clicked event for the Sign In button
        private async void OnSignInClicked(object sender, EventArgs e)
        {
            // Add logic for Sign In button click here, for example, navigate to the SignInPage
            await Navigation.PushAsync(new SignUpPage());
        }

        private async void OnLogInClicked(object sender, EventArgs e)
        {
            // Add logic for Log In button click here, for example, navigate to the LogInPage
            await Navigation.PushAsync(new LogInPage());
        }
    }
}