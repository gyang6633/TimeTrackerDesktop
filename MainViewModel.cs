using System;
using System.ComponentModel;

namespace MauiApp3
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _data;

        public string Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        public Command GetDataCommand { get; }

        public MainViewModel()
        {
            _apiService = new ApiService();
            GetDataCommand = new Command(async () => await GetDataAsync());
        }

        private async Task GetDataAsync()
        {
            string url = "https://api.example.com/data"; // Replace with your API endpoint
            Data = await _apiService.GetDataFromApiAsync(url);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ApiService
    {
        internal async Task<string> GetDataFromApiAsync(string url)
        {
            throw new NotImplementedException();
        }
    }
}