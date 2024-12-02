using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MauiApp3.Components;

namespace MauiApp3
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private readonly UserGroupsViewModel _viewModel;
        private DateTime _selectedWeekStartDate;
        private string _weekRangeDisplay;
        

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public HomePage()
        {
            InitializeComponent();
            // Add a button to navigate to PeerReviewPage

            // Initialize ViewModel and bind it to the page
            _viewModel = new UserGroupsViewModel(this);
            BindingContext = _viewModel;

            // Initialize the selected week
            SelectedWeekStartDate = GetStartOfWeek(DateTime.Now);
            _viewModel.SelectedWeekStartDate = SelectedWeekStartDate;

            UpdateWeekRangeDisplay();
        }

        // Property for the currently selected week's start date
        public DateTime SelectedWeekStartDate
        {
            get => _selectedWeekStartDate;
            set
            {
                if (_selectedWeekStartDate != value)
                {
                    _selectedWeekStartDate = value;
                    OnPropertyChanged(nameof(SelectedWeekStartDate));
                    WeekRangeDisplay = $"{GetStartOfWeek(_selectedWeekStartDate):MM/dd/yyyy} - {GetEndOfWeek(_selectedWeekStartDate):MM/dd/yyyy}";
                }
            }
        }

        // Display the week range
        public string WeekRangeDisplay
        {
            get => _weekRangeDisplay;
            private set
            {
                if (_weekRangeDisplay != value)
                {
                    _weekRangeDisplay = value;
                    OnPropertyChanged(nameof(WeekRangeDisplay));
                }
            }
        }

        // Update the display for the week range
        private void UpdateWeekRangeDisplay()
        {
            OnPropertyChanged(nameof(WeekRangeDisplay));
        }

        // Commands for navigation
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private DateTime GetEndOfWeek(DateTime date)
        {
            return GetStartOfWeek(date).AddDays(6);
        }
    }
}
