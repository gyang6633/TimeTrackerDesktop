using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MauiApp3.Components
{
    public class SignUpViewModel : INotifyPropertyChanged
    {
        private string _email;
        private string _password;
        private string _confirmPassword;
        private readonly HttpClient _httpClient;
        private readonly Page _page;

        public SignUpViewModel()
        {
            // Initialize HttpClient or use dependency injection
            _httpClient = new HttpClient();
            SignUpCommand = new Command(async () => await SignUpAsync());
        }
          // Constructor that accepts a Page reference
        public SignUpViewModel(Page page) : this()  // Reuse the parameterless constructor
        {
            _page = page;
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged(nameof(ConfirmPassword));
                }
            }
        }

        public ICommand SignUpCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task SignUpAsync()
        {
            // Perform basic validation
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // Call backend API to change password
            var url = "http://localhost:5264/api/auth/update-password";
            var updatePasswordRequest = new Dictionary<string, string>
            {
                { "Password", Password }
            };

            try
            {
                var response = await _httpClient.PatchAsync(url, new FormUrlEncodedContent(updatePasswordRequest));

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Password updated successfully!", "OK");
                    // Navigate to login page
                    await _page.Navigation.PushAsync(new LogInPage());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}