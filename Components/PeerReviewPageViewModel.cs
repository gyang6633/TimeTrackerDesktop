
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

        private List<PeerReview> _peerReview;
        public List<PeerReview> PeerReviews
        {
            get => _peerReview;
            set
            {
                if (_peerReview != value)
                {
                    _peerReview = value;
                    OnPropertyChanged(nameof(PeerReview));
                  
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
        private void ToggleReviewsGivenExpand(User userGroup)
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
        private void ToggleReviewsReceivedExpand(User user)
        {
            if (user != null)
            {
                user.IsReviewsReceivedExpanded = !user.IsReviewsReceivedExpanded;

                // Collapse the other expansion
                if (user.IsReviewsReceivedExpanded)
                {
                    user.IsReviewsReceivedExpanded = false;
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
            FetchPeerReviewsCommand = new Command(async () => await LoadUserGroupsAsync());
            ToggleReviewsGivenExpandCommand = new Command <User>(ToggleReviewsGivenExpand);
            ToggleReviewsReceivedExpandCommand = new Command <User>(ToggleReviewsReceivedExpand);
            LoadUserGroupsAsync().ConfigureAwait(false);
        }


        private async Task LoadUserGroupsAsync()
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
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Handles camelCase mapping
                        PropertyNameCaseInsensitive = true // Allows case-insensitive matching
                    };

                    foreach (var reviewer in userGroupList.Take(2))
                    {
                        // Initialize ReviewsGiven for the reviewer
                        reviewer.ReviewsGiven = reviewer.ReviewsGiven ?? new List<PeerReview>();

                        foreach (var reviewee in userGroupList.Where(u => u.netID != reviewer.netID).Take(3))
                        {
                            // Fetch peer reviews for each reviewer-reviewee pair
                            var prUrl = $"http://localhost:5264/api/peerreviewanswer/reviewer/{reviewer.netID}/reviewee/{reviewee.netID}";
                            try
                            {
                                var prResponseString = await _httpClient.GetStringAsync(prUrl);

                                // Deserialize peer reviews
                                if (!string.IsNullOrWhiteSpace(prResponseString))
                                {
                                    var peerReviews = JsonSerializer.Deserialize<List<PeerReview>>(prResponseString, options);
                                    foreach (var peerReview in peerReviews)
                                    {
                                        if (peerReview != null)
                                        {
                                            // Add to reviewer's list of ReviewsGiven
                                            reviewer.ReviewsGiven.Add(peerReview);
                                            reviewer.NumberReviewsGiven += 1;

                                            // Add to reviewee's list of ReviewsReceived
                                            reviewee.ReviewsReceived = reviewee.ReviewsReceived ?? new List<PeerReview>();
                                            reviewee.ReviewsReceived.Add(peerReview);
                                            reviewee.NumberReviewsReceived += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    // Log if no data found
                                    Console.WriteLine($"No peer review data found for reviewer {reviewer.netID} and reviewee {reviewee.netID}.");
                                }
                            }
                            catch (Exception innerEx)
                            {
                                // Log errors for individual reviewer-reviewee pairs
                                Console.WriteLine($"Error fetching peer review for reviewer {reviewer.netID} and reviewee {reviewee.netID}: {innerEx.Message}");
                            }
                        }
                    }

                    if (userGroupList != null)
                    {
                        allUserGroups.AddRange(userGroupList);
                    }
                }

                UserGroups = allUserGroups;

                // Notify UI
                OnPropertyChanged(nameof(UserGroups));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching peer reviews");
            }
        }
    }



    public class PeerReview
    {
        public int PeerReviewId { get; set; }
        public string ReviewerName { get; set; }
        public string RevieweeName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<PeerReviewAnswer> Answers { get; set; }
    }

    public class PeerReviewAnswer
    {
        public string Question { get; set; }
        public int NumericalFeedback { get; set; }
        public string WrittenFeedback { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string NetID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public int Group { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
    

