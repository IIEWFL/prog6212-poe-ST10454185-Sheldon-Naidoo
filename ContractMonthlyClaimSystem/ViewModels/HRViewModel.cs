using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ContractMonthlyClaimSystem.ViewModels
{
    // HR is responsible for final payment processing of approved claims.
    public class HRViewModel : ViewModelBase
    {
        public Action CloseWindowAction { get; set; }

        private readonly ClaimService claimService;
        private readonly RelayCommand _processPaymentCommand;

        // NEW: Property to hold available statuses for the ComboBox
        private List<ClaimStatus> _availableStatuses;
        public List<ClaimStatus> AvailableStatuses
        {
            get => _availableStatuses;
            set { _availableStatuses = value; OnPropertyChanged(); }
        }

        // All claims loaded for display/filtering
        private ObservableCollection<Claims> _allClaims;
        public ObservableCollection<Claims> AllClaims
        {
            get => _allClaims;
            set
            {
                _allClaims = value;
                OnPropertyChanged();
                // Automatically update the filtered list when the main list changes
                FilterClaims();
            }
        }

        // Claims currently displayed after filtering/search
        private ObservableCollection<Claims> _filteredClaims;
        public ObservableCollection<Claims> FilteredClaims
        {
            get => _filteredClaims;
            set
            {
                _filteredClaims = value;
                OnPropertyChanged();
            }
        }

        // --- Filtering/Search Properties ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterClaims(); // Filter claims whenever search text changes
            }
        }

        // The default view for HR should only show APPROVED claims (Status ID 4)
        private int? _selectedStatusId = 4;
        public int? SelectedStatusId
        {
            get => _selectedStatusId;
            set
            {
                _selectedStatusId = value;
                OnPropertyChanged();
                FilterClaims(); // Filter claims whenever status changes
            }
        }

        // --- Detail View Properties ---
        private Claims _selectedClaim;
        public Claims SelectedClaim
        {
            get => _selectedClaim;
            set
            {
                _selectedClaim = value;
                OnPropertyChanged();

                // Load details when a claim is selected
                if (value != null)
                {
                    Task.Run(LoadClaimDetailsAsync);
                }
                else
                {
                    // Clear details if selection is cleared
                    ClaimHours = null;
                    ClaimDocuments = null;
                }

                _processPaymentCommand?.RaiseCanExecuteChanged();
            }
        }

        private List<HoursWorked> _claimHours;
        public List<HoursWorked> ClaimHours
        {
            get => _claimHours;
            set { _claimHours = value; OnPropertyChanged(); }
        }

        private List<SupportingDocument> _claimDocuments;
        public List<SupportingDocument> ClaimDocuments
        {
            get => _claimDocuments;
            set { _claimDocuments = value; OnPropertyChanged(); }
        }

        // --- Commands ---
        public ICommand LoadClaimsCommand { get; }
        public ICommand ProcessPaymentCommand => _processPaymentCommand;
        public ICommand GoHomeCommand { get; }

        // Constructor
        public HRViewModel()
        {
            claimService = new ClaimService();
            _allClaims = new ObservableCollection<Claims>();
            _filteredClaims = new ObservableCollection<Claims>();

            // Initialize commands
            LoadClaimsCommand = new RelayCommand(async _ => await LoadAllClaimsAsync());
            GoHomeCommand = new RelayCommand(_ => CloseWindowAction?.Invoke());

            // Process Payment command can only execute if a claim is selected AND it is currently Approved (StatusID = 4)
            // It changes the status to 5 (Completed/Paid)
            _processPaymentCommand = new RelayCommand(
                async _ => await UpdateClaimStatusAsync(5, "Completed/Paid"),
                _ => SelectedClaim != null && SelectedClaim.StatusID == 4);

            // Load claims and statuses on startup
            Task.Run(async () =>
            {
                await LoadStatusesAsync(); // Load statuses first
                await LoadAllClaimsAsync(); // Then load claims
            });
        }

        // --- Core Methods ---

        // NEW: Method to load statuses
        private async Task LoadStatusesAsync()
        {
            try
            {
                var statuses = await claimService.GetAllStatuses();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableStatuses = statuses;
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading statuses: {ex.Message}", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Loads all claims from the service (or claims relevant to the HR role)
        private async Task LoadAllClaimsAsync()
        {
            try
            {
                // Fetch claims relevant to HR (e.g., all claims that are currently StatusID 4 - Approved)
                var claimsList = await claimService.GetAllClaims();

                foreach (var claim in claimsList)
                {
                    // Fetch lecturer details for calculating total amount
                    var lecturer = await claimService.GetLecturerById(claim.LecturerID);
                    decimal rate = lecturer?.HourlyRate ?? 500.00m;

                    // Recalculate Total Amount 
                    claim.TotalAmount = (await claimService.GetHoursWorkedByClaim(claim.ClaimID)).Sum(h => (decimal)h.Hours) * rate;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllClaims = new ObservableCollection<Claims>(claimsList);
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading all claims for HR: {ex.Message}", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Filters the claims based on the current search text and status ID
        private void FilterClaims()
        {
            if (AllClaims == null) return;

            var filtered = AllClaims.AsEnumerable();

            // 1. Filter by Status (Defaults to 4: Approved)
            if (SelectedStatusId.HasValue && SelectedStatusId.Value > 0)
            {
                filtered = filtered.Where(c => c.StatusID == SelectedStatusId.Value);
            }

            // 2. Filter by Search Text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    c.ClaimID.ToString().Contains(searchLower) ||
                    c.Month.ToString().Contains(searchLower) ||
                    c.Year.ToString().Contains(searchLower)
                );
            }

            FilteredClaims = new ObservableCollection<Claims>(filtered);
        }

        // Loads the detailed hours and documents for the selected claim
        private async Task LoadClaimDetailsAsync()
        {
            if (SelectedClaim == null) return;

            try
            {
                var hours = await claimService.GetHoursWorkedByClaim(SelectedClaim.ClaimID);
                var docs = await claimService.GetDocumentsByClaim(SelectedClaim.ClaimID);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ClaimHours = hours;
                    ClaimDocuments = docs;
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading claim details: {ex.Message}", "Detail Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Updates the status of the selected claim (5 for Completed/Paid)
        private async Task UpdateClaimStatusAsync(int newStatusId, string statusName)
        {
            if (SelectedClaim == null) return;

            // Optional: Add a confirmation dialog for payment processing here
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to mark Claim {SelectedClaim.ClaimID} as '{statusName}' (Payment Processed)? This action cannot be undone.",
                "Confirm Payment Process",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;


            try
            {
                // This is the core automation step: updating the final status
                bool success = await claimService.UpdateClaimStatus(SelectedClaim.ClaimID, newStatusId);

                if (success)
                {
                    System.Windows.MessageBox.Show($"Claim {SelectedClaim.ClaimID} payment has been successfully processed and marked as {statusName}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Force UI update and reload the list to remove the processed item
                    SelectedClaim.StatusID = newStatusId;
                    SelectedClaim.StatusName = await claimService.GetStatusNameById(newStatusId);

                    // Reload the full list to update the FilteredClaims (which typically only shows status 4)
                    await LoadAllClaimsAsync();

                    // Clear the selection after action
                    SelectedClaim = null;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Failed to mark claim {SelectedClaim.ClaimID} as {statusName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
