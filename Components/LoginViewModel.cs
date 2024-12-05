// Written by Grace and Eric 
// This is the ViewModel for the Login Screen 
using System.Net.Http.Json; // Provides extension methods for HTTP content deserialization
using System.Windows.Input; // Allows command binding in the ViewModel

namespace MauiApp3.Components
{
    public class LoginViewModel : BindableObject
    {
        private string _netId; // Stores the user's NetID
        private string _password; // Stores the user's password
        private readonly HttpClient _httpClient; // Handles HTTP requests
        private readonly Page _page; 

        // Property for the user's NetID with change notification
        public string NetID
        {
            get => _netId;
            set
            {
                _netId = value;
                OnPropertyChanged(); // Notify the UI of property changes
            }
        }

        // Property for the user's password with change notification
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(); // Notify the UI of property changes
            }
        }

        // Command that triggers the Login method when executed
        public ICommand LoginCommand { get; }

        // Default constructor for XAML binding
        public LoginViewModel()
        {
            _httpClient = new HttpClient(); // Initialize the HttpClient for API calls
            LoginCommand = new Command(async () => await Login()); // Bind the LoginCommand to the Login method
        }

        // Constructor that accepts a Page reference, used for navigation and alerts
        public LoginViewModel(Page page) : this()  // Calls the default constructor
        {
            _page = page; // Assigns the page reference
        }

        // Method to handle the login logic
        private async Task Login()
        {
            try
            {
                // Create a dictionary containing login credentials
                var loginRequest = new Dictionary<string, string>
                {
                    { "NetID", _netId }, // NetID entered by the user
                    { "Password", _password } // Password entered by the user
                };

                // API endpoint for login
                var url = "http://localhost:5264/api/auth/login";
                var content = new FormUrlEncodedContent(loginRequest); // Encode login data 

                // Send a POST request with the login credentials
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode) // Check if the response indicates success
                {
                    // Deserialize the response content into a LoginResponse object
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (result.requiresPasswordChange) // Check if a password change is required
                    {
                        // Navigate to the SignUpPage if the user needs to change their password
                        await _page.Navigation.PushAsync(new SignUpPage());
                    }
                    else
                    {
                        // Navigate to the HomePage if login is successful
                        await _page.Navigation.PushAsync(new HomePage());
                    }
                }
                else
                {
                    // If the response indicates an error, display the error message
                    var error = await response.Content.ReadAsStringAsync();
                    await _page.DisplayAlert("Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and display the error message
                await _page.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    // Response from the login API
    public class LoginResponse
    {
        public string message { get; set; } // Message returned from the API
        public bool requiresPasswordChange { get; set; } // Indicates if the user needs to change their password
    }
}
