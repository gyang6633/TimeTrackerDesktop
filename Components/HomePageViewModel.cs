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
                var jsonDocument = JsonDocument.Parse(responseString);

                var allUserGroups = new List<UserGroup>();

                // Iterate through each group property in the JSON
                foreach (var groupProperty in jsonDocument.RootElement.EnumerateObject())
                {
                    var userGroupList = JsonSerializer.Deserialize<List<UserGroup>>(groupProperty.Value.GetRawText());
                    if (userGroupList != null)
                    {
                        allUserGroups.AddRange(userGroupList);
                    }
                }

                // Set the UserGroups property to the aggregated list of all groups
                UserGroups = allUserGroups;

                // Calculate weekly cumulative hours for each user
                foreach (var userGroup in UserGroups)
                {
                    int totalWeeklyHours = 0;
                    foreach (var timeLog in userGroup.timeLogs)
                    {
                        foreach (var entry in timeLog.timeLogEntries)
                        {
                            totalWeeklyHours += entry.duration; // Assuming duration is in hours
                        }
                    }
                    userGroup.WeeklyCumulativeHours = totalWeeklyHours;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }




        public class UserGroup
        {
            public int id { get; set; }
            public string netID { get; set; }
            public string role { get; set; }
            public int group { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public List<TimeLog> timeLogs { get; set; }

            // Weekly cumulative hours property for binding
            public int WeeklyCumulativeHours { get; set; }
        }


        public class TimeLog
        {
            public int id { get; set; }
            public int userId { get; set; }
            public string title { get; set; }
            public List<TimeLogEntry> timeLogEntries { get; set; }
        }

        public class TimeLogEntry
        {
            public int id { get; set; }
            public int timeLogId { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
            public int duration { get; set; }
            public string description { get; set; }
        }

    }
}