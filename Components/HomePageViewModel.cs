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

        public ICommand NavigateToPeerReviewCommand { get; }

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
            NavigateToPeerReviewCommand = new Command(async () =>
            {
                // Log a message when the button is clicked
                Console.WriteLine("NavigateToPeerReviewCommand executed!");

                // Navigate to PeerReviewPage
                await _page.Navigation.PushAsync(new PeerReviewPage());
            });
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

            // Notify the view to refresh the UI
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
                FilteredTimeLogs = new List<TimeLog>();
                WeeklyCumulativeHours = 0;
                WeeklyCumulativeHoursFormatted = "00:00";
            }

            OnPropertyChanged(nameof(FilteredTimeLogs));
            OnPropertyChanged(nameof(WeeklyCumulativeHoursFormatted));
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

        private int _numberReviewsGiven;
        public int NumberReviewsGiven
        {
            get => _numberReviewsGiven;
            set
            {
                if (_numberReviewsGiven != value)
                {
                    _numberReviewsGiven = value;
                    OnPropertyChanged(nameof(NumberReviewsGiven)); // Only needed if bound to the UI
                }
            }
        }

        private int _numberReviewsReceived;
        public int NumberReviewsReceived
        {
            get => _numberReviewsReceived;
            set
            {
                if (_numberReviewsReceived != value)
                {
                    _numberReviewsReceived = value;
                    OnPropertyChanged(nameof(NumberReviewsReceived)); // Only needed if bound to the UI
                }
            }
        }

        public bool HasReviewsReceived => _numberReviewsReceived > 0;

        public List<User> Users { get; set; }

        private bool _isReviewsGivenExpanded;
        public bool IsReviewsGivenExpanded
        {
            get => _isReviewsGivenExpanded;
            set
            {
                if (_isReviewsGivenExpanded != value)
                {
                    _isReviewsGivenExpanded = value;
                    OnPropertyChanged(nameof(IsReviewsGivenExpanded));
                }
            }
        }

        private bool _isReviewsReceivedExpanded;
        public bool IsReviewsReceivedExpanded
        {
            get => _isReviewsReceivedExpanded;
            set
            {
                if (_isReviewsReceivedExpanded != value)
                {
                    _isReviewsReceivedExpanded = value;
                    OnPropertyChanged(nameof(IsReviewsReceivedExpanded));
                }
            }
        }
        
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
