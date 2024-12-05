// Written by Grace Y.
// This is the ViewModel to manage user groups and their data
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
        private List<UserGroup> _userGroups; // Stores the list of user groups
        private readonly HttpClient _httpClient; // HttpClient for API communication
        private readonly Page _page; // Reference to the current page for navigation and UI updates

        private DateTime _selectedWeekStartDate; // Selected start date of the week

        // Navigates to the Peer Review page
        public ICommand NavigateToPeerReviewCommand { get; }

        // Property for the selected start date of the week, triggers updates when changed
        public DateTime SelectedWeekStartDate
        {
            get => _selectedWeekStartDate;
            set
            {
                if (_selectedWeekStartDate != value)
                {
                    _selectedWeekStartDate = value;
                    OnPropertyChanged(nameof(SelectedWeekStartDate)); // Notify UI of change
                    UpdateFilteredTimeLogs(); // Update logs when the date changes
                }
            }
        }

        // Property to store the user groups, triggers updates
        public List<UserGroup> UserGroups
        {
            get => _userGroups;
            set
            {
                if (_userGroups != value)
                {
                    _userGroups = value;
                    OnPropertyChanged(nameof(UserGroups)); // Notify UI
                    OnPropertyChanged(nameof(StudentGroups)); // Notify dependent property
                }
            }
        }

        // Filters user groups for students only
        public List<UserGroup> StudentGroups =>
            _userGroups?.FindAll(group => string.Equals(group.role, "student", StringComparison.OrdinalIgnoreCase)) ?? new List<UserGroup>();

        // Command to fetch user groups from the backend API
        public ICommand FetchUserGroupsCommand { get; }
        // Command to toggle the weekly hours expansion for a user group
        public ICommand ToggleWeeklyExpandCommand { get; }
        // Command to toggle the cumulative hours expansion for a user group
        public ICommand ToggleCumulativeExpandCommand { get; }

        // Constructor to initialize the ViewModel
        public UserGroupsViewModel(Page page)
        {
            _page = page;
            _httpClient = new HttpClient();
            // Command to navigate to the Peer Review page
            NavigateToPeerReviewCommand = new Command(async () =>
            {
                // Navigate to PeerReviewPage
                await _page.Navigation.PushAsync(new PeerReviewPage());
            });
            FetchUserGroupsCommand = new Command(async () => await FetchUserGroupsAsync()); // Initialize fetch command
            ToggleWeeklyExpandCommand = new Command<UserGroup>(ToggleWeeklyExpand); // Initialize weekly toggle command
            ToggleCumulativeExpandCommand = new Command<UserGroup>(ToggleCumulativeExpand); // Initialize cumulative toggle command
            SelectedWeekStartDate = DateTime.Now; // Initialize with the current week
            FetchUserGroupsAsync().ConfigureAwait(false); // Fetch data from API
        }

        // Method to toggle the weekly hours expansion for a specific user group
        private void ToggleWeeklyExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsExpandedForWeeklyHours = !userGroup.IsExpandedForWeeklyHours;

                // Collapse cumulative hours if weekly hours are expanded
                if (userGroup.IsExpandedForWeeklyHours)
                {
                    userGroup.IsExpandedForCumulativeHours = false;
                }

                OnPropertyChanged(nameof(UserGroups)); // Notify UI to refresh
            }
        }

        // Method to toggle the cumulative hours expansion for a specific user group
        private void ToggleCumulativeExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsExpandedForCumulativeHours = !userGroup.IsExpandedForCumulativeHours;

                // Collapse weekly hours if cumulative hours are expanded
                if (userGroup.IsExpandedForCumulativeHours)
                {
                    userGroup.IsExpandedForWeeklyHours = false;
                }

                OnPropertyChanged(nameof(UserGroups));
            }
        }

        // Updates filtered time logs for each user group based on the selected week
        private void UpdateFilteredTimeLogs()
        {
            if (UserGroups == null || !UserGroups.Any())
                return;

            var startOfWeek = SelectedWeekStartDate; // Start date of the week
            var endOfWeek = startOfWeek.AddDays(6); // End date of the week

            foreach (var userGroup in UserGroups)
            {
                userGroup.UpdateFilteredTimeLogs(startOfWeek, endOfWeek); // Update filtered logs
            }

            // Notify the view to refresh the UI
            OnPropertyChanged(nameof(UserGroups)); 
        }


        // Event for property change notification
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to raise property change notifications
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Fetches user groups from the backend API
        private async Task FetchUserGroupsAsync()
        {
            var url = "http://localhost:5264/api/user/groups"; // API endpoint

            try
            {
                var responseString = await _httpClient.GetStringAsync(url); // Fetch response as string
                var jsonDocument = JsonDocument.Parse(responseString); // Parse JSON

                var allUserGroups = new List<UserGroup>();

                // Deserialize each user group from the JSON
                foreach (var groupProperty in jsonDocument.RootElement.EnumerateObject())
                {
                    var userGroupList = JsonSerializer.Deserialize<List<UserGroup>>(groupProperty.Value.GetRawText());
                    if (userGroupList != null)
                    {
                        allUserGroups.AddRange(userGroupList);
                    }
                }

                UserGroups = allUserGroups; // Assign the fetched user groups

                // Calculate and format weekly and cumulative hours for each user group
                foreach (var userGroup in UserGroups)
                {
                    // Loop through the time logs and calculate the hours/mins spent
                    int totalCumulativeMinutes = 0; // For cumulative minutes
                    foreach (var timeLog in userGroup.timeLogs)
                    {
                        // For weekly minutes
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

                UpdateFilteredTimeLogs(); // Refresh filtered logs
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }


    // Represents a user group with properties and logic for managing time logs
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
        
        // Filtering time log properties and logic
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

        // Updating the filtered time logs 
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
                                            DateTime.Parse(entry.createdAt).Date <= selectedWeekEndDate.Date)
                            .ToList()
                    })
                    .ToList();

                // Recalculate WeeklyCumulativeHours
                int totalWeeklyMinutes = FilteredTimeLogs.Sum(log => log.timeLogEntries.Sum(entry => entry.duration));
                WeeklyCumulativeHours = totalWeeklyMinutes;
                WeeklyCumulativeHoursFormatted = $"{totalWeeklyMinutes / 60:D2}:{totalWeeklyMinutes % 60:D2}";
            }
            else
            {
                // If no minutes, then set the weekly cumulative hours to 0
                FilteredTimeLogs = new List<TimeLog>();
                WeeklyCumulativeHours = 0;
                WeeklyCumulativeHoursFormatted = "00:00";
            }

            OnPropertyChanged(nameof(FilteredTimeLogs));
            OnPropertyChanged(nameof(WeeklyCumulativeHoursFormatted));
        }

        // For the expanded weekly hours property and logic 
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

        // For the expanded cumulative hours property and logic
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

        // Number of reviews given to student for Peer Review
        private int _numberReviewsGiven;
        public int NumberReviewsGiven
        {
            get => _numberReviewsGiven;
            set
            {
                if (_numberReviewsGiven != value)
                {
                    _numberReviewsGiven = value;
                    OnPropertyChanged(nameof(NumberReviewsGiven));
                }
            }
        }

        // Number of reviews received by student for Peer Review
        private int _numberReviewsReceived;
        public int NumberReviewsReceived
        {
            get => _numberReviewsReceived;
            set
            {
                if (_numberReviewsReceived != value)
                {
                    _numberReviewsReceived = value;
                    OnPropertyChanged(nameof(NumberReviewsReceived)); 
                }
            }
        }

        // If user has reviews received, display them 
        public bool HasReviewsReceived => _numberReviewsReceived > 0;

        public List<User> Users { get; set; }

        // Header for the reviews received
        public string ReviewsReceivedHeader => $"Reviews Received for {firstName} {lastName}";

        // Reviews received
        private List<PeerReview> _reviewsReceived;
        public List<PeerReview> ReviewsReceived {
            get => _reviewsReceived;
            set
            {
                if (_reviewsReceived != value)
                {
                    _reviewsReceived = value;
                    OnPropertyChanged(nameof(ReviewsReceived));
                }
            } 
        }
        private List<PeerReview> _reviewsGiven;

        public List<PeerReview> ReviewsGiven {
            get => _reviewsGiven;
            set
            {
                if (_reviewsGiven != value)
                {
                    _reviewsGiven = value;
                    OnPropertyChanged(nameof(ReviewsGiven));
                }
            } 
        }
    }

    // Represents a time log with associated entries
    public class TimeLog
        {
            public int id { get; set; } // Time log ID
            public int userId { get; set; } // Associated User ID
            public string title { get; set; } // Title of time log
            public List<TimeLogEntry> timeLogEntries { get; set; } // List of time log entries
        }

        // Represents an individual entry in a time log
        public class TimeLogEntry
        {
            public int id { get; set; } // Entry ID
            public int timeLogId { get; set; } // Associated time log ID
            public int duration { get; set; } // Duration in minutes
            public string DurationFormatted => $"{duration / 60:D2}:{duration % 60:D2}"; // Formatted duration
            public string description { get; set; } // Description of entry
            public string createdAt { get; set; } // Creation date of entry

            public string FormattedCreatedAt
            {
                get
                {
                    if (DateTime.TryParse(createdAt, out var date))
                    {
                        return date.ToString("MM/dd"); // Return formatted date
                    }
                    return string.Empty; // Return empty if parsing fails
                }
            }
        }
    }
