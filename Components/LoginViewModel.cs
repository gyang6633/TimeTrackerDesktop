using System.Net.Http.Json;
using System.Windows.Input;

namespace MauiApp3.Components
{
    public class LoginViewModel : BindableObject
    {
        private string _netId;
        private string _password;
        private readonly HttpClient _httpClient;
        private readonly Page _page;

        public string NetID
        {
            get => _netId;
            set
            {
                _netId = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }

        // Add a public parameterless constructor for XAML
        public LoginViewModel()
        {
            _httpClient = new HttpClient();
            LoginCommand = new Command(async () => await Login());
        }

        // Constructor that accepts a Page reference
        public LoginViewModel(Page page) : this()  // Reuse the parameterless constructor
        {
            _page = page;
        }

        private async Task Login()
        {
            try
            {
                var loginRequest = new Dictionary<string, string>
                {
                    { "NetID", _netId },
                    { "Password", _password }
                };

                var url = "http://localhost:5264/api/auth/login";
                var content = new FormUrlEncodedContent(loginRequest);

                var response = await _httpClient.PostAsync(url, content);  // Use PostAsync for form data

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result.requiresPasswordChange)
                    {
                        //await _page.DisplayAlert("Login", "Password change required", "OK");

                        // Navigate to SignUpPage when password change is required
                        await _page.Navigation.PushAsync(new SignUpPage());
                    }
                    else
                    {
                        await _page.DisplayAlert("Login", "Login successful", "OK");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await _page.DisplayAlert("Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                await _page.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    public class LoginResponse
    {
        public string message { get; set; }
        public bool requiresPasswordChange { get; set; }
    }
}
