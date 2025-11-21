using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class LecturerViewModel : ViewModelBase
    {
        // Code Attribution
        // This method was adapted from Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        // Microsoft Learn

        // Part 2: NEW Code
        // Action delegate to close the Window in the LecturerView
        public Action CloseWindowAction { get; set; }

        private readonly ClaimService claimService;

        // NEW
        // --- Lecturer Data ---
        private Lecturer _currentLecturer;
        public Lecturer CurrentLecturer
        {
            get => _currentLecturer;
            set
            {
                _currentLecturer = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Claims> claims;
        public ObservableCollection<Claims> Claims
        {
            get => claims;
            set
            {
                claims = value;
                OnPropertyChanged();
            }
        }

        // NEW
        // --- New Claim Submission Data ---

        // NEW: Uses a list of DTOs (HoursWorkedSubmission) instead of the database model (HoursWorked)
        public ObservableCollection<HoursWorkedSubmission> CurrentHours { get; set; } = new ObservableCollection<HoursWorkedSubmission>();
        public ObservableCollection<SupportingDocument> CurrentDocuments { get; set; } = new ObservableCollection<SupportingDocument>();

        // NEW: Property to bind to the fields for adding a new single entry
        private HoursWorkedSubmission _newHourEntry = new HoursWorkedSubmission { DateWorked = DateTime.Today, Hours = 1.0, Description = "" };
        public HoursWorkedSubmission NewHourEntry
        {
            get => _newHourEntry;
            set
            {
                _newHourEntry = value;
                OnPropertyChanged();
            }
        }

        public int SelectedMonth { get; set; } = DateTime.Now.Month;
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        // ICommand properties for UI buttons. These are bound to methods that handle user actions.
        public ICommand AddHoursCommand { get; }
        public ICommand RemoveHoursCommand { get; } // NEW
        public ICommand SubmitClaimCommand { get; }
        public ICommand LoadClaimsCommand { get; }

        // Part 2: NEW Code
        // New commands properties for uploading documents and the main menu button
        public ICommand UploadDocumentCommand { get; }
        public ICommand GoHomeCommand { get; }

        // Part 2: UPDATED Code
        // Constructor for ViewModel
        public LecturerViewModel()
        {
            // Initialize services and collections
            claimService = new ClaimService();
            Claims = new ObservableCollection<Claims>();

            // Initialization for observable collections
            CurrentHours = new ObservableCollection<HoursWorkedSubmission>();
            CurrentDocuments = new ObservableCollection<SupportingDocument>();
            NewHourEntry = new HoursWorkedSubmission { DateWorked = DateTime.Today, Hours = 1.0, Description = "" };

            // Initialize the commands, linking them to their respective methods.
            AddHoursCommand = new RelayCommand(_ => AddHourEntry(), _ => CanAddHourEntry());
            RemoveHoursCommand = new RelayCommand(RemoveHourEntry, _ => CurrentHours.Any());
            SubmitClaimCommand = new RelayCommand(async _ => await SubmitClaimAsync(), _ => CanSubmitClaim());
            LoadClaimsCommand = new RelayCommand(_ => Task.Run(LoadClaimsAsync));

            // Initialized command for uploading documents
            UploadDocumentCommand = new RelayCommand(_ => UploadDocument());

            // Initialized command for navigation
            GoHomeCommand = new RelayCommand(_ => ExecuteGoHome());

            Task.Run(LoadClaimsAsync);
        }

        // Part 2: NEW Code
        // Method that executes the logic for the "Back to Main Menu" button.
        private void ExecuteGoHome()
        {
            // Invokes the Action delegate, which the LecturerView will handle by closing itself.
            CloseWindowAction?.Invoke();
        }

        // Part 2: UPDATED Code
        // Asynchronous method to load claims from the ClaimService.
        private async Task LoadClaimsAsync()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher?view=windowsdesktop-9.0
            // Microsoft Learn

            try
            {
                // 1. Get Lecturer Data (Crucial for Hourly Rate)
                // Assuming lecturer ID 1 for mock purposes, but this should ideally come from an authentication service.
                CurrentLecturer = await claimService.GetCurrentLecturer();

                // 2. Load Claim History
                var claimsList = await claimService.GetClaimsByLecturer(CurrentLecturer?.LecturerID ?? 1);

                // Loops through the claims to calculate and set the total amount for each.
                foreach (var claim in claimsList)
                {
                    // Use the actual hourly rate if available, otherwise default to the hardcoded rate (500.00m)
                    decimal rate = CurrentLecturer?.HourlyRate ?? 500.00m;
                    claim.TotalAmount = (await claimService.GetHoursWorkedByClaim(claim.ClaimID)).Sum(h => (decimal)h.Hours) * rate;
                }

                // Ensures UI update happens on the main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Claims = new ObservableCollection<Claims>(claimsList);
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading data: {ex.Message}", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Part 3: UPDATED Code
        // NEW: Method to add a single hours-worked entry based on NewHourEntry fields.
        private void AddHourEntry()
        {
            // Simple validation check before adding
            if (NewHourEntry.Hours <= 0 || string.IsNullOrWhiteSpace(NewHourEntry.Description))
            {
                MessageBox.Show("Please ensure hours are greater than zero and a description is provided.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a copy of the entry to add (to prevent modifying the item in the list when updating NewHourEntry)
            var hourToAdd = new HoursWorkedSubmission
            {
                DateWorked = NewHourEntry.DateWorked,
                Hours = NewHourEntry.Hours,
                Description = NewHourEntry.Description
            };

            CurrentHours.Add(hourToAdd);

            // Recalculates total amount when hours are added
            CalculateTotalAmount();

            // Reset the input fields for the next entry
            NewHourEntry = new HoursWorkedSubmission { DateWorked = DateTime.Today, Hours = 1.0, Description = "" };
        }

        // NEW: Logic to determine if an hour entry can be added (optional)
        private bool CanAddHourEntry()
        {
            // Perform basic checks on the input fields for NewHourEntry here if desired
            return true;
        }

        // NEW: Method to remove a selected hours-worked entry.
        private void RemoveHourEntry(object parameter)
        {
            if (parameter is HoursWorkedSubmission entryToRemove)
            {
                CurrentHours.Remove(entryToRemove);
                CalculateTotalAmount(); // Recalculate after removal
            }
        }

        // Part 3: NEW Code
        // Helper method to recalculate total amount
        private void CalculateTotalAmount()
        {
            // Use the lecturer's actual hourly rate from the loaded model
            decimal rate = CurrentLecturer?.HourlyRate ?? 500.00m;

            // Sum all hours in the submission list
            TotalAmount = CurrentHours.Sum(h => (decimal)h.Hours) * rate;
        }

        // Part 2: NEW Code
        // Method that uses dummy data for uploading a document
        private void UploadDocument()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.messagebox?view=windowsdesktop-9.0
            // Microsoft Learn

            // Mock file properties
            string simulatedFileName = $"Support_Doc_{CurrentDocuments.Count + 1}_{DateTime.Now.Ticks % 100}.pdf";
            long simulatedFileSize = 1 * 1024 * 1024;

            const long MAX_SIZE_BYTES = 5 * 1024 * 1024;
            string fileExtension = System.IO.Path.GetExtension(simulatedFileName).ToLower();
            string[] allowedExtensions = { ".pdf", ".docx", ".xlsx" };

            if (simulatedFileSize > MAX_SIZE_BYTES)
            {
                System.Windows.MessageBox.Show("File size exceeds the 5MB limit. Please upload a smaller file.", "Upload Error");
                return;
            }

            if (!allowedExtensions.Contains(fileExtension))
            {
                System.Windows.MessageBox.Show("Unsupported file type. Please use PDF, DOCX, or XLSX.", "Upload Error");
                return;
            }

            CurrentDocuments.Add(new SupportingDocument
            {
                FileName = simulatedFileName,
                FilePath = $"/server/claims/user1/{simulatedFileName}"
            });
        }


        // Part 3: UPDATED Code
        // Asynchronous method to handle the submission of a new claim.
        private async Task SubmitClaimAsync()
        {
            if (!CanSubmitClaim()) return;

            try
            {
                // Convert DTOs back to the database model (HoursWorked) for the service layer
                var hoursWorkedList = CurrentHours.Select(h => new HoursWorked
                {
                    DateWorked = h.DateWorked,
                    Hours = h.Hours,
                    Description = h.Description,
                }).ToList();

                var newClaim = new Claims
                {
                    LecturerID = CurrentLecturer.LecturerID, // Use actual Lecturer ID
                    Month = SelectedMonth,
                    Year = SelectedYear,
                    SubmissionDate = DateTime.Now,
                    StatusID = 1
                };

                // Expects an integer from the service instead of a boolean.
                int newClaimId = await claimService.SubmitNewClaim(newClaim, hoursWorkedList, CurrentDocuments.ToList());

                if (newClaimId > 0)
                {
                    System.Windows.MessageBox.Show("Claim submitted successfully for review!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                    // Clear submission data and reset total
                    CurrentHours.Clear();
                    CurrentDocuments.Clear();
                    TotalAmount = 0.00m;

                    // Reload the history to show the newly submitted claim
                    await LoadClaimsAsync();
                }
                else
                {
                    // Failure case if the service returned 0
                    System.Windows.MessageBox.Show("Failed to submit claim. Please ensure all required data is correct.", "Submission Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An unexpected error occurred during submission: {ex.Message}", "Critical Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // PART 3
        // NEW: Logic to check if submission is possible
        private bool CanSubmitClaim()
        {
            bool hasHours = CurrentHours.Any() && CurrentHours.All(h => h.Hours > 0 && !string.IsNullOrWhiteSpace(h.Description));
            bool hasLecturer = CurrentLecturer != null;

            if (!hasLecturer)
            {
                // Optionally show a message that lecturer data failed to load
            }
            return hasHours && hasLecturer;
        }
    }

    // NEW CLASS: Data Transfer Object (DTO) for collecting hour submissions, decoupled from the DB model.
    // It also inherits from ViewModelBase to enable two-way binding and validation feedback in the UI.
    public class HoursWorkedSubmission : ViewModelBase
    {
        private DateTime _dateWorked = DateTime.Today;

        [Required(ErrorMessage = "Date is required.")]
        public DateTime DateWorked
        {
            get => _dateWorked;
            set
            {               
                _dateWorked = value;
                OnPropertyChanged();
            }
        }

        private double _hours;
        [Required(ErrorMessage = "Hours are required.")]
        [Range(0.1, 24.0, ErrorMessage = "Hours must be greater than zero.")]
        public double Hours
        {
            get => _hours;
            set
            {
                _hours = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(255)]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }
}
