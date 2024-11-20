using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
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
            _httpClient = new HttpClient();
            FetchUserGroupsCommand = new Command(async () => await FetchUserGroupsAsync());
            ToggleExpandCommand = new Command<UserGroup>(ToggleExpand);
            FetchUserGroupsAsync().ConfigureAwait(false);
        }

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
                    OnPropertyChanged(nameof(StudentGroups));
                }
            }
        }

        public List<UserGroup> StudentGroups =>
            _userGroups?.FindAll(group => string.Equals(group.role, "student", StringComparison.OrdinalIgnoreCase)) ?? new List<UserGroup>();

        public ICommand FetchUserGroupsCommand { get; }
        public ICommand ToggleExpandCommand { get; }

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

                foreach (var groupProperty in jsonDocument.RootElement.EnumerateObject())
                {
                    var userGroupList = JsonSerializer.Deserialize<List<UserGroup>>(groupProperty.Value.GetRawText());
                    if (userGroupList != null)
                    {
                        allUserGroups.AddRange(userGroupList);
                    }
                }

                UserGroups = allUserGroups;

                foreach (var userGroup in UserGroups)
                {
                    int totalWeeklyMinutes = 0;
                    foreach (var timeLog in userGroup.timeLogs)
                    {
                        foreach (var entry in timeLog.timeLogEntries)
                        {
                            totalWeeklyMinutes += entry.duration;
                        }
                    }

                    userGroup.WeeklyCumulativeHours = totalWeeklyMinutes;
                    userGroup.WeeklyCumulativeHoursFormatted = $"{totalWeeklyMinutes / 60:D2}:{totalWeeklyMinutes % 60:D2}";
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ToggleExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsExpanded = !userGroup.IsExpanded;
            }
        }

        public class UserGroup : INotifyPropertyChanged
        {
            public int id { get; set; }
            public string netID { get; set; }
            public string role { get; set; }
            public int group { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public List<TimeLog> timeLogs { get; set; }
            public int WeeklyCumulativeHours { get; set; }
            public string WeeklyCumulativeHoursFormatted { get; set; }

            private bool _isExpanded;
            public bool IsExpanded
            {
                get => _isExpanded;
                set
                {
                    if (_isExpanded != value)
                    {
                        _isExpanded = value;
                        OnPropertyChanged(nameof(IsExpanded));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
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
