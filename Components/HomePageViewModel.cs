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

        private DateTime _selectedWeekStartDate;

        public DateTime SelectedWeekStartDate
        {
            get => _selectedWeekStartDate;
            set
            {
                if (_selectedWeekStartDate != value)
                {
                    _selectedWeekStartDate = value;
                    OnPropertyChanged(nameof(SelectedWeekStartDate));
                    UpdateFilteredTimeLogs();
                }
            }
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
        public ICommand ToggleWeeklyExpandCommand { get; }
        public ICommand ToggleCumulativeExpandCommand { get; }

        public UserGroupsViewModel(Page page)
        {
            _page = page;
            _httpClient = new HttpClient();
            FetchUserGroupsCommand = new Command(async () => await FetchUserGroupsAsync());
            ToggleWeeklyExpandCommand = new Command<UserGroup>(ToggleWeeklyExpand);
            ToggleCumulativeExpandCommand = new Command<UserGroup>(ToggleCumulativeExpand);
            SelectedWeekStartDate = DateTime.Now; // Initialize with the current week
            FetchUserGroupsAsync().ConfigureAwait(false);
        }

        private void ToggleWeeklyExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsExpandedForWeeklyHours = !userGroup.IsExpandedForWeeklyHours;

                // Collapse the other expansion
                if (userGroup.IsExpandedForWeeklyHours)
                {
                    userGroup.IsExpandedForCumulativeHours = false;
                }

                OnPropertyChanged(nameof(UserGroups));
            }
        }

        private void ToggleCumulativeExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsExpandedForCumulativeHours = !userGroup.IsExpandedForCumulativeHours;

                // Collapse the other expansion
                if (userGroup.IsExpandedForCumulativeHours)
                {
                    userGroup.IsExpandedForWeeklyHours = false;
                }

                OnPropertyChanged(nameof(UserGroups));
            }
        }

        private void UpdateFilteredTimeLogs()
        {
            if (UserGroups == null || !UserGroups.Any())
                return;

            var startOfWeek = SelectedWeekStartDate;
            var endOfWeek = startOfWeek.AddDays(6);

            foreach (var userGroup in UserGroups)
            {
                userGroup.UpdateFilteredTimeLogs(startOfWeek, endOfWeek);
            }

            OnPropertyChanged(nameof(UserGroups));
        }


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
                    int totalCumulativeMinutes = 0;
                    foreach (var timeLog in userGroup.timeLogs)
                    {
                        int totalWeeklyMinutes = 0;
                        foreach (var entry in timeLog.timeLogEntries)
                        {
                            totalWeeklyMinutes += entry.duration;
                            totalCumulativeMinutes += entry.duration;
                        }
                        userGroup.WeeklyCumulativeHours = totalWeeklyMinutes;
                        userGroup.WeeklyCumulativeHoursFormatted = $"{totalWeeklyMinutes / 60:D2}:{totalWeeklyMinutes % 60:D2}";
                    }
                    userGroup.TotalCumulativeHours = totalCumulativeMinutes;
                    userGroup.TotalCumulativeHoursFormatted = $"{totalCumulativeMinutes / 60:D2}:{totalCumulativeMinutes % 60:D2}";
                }

                UpdateFilteredTimeLogs();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
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
        public int TotalCumulativeHours { get; set; }
        public string TotalCumulativeHoursFormatted { get; set; }

        private List<TimeLog> _filteredTimeLogs;
        public List<TimeLog> FilteredTimeLogs
        {
            get => _filteredTimeLogs;
            set
            {
                if (_filteredTimeLogs != value)
                {
                    _filteredTimeLogs = value;
                    OnPropertyChanged(nameof(FilteredTimeLogs));
                }
            }
        }

        public void UpdateFilteredTimeLogs(DateTime selectedWeekStartDate, DateTime selectedWeekEndDate)
        {
            if (timeLogs != null)
            {
                FilteredTimeLogs = timeLogs
                    .Where(log => log.timeLogEntries.Any(entry =>
                        DateTime.Parse(entry.createdAt).Date >= selectedWeekStartDate.Date &&
                        DateTime.Parse(entry.createdAt).Date <= selectedWeekEndDate.Date))
                    .Select(log => new TimeLog
                    {
                        id = log.id,
                        userId = log.userId,
                        title = log.title,
                        timeLogEntries = log.timeLogEntries
                            .Where(entry => DateTime.Parse(entry.createdAt).Date >= selectedWeekStartDate.Date &&
                                            DateTime.Parse(entry.createdAt) <= selectedWeekEndDate.Date)
                            .ToList()
                    })
                    .ToList();
            }
            else
            {
                FilteredTimeLogs = new List<TimeLog>();
            }

            OnPropertyChanged(nameof(FilteredTimeLogs));
        }





        private bool _isExpandedForWeeklyHours;
        public bool IsExpandedForWeeklyHours
        {
            get => _isExpandedForWeeklyHours;
            set
            {
                if (_isExpandedForWeeklyHours != value)
                {
                    _isExpandedForWeeklyHours = value;
                    OnPropertyChanged(nameof(IsExpandedForWeeklyHours));
                }
            }
        }

        private bool _isExpandedForCumulativeHours;
        public bool IsExpandedForCumulativeHours
        {
            get => _isExpandedForCumulativeHours;
            set
            {
                if (_isExpandedForCumulativeHours != value)
                {
                    _isExpandedForCumulativeHours = value;
                    OnPropertyChanged(nameof(IsExpandedForCumulativeHours));
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
            public int duration { get; set; }
            public string DurationFormatted => $"{duration / 60:D2}:{duration % 60:D2}";
        public string description { get; set; }
            public string createdAt { get; set; }

            public string FormattedCreatedAt
            {
                get
                {
                    if (DateTime.TryParse(createdAt, out var date))
                    {
                        return date.ToString("MM/dd");
                    }
                    return string.Empty; // Return empty if parsing fails
                }
            }
        }
    }
