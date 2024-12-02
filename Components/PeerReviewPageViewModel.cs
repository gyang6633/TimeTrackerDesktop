
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

         public ICommand NavigateToCreatePRQCommand { get; }

        public PeerReviewPageViewModel(Page page)
        {
            _page = page;
            _httpClient = new HttpClient();
            NavigateToCreatePRQCommand = new Command(async () =>
            {
                // Navigate to PeerReviewPage
                await _page.Navigation.PushAsync(new CreatePRQPage());
            });
            FetchPeerReviewsCommand = new Command(async () => await LoadPeerReviewsAsync());
            ToggleReviewsGivenExpandCommand = new Command <UserGroup>(ToggleReviewsGivenExpand);
            ToggleReviewsReceivedExpandCommand = new Command <UserGroup>(ToggleReviewsReceivedExpand);
            LoadPeerReviewsAsync().ConfigureAwait(false);
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
                await Application.Current.MainPage.DisplayAlert("Output", responseString, "OK");
                //for each user in returned list of users
                foreach (var userGroup in UserGroups)
                {
                    //count number of reviews
                    int numReviewsGiven = 0;
                    int numReviewsReceived = 0;
                    //loop through all reviews 
                    foreach (var review in userGroup.ReviewsGiven)
                    {
                        //increment per review
                        numReviewsGiven += 1;
                    }
                    foreach (var review in userGroup.ReviewsReceived)
                    {
                        numReviewsReceived += 1;
                    }
                    //set property in user class
                    userGroup.NumberReviewsGiven = numReviewsGiven;
                    userGroup.NumberReviewsReceived = numReviewsReceived;
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
    public class PeerReviewAnswer
    {
        public int PeerReviewAnswerId { get; set; } // The answer ID

        // Foreign key to PeerReview and PeerReviewQuestion
        public int PeerReviewId { get; set; } // The review ID for this answer
        public required PeerReview PeerReview { get; set; } // Navigation property to the PeerReview

        public int PeerReviewQuestionId { get; set; } // The question ID for this answer
        public required PeerReviewQuestion PeerReviewQuestion { get; set; } // Navigation property to the PeerReviewQuestion

        // Answer details
        public int NumericalFeedback { get; set; }  // Numerical score for this specific question
        public required string WrittenFeedback { get; set; } // Written feedback for this specific question
    }
        public class PeerReviewQuestion
    {
        public int PeerReviewQuestionId { get; set; } // The question ID
        public required string QuestionText { get; set; }  // The question text
    }
}
