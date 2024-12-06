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
        //http Client for API request
        private readonly HttpClient _httpClient;
        //reference to current page to enable navigation
        private readonly Page _page;
        //stores and allows dynamic updates userGroups
        private List<UserGroup> _userGroups;
        //property bound to UI that updates on PropertyChange
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
        //stores and allows dynamic updates to PeerReviews
        private List<PeerReview> _peerReview;
        //property bound to UI that updates on PropertyChange
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
        //property bound to UI that stores and filters for users with teh "student" role
        public List<UserGroup> StudentGroups =>
            _userGroups?.FindAll(group => string.Equals(group.role, "student", StringComparison.OrdinalIgnoreCase)) ?? new List<UserGroup>();
        //Event to notify UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        //triggers on property change event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //command bound to UI to get PeerReviews
        public ICommand FetchPeerReviewsCommand { get; }
        //Command bound to UI to toggle expansion of Reviews Given
        public ICommand ToggleReviewsGivenExpandCommand {get; }
        //toggles boolean value that indicates expansion state for table Reviews Given
        private void ToggleReviewsGivenExpand(User userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsReviewsGivenExpanded = !userGroup.IsReviewsGivenExpanded;

                // collapse the expansion
                if (userGroup.IsReviewsGivenExpanded)
                {
                    userGroup.IsReviewsGivenExpanded = false;
                }

                OnPropertyChanged(nameof(UserGroups));
            }
        }
        //Command bound to UI to toggle expansion of Reviews Received
         public ICommand ToggleReviewsReceivedExpandCommand {get; }
        //toggles boolean value that indicates expansion state for table Reviews Received
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
        //Command bound to UI that navigates to CreatePRQPage
         public ICommand NavigateToCreatePRQCommand { get; }
        //constructor for ViewModel
        public PeerReviewPageViewModel(Page page)
        {
            //reference current page
            _page = page;
            //initialzie http Client
            _httpClient = new HttpClient();
            //niitialize command that awaits button and asynchronously loads CreatePRQReviewPage
            NavigateToCreatePRQCommand = new Command(async () =>
            {
                //navigate to PeerReviewPage
                await _page.Navigation.PushAsync(new CreatePRQPage());
            });
            //initialize command that with that fetches users and their corresponding peer reviews
            FetchPeerReviewsCommand = new Command(async () => await LoadUserGroupsAsync());
            //initialize commands that toggle expansion states
            ToggleReviewsGivenExpandCommand = new Command <User>(ToggleReviewsGivenExpand);
            ToggleReviewsReceivedExpandCommand = new Command <User>(ToggleReviewsReceivedExpand);
            //load user groups and peer reviews asycnhronously
            LoadUserGroupsAsync().ConfigureAwait(false);
        }

        //load user groups and peer reviews asycnhronously
        private async Task LoadUserGroupsAsync()
        {
            var url = "http://localhost:5264/api/user/groups";

            try
            {
                //fetch user groups and parse response
                var responseString = await _httpClient.GetStringAsync(url);
                var jsonDocument = JsonDocument.Parse(responseString);
                //create list of users 
                var allUserGroups = new List<UserGroup>();
                //process users by key value (group number)
                foreach (var groupProperty in jsonDocument.RootElement.EnumerateObject())
                {
                    //deserialize user groups
                    var userGroupList = JsonSerializer.Deserialize<List<UserGroup>>(groupProperty.Value.GetRawText());
                    //set deserializing options to convert response to camel case
                    var options = new JsonSerializerOptions
                    {
                        //handles camelCase mapping
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                        //allow case-insensitive matching
                        PropertyNameCaseInsensitive = true 
                    };
                    //process users belonging to same group
                    //assign user processed in outer loop as "reviewer"
                    //FOR TESTING PURPOSES: limit the amount of reviewers to 2 because several API calls take too much time to process
                    foreach (var reviewer in userGroupList.Take(2))
                    {
                        //initialize ReviewsGiven for the reviewer
                        reviewer.ReviewsGiven = reviewer.ReviewsGiven ?? new List<PeerReview>();
                        //iterate through users again in nested loops
                        //ensure that reviewers and reviewees do not match 
                        //FOR TESTING PURPOSES: limit the amount of reviewers to 3 because several API calls take too much time to process
                        foreach (var reviewee in userGroupList.Where(u => u.netID != reviewer.netID).Take(3))
                        {
                            //fetch peer reviews for each reviewer-reviewee pair using associated PK netId
                            var prUrl = $"http://localhost:5264/api/peerreviewanswer/reviewer/{reviewer.netID}/reviewee/{reviewee.netID}";
                            try
                            {
                                //store response
                                var prResponseString = await _httpClient.GetStringAsync(prUrl);
                                //check for response validitiy before initializing objects
                                //there will be several null responses at some point so allow null strings to not trigger try-catch alerts
                                if (!string.IsNullOrWhiteSpace(prResponseString))
                                {
                                    // deserialize peer reviews and format
                                    var peerReviews = JsonSerializer.Deserialize<List<PeerReview>>(prResponseString, options);
                                    //each reviewer-reviewee pair will have up to two reviews
                                    foreach (var peerReview in peerReviews)
                                    {
                                        //validate response
                                        if (peerReview != null)
                                        {
                                            //sdd to reviewer's list of ReviewsGiven and increment reviewGiven
                                            reviewer.ReviewsGiven.Add(peerReview);
                                            reviewer.NumberReviewsGiven += 1;

                                            // Add to reviewee's list of ReviewsReceived and increment reviewsReceived
                                            reviewee.ReviewsReceived = reviewee.ReviewsReceived ?? new List<PeerReview>();
                                            reviewee.ReviewsReceived.Add(peerReview);
                                            reviewee.NumberReviewsReceived += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    // log if no data found
                                    Console.WriteLine($"No peer review data found for reviewer {reviewer.netID} and reviewee {reviewee.netID}.");
                                }
                            }
                            catch (Exception innerEx)
                            {
                                // log errors for individual reviewer-reviewee pairs
                                Console.WriteLine($"Error fetching peer review for reviewer {reviewer.netID} and reviewee {reviewee.netID}: {innerEx.Message}");
                            }
                        }
                    }
                    //add all users to list of users if not null
                    if (userGroupList != null)
                    {
                        allUserGroups.AddRange(userGroupList);
                    }
                }
                //sert UserGroups property bound to UI
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
    //class that define PeerReview relation objects and attributes that will be displayed in UI
    //PeerReview only exists in the context of a relation between two User entities
    //PeerReview and its associated questions (PeerReviewQuestion) and answers (PeerReviewAnswer) are sent in single DTO from endpoint
    public class PeerReview
    {
        //primary key 
        public int PeerReviewId { get; set; }
        //correspinding Reviewer and Reviewee
        public string ReviewerName { get; set; }
        public string RevieweeName { get; set; }
        //submission date
        public DateTime SubmittedAt { get; set; }
        //List that contains answers to PeerReviews
        public List<PeerReviewAnswer> Answers { get; set; }
    }
    //class that defines PeerReviewAnswers and associated questions
    //PeerReviewAnswers only exist in the context of PeerReview relation
    //maps to PeerReview via PeerReviewID FK (not defined here)
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
    

