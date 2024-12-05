// Written by Grace and Eric
// This is the ViewModel for the sign-up screen which allows a user to sign in and update their password.

using System.ComponentModel; // Provides INotifyPropertyChanged for property change notifications
using System.Net.Http;
using System.Net.Http.Json; 
using System.Threading.Tasks; 
using System.Windows.Input; // Provides ICommand interface for binding commands
using Microsoft.Maui.Controls;

namespace MauiApp3.Components
{
    // ViewModel for the sign-up screen implementing property change notifications
    public class SignUpViewModel : INotifyPropertyChanged
    {
        // Private fields to store email, password, and confirm password
        private string _email;
        private string _password;
        private string _confirmPassword;

        // HttpClient for making HTTP requests
        private readonly HttpClient _httpClient;

        // Page reference for navigation and displaying alerts
        private readonly Page _page;

        // Default constructor
        public SignUpViewModel()
        {
            _httpClient = new HttpClient();

            // Command to handle sign-up functionality
            SignUpCommand = new Command(async () => await SignUpAsync());
        }

        // Constructor that accepts a Page reference for navigation and displaying alerts
        public SignUpViewModel(Page page) : this()  
        {
            _page = page;
        }

        // Email property with change notification
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email)); // Notify UI of property change
                }
            }
        }

        // Password property with change notification
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password)); // Notify UI of property change
                }
            }
        }

        // ConfirmPassword property with change notification
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged(nameof(ConfirmPassword)); // Notify UI of property change
                }
            }
        }

        // Command for the sign-up action
        public ICommand SignUpCommand { get; }

        // Event for notifying when a property value changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise the PropertyChanged event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Method to handle the sign-up logic
        private async Task SignUpAsync()
        {
            // Validate that all fields are filled
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            // Check if passwords match
            if (Password != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // URL of the backend API to update the password
            var url = "http://localhost:5264/api/auth/update-password";

            // Prepare the request payload
            var updatePasswordRequest = new Dictionary<string, string>
            {
                { "Password", Password }
            };

            try
            {
                // Send PATCH request to the API
                var response = await _httpClient.PatchAsync(url, new FormUrlEncodedContent(updatePasswordRequest));

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Password updated successfully!", "OK");

                    // Navigate to the login page
                    await _page.Navigation.PushAsync(new LogInPage());
                }
                else
                {
                    // Display the error message from the response
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions and display an error message
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
