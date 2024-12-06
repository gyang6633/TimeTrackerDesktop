//Written by Grace Y.
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MauiApp3.Components;

namespace MauiApp3
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private readonly UserGroupsViewModel _viewModel; // The ViewModel associated with this page
        private DateTime _selectedWeekStartDate; // Stores the start date of the currently selected week
        private string _weekRangeDisplay; // Stores the display text for the selected week's range

        // Event to notify the UI about property changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to raise the PropertyChanged event
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Constructor for the HomePage
        public HomePage()
        {
            InitializeComponent(); // Initialize UI components defined in XAML
            
            // Initialize the ViewModel and bind it to the page's BindingContext
            _viewModel = new UserGroupsViewModel(this);
            BindingContext = _viewModel;

            // Set the initial selected week to the current week
            SelectedWeekStartDate = GetStartOfWeek(DateTime.Now);
            _viewModel.SelectedWeekStartDate = SelectedWeekStartDate;

            // Update the displayed week range
            UpdateWeekRangeDisplay();
        }

        // Property for the currently selected week's start date
        public DateTime SelectedWeekStartDate
        {
            get => _selectedWeekStartDate;
            set
            {
                // Update the property and notify the UI if the value changes
                if (_selectedWeekStartDate != value)
                {
                    _selectedWeekStartDate = value;
                    OnPropertyChanged(nameof(SelectedWeekStartDate));

                    // Update the week range display with the new dates
                    WeekRangeDisplay = $"{GetStartOfWeek(_selectedWeekStartDate):MM/dd/yyyy} - {GetEndOfWeek(_selectedWeekStartDate):MM/dd/yyyy}";
                }
            }
        }

        // Property for the text display of the week range
        public string WeekRangeDisplay
        {
            get => _weekRangeDisplay;
            private set
            {
                // Update the property and notify the UI if the value changes
                if (_weekRangeDisplay != value)
                {
                    _weekRangeDisplay = value;
                    OnPropertyChanged(nameof(WeekRangeDisplay));
                }
            }
        }

        // Updates the week range display by notifying the UI
        private void UpdateWeekRangeDisplay()
        {
            OnPropertyChanged(nameof(WeekRangeDisplay));
        }

        // Commands for navigating between weeks
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }

        // Helper method to calculate the start date of the week containing the given date
        private DateTime GetStartOfWeek(DateTime date)
        {
            // Calculate the difference between the given date's day of the week and Monday
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

            // Subtract the difference to get the Monday of the current week
            return date.AddDays(-diff).Date;
        }

        // Helper method to calculate the end date of the week containing the given date
        private DateTime GetEndOfWeek(DateTime date)
        {
            // Add 6 days to the start of the week to get the Sunday of the current week
            return GetStartOfWeek(date).AddDays(6);
        }
    }
}