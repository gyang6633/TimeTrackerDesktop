using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp3
{
    public class CreatePRQPageViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private string _newQuestionText;

        public ObservableCollection<PeerReviewQuestion> Questions { get; set; } = new ObservableCollection<PeerReviewQuestion>();

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

        public Command SubmitQuestionCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public CreatePRQPageViewModel()
        {
            _httpClient = new HttpClient();
            SubmitQuestionCommand = new Command(async () => await SubmitQuestionAsync());
            LoadQuestionsAsync().ConfigureAwait(false);
        }

        private async Task LoadQuestionsAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("http://localhost:5264/api/peerreviewquestion");

                // Configure deserialization options for camelCase to PascalCase mapping
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Ignore case differences between JSON and C# properties
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var questions = JsonSerializer.Deserialize<List<PeerReviewQuestion>>(response, options);

                if (questions != null)
                {
                    Questions.Clear();
                    foreach (var question in questions)
                    {
                        Questions.Add(question);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error loading questions: {ex.Message}", "OK");
            }
        }

        private async Task SubmitQuestionAsync()
        {
            if (string.IsNullOrWhiteSpace(NewQuestionText))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Question text is empty.", "OK");
                return;
            }

            try
            {
                var newQuestion = new PeerReviewQuestion { QuestionText = NewQuestionText };
                var content = new StringContent(JsonSerializer.Serialize(newQuestion), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:5264/api/peerreviewquestion", content);
                if (response.IsSuccessStatusCode)
                {
                    Questions.Add(newQuestion);
                    NewQuestionText = string.Empty;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Error submitting question: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error submitting question: {ex.Message}", "OK");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PeerReviewQuestion
    {
        public int PeerReviewQuestionId { get; set; } // The question ID
        public required string QuestionText { get; set; } // The question text
    }
}