//Written by Eric W.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp3
{
    //ViewModel for CreatePRQPage
    public class CreatePRQPageViewModel : INotifyPropertyChanged
    {
        //http client for making APR requests and retrieving responses
        private readonly HttpClient _httpClient;
        //dynamically stores text to be entered in input field
        private string _newQuestionText;
        //observable collection to hold list of existing peer review questions
        //bound to UI and updates dynamically
        public ObservableCollection<PeerReviewQuestion> Questions { get; set; } = new ObservableCollection<PeerReviewQuestion>();
        //property for new questions that trigger on UI update. bound to UI
        public string NewQuestionText
        {
            get => _newQuestionText;
            set
            {
                if (_newQuestionText != value)
                {
                    _newQuestionText = value;
                    OnPropertyChanged(nameof(NewQuestionText));
                }
            }
        }
        //command bound to submit questions button in UI. triggers http POST
        public Command SubmitQuestionCommand { get; }
        //event to notify UI of property changes for bound variables
        public event PropertyChangedEventHandler PropertyChanged;
        //default constructor to initialize viewModel
        public CreatePRQPageViewModel()
        {
            //initialize HTTP client for API calls
            _httpClient = new HttpClient();
            //initialize command for submitting questions that triggers async http POST
            SubmitQuestionCommand = new Command(async () => await SubmitQuestionAsync());
            //asyncronously load existing questions from API response
            LoadQuestionsAsync().ConfigureAwait(false);
        }
        //load questions async from database through API response
        private async Task LoadQuestionsAsync()
        {
            try
            {
                //call endpoint and convert to string using httpClient
                var response = await _httpClient.GetStringAsync("http://localhost:5264/api/peerreviewquestion");
                // configure deserialization options for camelCase to PascalCase mapping
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Ignore case differences between JSON and C# properties
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                //deserialize JSON response into a List of PeerReviewQuestions
                var questions = JsonSerializer.Deserialize<List<PeerReviewQuestion>>(response, options);
                //is questions are not null, create new questions objects from response
                if (questions != null)
                {
                    Questions.Clear();
                    foreach (var question in questions)
                    {
                        Questions.Add(question);
                    }
                }
            }
            //trigger alert if API response is null
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error loading questions: {ex.Message}", "OK");
            }
        }
        //aynchronously submit questions via http POST
        private async Task SubmitQuestionAsync()
        {
            //validate input: if the input field is empty when the button is clicked
            //trigger alert that the field is empty
            if (string.IsNullOrWhiteSpace(NewQuestionText))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Question text is empty.", "OK");
                return;
            }

            try
            {
                // prepare form data to be sent 
                var formData = new Dictionary<string, string>
                {
                    { "QuestionText", NewQuestionText }
                };
                //convert to htto content for POST request
                var content = new FormUrlEncodedContent(formData);
                // make POST request
                var response = await _httpClient.PostAsync("http://localhost:5264/api/peerreviewquestion", content);
                //validate request
                if (response.IsSuccessStatusCode)
                {
                    // read response content
                    var responseString = await response.Content.ReadAsStringAsync();
                    //set JSON desrialize options
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    //deserialize response
                    var createdQuestion = JsonSerializer.Deserialize<PeerReviewQuestion>(responseString, options);
                    //validate response and add the new question into existing list of questions
                    if (createdQuestion != null)
                    {
                        Questions.Add(createdQuestion);
                    }
                    //reset input field
                    NewQuestionText = string.Empty;
                }
                //trigger alert if response fails
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Error submitting question: {response.StatusCode}", "OK");
                }
            }
            //trigger alert if POST request fails
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error submitting question: {ex.Message}", "OK");
            }
        }
        //function to notify UI of property changes
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //class that defines Peer Review Question
    public class PeerReviewQuestion
    {
        //ensure property names match JSON response so it can properly deserialized
        //question ID primary key
        public int PeerReviewQuestionId { get; set; } 
        //question content
        public required string QuestionText { get; set; } 
    }
}