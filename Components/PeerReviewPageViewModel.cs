
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiApp3.Components;

namespace MauiApp3.Components
{
    public class PeerReviewPageViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly Page _page;
        private List<UserGroup> _userGroups;
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand FetchPeerReviewsCommand { get; }
        public ICommand ToggleReviewsGivenExpandCommand {get; }
        private void ToggleReviewsGivenExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsReviewsGivenExpanded = !userGroup.IsReviewsGivenExpanded;

                // Collapse the other expansion
                if (userGroup.IsReviewsGivenExpanded)
                {
                    userGroup.IsReviewsGivenExpanded = false;
                }

                OnPropertyChanged(nameof(UserGroups));
            }
        }

         public ICommand ToggleReviewsReceivedExpandCommand {get; }
        private void ToggleReviewsReceivedExpand(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsReviewsReceivedExpanded = !userGroup.IsReviewsReceivedExpanded;

                // Collapse the other expansion
                if (userGroup.IsReviewsReceivedExpanded)
                {
                    userGroup.IsReviewsReceivedExpanded = false;
                }

                OnPropertyChanged(nameof(UserGroups));
            }
        }

        public PeerReviewPageViewModel(Page page)
        {
            _page = page;
            _httpClient = new HttpClient();
            FetchPeerReviewsCommand = new Command(async () => await LoadPeerReviewsAsync());
            LoadPeerReviewsAsync().ConfigureAwait(false);
            ToggleReviewsGivenExpandCommand = new Command <UserGroup>(ToggleReviewsGivenExpand);
            ToggleReviewsReceivedExpandCommand = new Command <UserGroup>(ToggleReviewsReceivedExpand);
        }


        private async Task LoadPeerReviewsAsync()
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
                    int reviewsGiven = 0;
                    int reviewsReceived = 0;
                    foreach (var review in userGroup.ReviewsGiven)
                    {
                        reviewsGiven += 1;
                    }
                    foreach (var review in userGroup.ReviewsReceived)
                    {
                        reviewsReceived += 1;
                    }
                    userGroup.NumberReviewsGiven = reviewsGiven;
                    userGroup.NumberReviewsReceived = reviewsReceived;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching peer reviews: {ex.Message}");
            }
        }
    }

    public class PeerReview
    {
        public int PeerReviewId { get; set; }
        public string ReviewerId { get; set; }
        public UserGroup Reviewer { get; set; }
        public string RevieweeId { get; set; }
        public UserGroup Reviewee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        private bool _isExpandedForQuestions;
        public bool IsExpandedForQuestions
        {
            get => _isExpandedForQuestions;
            set
            {
                if (_isExpandedForQuestions != value)
                {
                    _isExpandedForQuestions = value;
                    OnPropertyChanged(nameof(IsExpandedForQuestions));
                }
            }
        }

        private bool _isExpandedForAnswers;
        public bool IsExpandedForAnswers
        {
            get => _isExpandedForAnswers;
            set
            {
                if (_isExpandedForAnswers != value)
                {
                    _isExpandedForAnswers = value;
                    OnPropertyChanged(nameof(IsExpandedForAnswers));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
