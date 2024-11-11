using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MauiApp3.Components;

namespace MauiApp3
{
    public partial class HomePage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<Section> Sections { get; set; }
        private Section _selectedSection;
        private DateTime _selectedWeekStartDate;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Property for the currently selected section
        public Section SelectedSection
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                OnPropertyChanged(nameof(SelectedSection));
                PopulateStudentHoursGrid();
            }
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
                    PopulateStudentHoursGrid();
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

            // Initialize sections with sample data
            Sections = new ObservableCollection<Section>
            {
                new Section
                {
                    SectionName = "4485.001",
                    Groups = new ObservableCollection<StudentGroup>
                    {
                        new StudentGroup
                        {
                            GroupName = "78",
                            Students = new ObservableCollection<Student>
                            {
                                new Student { FirstName = "Alex", LastName = "Johnson", WeeklyHours = new List<int> { 5, 6, 4, 3 }, CumulativeHours = 50 },
                                new Student { FirstName = "Jessica", LastName = "Lee", WeeklyHours = new List<int> { 3, 4, 5, 2 }, CumulativeHours = 45 },
                                new Student { FirstName = "Tom", LastName = "Brown", WeeklyHours = new List<int> { 6, 5, 4, 7 }, CumulativeHours = 60 }
                            }
                        },
                        new StudentGroup
                        {
                            GroupName = "79",
                            Students = new ObservableCollection<Student>
                            {
                                new Student { FirstName = "Liam", LastName = "Smith", WeeklyHours = new List<int> { 4, 5, 6, 5 }, CumulativeHours = 55 },
                                new Student { FirstName = "Sophia", LastName = "Wilson", WeeklyHours = new List<int> { 2, 3, 4, 3 }, CumulativeHours = 35 },
                                new Student { FirstName = "Olivia", LastName = "Davis", WeeklyHours = new List<int> { 5, 4, 6, 5 }, CumulativeHours = 50 }
                            }
                        }
                    }
                },
                new Section
                {
                    SectionName = "4485.002",
                    Groups = new ObservableCollection<StudentGroup>
                    {
                        new StudentGroup
                        {
                            GroupName = "80",
                            Students = new ObservableCollection<Student>
                            {
                                new Student { FirstName = "Noah", LastName = "Moore", WeeklyHours = new List<int> { 3, 4, 5, 4 }, CumulativeHours = 45 },
                                new Student { FirstName = "Emma", LastName = "White", WeeklyHours = new List<int> { 6, 5, 7, 6 }, CumulativeHours = 65 },
                                new Student { FirstName = "William", LastName = "Harris", WeeklyHours = new List<int> { 4, 3, 4, 5 }, CumulativeHours = 40 }
                            }
                        },
                        new StudentGroup
                        {
                            GroupName = "81",
                            Students = new ObservableCollection<Student>
                            {
                                new Student { FirstName = "Ava", LastName = "Martinez", WeeklyHours = new List<int> { 2, 4, 3, 4 }, CumulativeHours = 35 },
                                new Student { FirstName = "Ethan", LastName = "Garcia", WeeklyHours = new List<int> { 5, 6, 5, 4 }, CumulativeHours = 55 },
                                new Student { FirstName = "Mason", LastName = "Martinez", WeeklyHours = new List<int> { 4, 4, 4, 5 }, CumulativeHours = 40 }
                            }
                        }
                    }
                }
            };

            // Set BindingContext after initializing data
            BindingContext = new UserGroupsViewModel(this);

            if (Sections.Count > 0)
            {
                SelectedSection = Sections[0];
            }

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

        private void PopulateStudentHoursGrid()
        {
            StudentHoursGrid.Children.Clear();
            StudentHoursGrid.ColumnDefinitions.Clear();
            StudentHoursGrid.RowDefinitions.Clear();

            if (SelectedSection == null) return;

            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StudentHoursGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddToGrid(new Label { Text = "Group", FontAttributes = FontAttributes.Bold, Padding = new Thickness(10, 0), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 0, 0);
            AddToGrid(new Label { Text = "First Name", FontAttributes = FontAttributes.Bold, Padding = new Thickness(10, 0), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 1, 0);
            AddToGrid(new Label { Text = "Last Name", FontAttributes = FontAttributes.Bold, Padding = new Thickness(10, 0), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 2, 0);
            AddToGrid(new Label { Text = "Total Hours for the Week", FontAttributes = FontAttributes.Bold, Padding = new Thickness(10, 0), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 3, 0);
            AddToGrid(new Label { Text = "Cumulative Total Hours", FontAttributes = FontAttributes.Bold, Padding = new Thickness(10, 0), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 4, 0);

            int currentRow = 1;

            foreach (var group in SelectedSection.Groups)
            {
                foreach (var student in group.Students)
                {
                    var hoursForWeek = GetHoursForSelectedWeek(student);

                    StudentHoursGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    AddToGrid(new Label { Text = group.GroupName, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 0, currentRow);
                    AddToGrid(new Label { Text = student.FirstName, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 1, currentRow);
                    AddToGrid(new Label { Text = student.LastName, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 2, currentRow);
                    AddToGrid(new Label { Text = hoursForWeek.ToString(), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 3, currentRow);
                    AddToGrid(new Label { Text = student.CumulativeHours.ToString(), HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Black }, 4, currentRow);

                    currentRow++;
                }
            }
        }

        private void AddToGrid(View view, int column, int row)
        {
            StudentHoursGrid.Children.Add(view);
            Grid.SetColumn(view, column);
            Grid.SetRow(view, row);
        }

        private int GetHoursForSelectedWeek(Student student)
        {
            return student.WeeklyHours.FirstOrDefault();
        }

        public class Section
        {
            public string SectionName { get; set; }
            public ObservableCollection<StudentGroup> Groups { get; set; }
        }

        public class StudentGroup
        {
            public string GroupName { get; set; }
            public ObservableCollection<Student> Students { get; set; }
        }

        public class Student
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<int> WeeklyHours { get; set; }
            public int CumulativeHours { get; set; }
        }
    }
}
