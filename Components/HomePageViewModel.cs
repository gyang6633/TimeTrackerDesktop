using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MauiApp3.Components
{
    public class UserGroupsViewModel : INotifyPropertyChanged
    {
        private List<UserGroup> _userGroups;
        private readonly HttpClient _httpClient;
        private readonly Page _page;

        public UserGroupsViewModel()
        {
            // Initialize HttpClient or use dependency injection
            _httpClient = new HttpClient();
            // Automatically trigger the data fetch when the ViewModel is instantiated
            FetchUserGroupsAsync().ConfigureAwait(false);
            FetchUserGroupsCommand = new Command(async () => await FetchUserGroupsAsync());
        }

        // Constructor that accepts a Page reference
        public UserGroupsViewModel(Page page) : this()
        {
            _page = page;
        }

        public List<UserGroup> UserGroups
        {
            get => _userGroups;
            set
            {
                if (_userGroups != value)
                {
                    _userGroups = value;
                    OnPropertyChanged(nameof(UserGroups));
                }
            }
        }

        public ICommand FetchUserGroupsCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    private async Task FetchUserGroupsAsync()
    {
        var url = "http://localhost:5264/api/user/groups";

        try
        {
            var responseString = await _httpClient.GetStringAsync(url);
            // Log or display the raw response
            //await Application.Current.MainPage.DisplayAlert("Response", responseString, "OK");

            // Now attempt to deserialize the response
            var jsonDocument = JsonDocument.Parse(responseString);
            var response = jsonDocument.RootElement.GetProperty("0").Deserialize<List<UserGroup>>();

            if (response != null)
            {
                UserGroups = response;
                string displayMessage = string.Join("\n", response.Select(group => $"ID: {group.id}, NetID: {group.netID}, Role: {group.role}, Group: {group.group}, First Name: {group.firstName}, Last Name: {group.lastName}"));
                await Application.Current.MainPage.DisplayAlert("Response", displayMessage, "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No data found.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Define a UserGroup model to match the JSON data structure
    public class UserGroup
    {
        public int id { get; set; }
        public string netID { get; set; }
        // Add other properties as per your JSON data structure
        public string role { get; set; }
        public int group { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}
}