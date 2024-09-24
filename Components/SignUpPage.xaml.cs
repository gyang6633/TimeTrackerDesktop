using Microsoft.Maui.Controls;

namespace MauiApp3;
    public partial class SignUpPage : ContentPage
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        // This method is executed when the "Sign Up" button is clicked
        private async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            // Get values from the input fields (Entries)
            string email = emailEntry.Text;
            string password = passwordEntry.Text;
            string confirmPassword = confirmPasswordEntry.Text;

            // Perform basic validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // Simulate sign-up logic, such as calling an API
            // (Add your actual sign-up logic here)
            await DisplayAlert("Success", "Account created successfully!", "OK");

            // After successful sign-up, navigate back to the login page or another page
            await Navigation.PushAsync(new LogInPage());
        }

        // Navigate to the login page when the "Already have an account?" label is clicked
        private async void OnLoginLabelTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LogInPage());
        }
    }
