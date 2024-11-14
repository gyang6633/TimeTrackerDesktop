using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MauiApp3.Components;

namespace MauiApp3
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        private DateTime _selectedWeekStartDate;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    UpdateWeekRangeDisplay();
                }
            }
        }

        // Property for displaying the week range as text
        public string WeekRangeDisplay { get; private set; }

        private void UpdateWeekRangeDisplay()
        {
            WeekRangeDisplay = $"{GetStartOfWeek(SelectedWeekStartDate):MM/dd/yyyy} - {GetEndOfWeek(SelectedWeekStartDate):MM/dd/yyyy}";
            OnPropertyChanged(nameof(WeekRangeDisplay));
        }

        // Commands for navigating between weeks
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }

        public HomePage()
        {
            InitializeComponent();
            BindingContext = new UserGroupsViewModel(this);

            SelectedWeekStartDate = GetStartOfWeek(DateTime.Now);

            PreviousWeekCommand = new Command(() =>
            {
                SelectedWeekStartDate = SelectedWeekStartDate.AddDays(-7);
            });

            NextWeekCommand = new Command(() =>
            {
                SelectedWeekStartDate = SelectedWeekStartDate.AddDays(7);
            });

            UpdateWeekRangeDisplay();
        }

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
