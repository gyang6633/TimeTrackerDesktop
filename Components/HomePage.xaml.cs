using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MauiApp3
{
    public partial class HomePage : ContentPage
    {
        public ObservableCollection<Section> Sections { get; set; }
        private Section _selectedSection;
        private DateTime _selectedWeekStartDate;

        // Property for the currently selected section
        public Section SelectedSection
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                OnPropertyChanged(nameof(SelectedSection));  // Notify of property change for UI binding
                PopulateStudentHoursGrid();  // Populate the grid when a new section is selected
            }
        }

        // Property for the currently selected week's start date
        public DateTime SelectedWeekStartDate
        {
            get => _selectedWeekStartDate;
            set
            {
                _selectedWeekStartDate = value;
                OnPropertyChanged(nameof(SelectedWeekStartDate));
                PopulateStudentHoursGrid();  // Re-populate the grid based on the new week
            }
        }

        // Commands for navigating between weeks
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }

        public HomePage()
        {
            InitializeComponent();

            // Initialize sample data
            Sections = new ObservableCollection<Section>
            {
                new Section { SectionName = "001", Students = GetSampleStudents("78") },
                new Section { SectionName = "002", Students = GetSampleStudents("79") },
                new Section { SectionName = "003", Students = GetSampleStudents("80") }
            };

            // Set BindingContext after initializing data
            BindingContext = this;

            // Preselect the first section
            if (Sections.Count > 0)
            {
                SelectedSection = Sections[0];
            }

            // Set initial selected week as the current week
            SelectedWeekStartDate = GetStartOfWeek(DateTime.Now);

            // Define commands for week navigation
            PreviousWeekCommand = new Command(() => SelectedWeekStartDate = SelectedWeekStartDate.AddDays(-7));
            NextWeekCommand = new Command(() => SelectedWeekStartDate = SelectedWeekStartDate.AddDays(7));
        }

        // Method to get the start of the week
        private DateTime GetStartOfWeek(DateTime date)
        {
            // Assuming the week starts on Monday
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        // Method to populate the grid based on the selected section and week
        private void PopulateStudentHoursGrid()
        {
            // Clear any previous data in the grid
            StudentHoursGrid.Children.Clear();
            StudentHoursGrid.ColumnDefinitions.Clear();
            StudentHoursGrid.RowDefinitions.Clear();

            // If no section is selected, do not proceed
            if (SelectedSection == null) return;

            // Define columns for the grid: Group, Names, Hours Worked, Cumulative Hours
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StudentHoursGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Add row for headers
            StudentHoursGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Add headers
            AddToGrid(new Label { Text = "Group", FontAttributes = FontAttributes.Bold }, 0, 0);
            AddToGrid(new Label { Text = "Names", FontAttributes = FontAttributes.Bold }, 1, 0);
            AddToGrid(new Label { Text = "Hours Worked", FontAttributes = FontAttributes.Bold }, 2, 0);
            AddToGrid(new Label { Text = "Cumulative Hours", FontAttributes = FontAttributes.Bold }, 3, 0);

            // Populate grid with student data for the selected week
            for (int row = 0; row < SelectedSection.Students.Count; row++)
            {
                var student = SelectedSection.Students[row];
                var hoursForWeek = GetHoursForSelectedWeek(student);

                // Add a row definition for each student
                StudentHoursGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Add student data to the grid
                AddToGrid(new Label { Text = student.Group }, 0, row + 1); // Group
                AddToGrid(new Label { Text = student.Name }, 1, row + 1);  // Names
                AddToGrid(new Label { Text = hoursForWeek.ToString() }, 2, row + 1); // Hours Worked for selected week
                AddToGrid(new Label { Text = student.CumulativeHours.ToString() }, 3, row + 1); // Cumulative Hours
            }
        }

        // Helper method to add views to the grid
        private void AddToGrid(View view, int column, int row)
        {
            StudentHoursGrid.Children.Add(view);
            Grid.SetColumn(view, column);
            Grid.SetRow(view, row);
        }

        // Sample method to get hours worked by a student in the selected week
        private int GetHoursForSelectedWeek(Student student)
        {
            // Assume each student has hours worked for different weeks stored in HoursWorked list.
            // This is a placeholder logic. Adjust it according to how you're storing hours.
            return student.HoursWorked.FirstOrDefault();
        }

        // Sample data generation
        private ObservableCollection<Student> GetSampleStudents(string group)
        {
            return new ObservableCollection<Student>
            {
                new Student { Name = "Chris", Group = group, HoursWorked = new List<int> { 4 }, CumulativeHours = 20 },
                new Student { Name = "Grace", Group = group, HoursWorked = new List<int> { 2 }, CumulativeHours = 22 },
                new Student { Name = "Eric", Group = group, HoursWorked = new List<int> { 1 }, CumulativeHours = 24 },
            };
        }

        // Classes for section and student data
        public class Section
        {
            public string SectionName { get; set; }
            public ObservableCollection<Student> Students { get; set; }
        }

        public class Student
        {
            public string Name { get; set; }
            public string Group { get; set; }
            public List<int> HoursWorked { get; set; }  // Hours worked for different weeks
            public int CumulativeHours { get; set; }
        }
    }
}