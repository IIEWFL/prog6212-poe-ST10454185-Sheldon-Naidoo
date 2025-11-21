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
    public class ManagerViewModel : ViewModelBase
    {
        // Action delegate to close the Window (for navigation back to Main Menu)
        public Action CloseWindowAction { get; set; }

        private readonly ClaimService claimService;
        private readonly RelayCommand _approveClaimCommand;
        private readonly RelayCommand _rejectClaimCommand;
        private readonly RelayCommand _verifyClaimCommand;

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

        private int? _selectedStatusId;
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

                _approveClaimCommand?.RaiseCanExecuteChanged();
                _rejectClaimCommand?.RaiseCanExecuteChanged();
                _verifyClaimCommand?.RaiseCanExecuteChanged();
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

        // PART 3
        // --- Commands ---
        public ICommand LoadClaimsCommand { get; }
        public ICommand ApproveClaimCommand => _approveClaimCommand;
        public ICommand RejectClaimCommand => _rejectClaimCommand;
        public ICommand VerifyClaimCommand => _verifyClaimCommand;
        public ICommand GoHomeCommand { get; }

        // PART 3
        // Constructor
        public ManagerViewModel()
        {
            claimService = new ClaimService();
            _allClaims = new ObservableCollection<Claims>();
            _filteredClaims = new ObservableCollection<Claims>();

            // Initialize commands
            LoadClaimsCommand = new RelayCommand(async _ => await LoadAllClaimsAsync());

            // Approve/Reject commands can only execute if a claim is selected AND it is currently pending review (StatusID = 3)
            _approveClaimCommand = new RelayCommand(async _ => await UpdateClaimStatusAsync(4, "Approved"),
                _ => SelectedClaim != null && SelectedClaim.StatusID == 3);

            _rejectClaimCommand = new RelayCommand(async _ => await UpdateClaimStatusAsync(2, "Rejected"),
                _ => SelectedClaim != null && SelectedClaim.StatusID == 3);

            // Verify command can run if a claim is selected
            _verifyClaimCommand = new RelayCommand(async _ => await ExecuteVerifyClaimAsync(),
                _ => SelectedClaim != null);

            GoHomeCommand = new RelayCommand(_ => CloseView());

            // Load claims on startup
            Task.Run(LoadAllClaimsAsync);
        }

        // --- Core Methods ---

        private void ExecuteGoHome()
        {
            CloseWindowAction?.Invoke();
        }

        // Loads all claims from the service (or claims relevant to the manager's role)
        private async Task LoadAllClaimsAsync()
        {
            try
            {
                // In a real application, this would fetch claims based on the manager's department/role.
                // For now, we fetch ALL claims.
                var claimsList = await claimService.GetAllClaims();

                // Fetch lecturer names for display in the grid
                foreach (var claim in claimsList)
                {
                    // Assuming you have a GetLecturerById method
                    var lecturer = await claimService.GetLecturerById(claim.LecturerID);
                    // Add a property to Claims model (e.g., LecturerFullName) for this to work in XAML
                    // claim.LecturerFullName = lecturer?.FullName ?? "Unknown Lecturer"; 

                    // Recalculate Total Amount (Important for consistency)
                    decimal rate = lecturer?.HourlyRate ?? 500.00m;
                    claim.TotalAmount = (await claimService.GetHoursWorkedByClaim(claim.ClaimID)).Sum(h => (decimal)h.Hours) * rate;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllClaims = new ObservableCollection<Claims>(claimsList);
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading all claims: {ex.Message}", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Filters the claims based on the current search text and status ID
        private void FilterClaims()
        {
            if (AllClaims == null) return;

            var filtered = AllClaims.AsEnumerable();

            // 1. Filter by Status
            if (SelectedStatusId.HasValue && SelectedStatusId.Value > 0)
            {
                filtered = filtered.Where(c => c.StatusID == SelectedStatusId.Value);
            }

            // 2. Filter by Search Text (e.g., Lecturer Name, Claim ID, Month/Year)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    c.ClaimID.ToString().Contains(searchLower) ||
                    // You'll need to update the Claims model to include LecturerFullName
                    // c.LecturerFullName.ToLower().Contains(searchLower) || 
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

        // PART 3
        // Saves the manager's verification notes and status
        private async Task ExecuteVerifyClaimAsync()
        {
            if (SelectedClaim == null) return;

            try
            {
                // Note: SelectedClaim.IsVerified and SelectedClaim.VerificationNotes are bound directly to the UI fields
                bool success = await claimService.UpdateClaimVerificationDetails(
                    SelectedClaim.ClaimID,
                    SelectedClaim.IsVerified,
                    SelectedClaim.VerificationNotes
                );

                if (success)
                {
                    System.Windows.MessageBox.Show($"Verification details for Claim {SelectedClaim.ClaimID} saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An unexpected error occurred during verification: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Updates the status of the selected claim (4 for Approved, 2 for Rejected)
        private async Task UpdateClaimStatusAsync(int newStatusId, string statusName)
        {
            if (SelectedClaim == null) return;

            try
            {
                bool success = await claimService.UpdateClaimStatus(SelectedClaim.ClaimID, newStatusId);

                if (success)
                {
                    System.Windows.MessageBox.Show($"Claim {SelectedClaim.ClaimID} has been successfully {statusName}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Force UI update and reload the list
                    SelectedClaim.StatusID = newStatusId;
                    SelectedClaim.StatusName = await claimService.GetStatusNameById(newStatusId);

                    await LoadAllClaimsAsync();

                    // Clear the selection after action
                    SelectedClaim = null;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Failed to {statusName.ToLower()} claim {SelectedClaim.ClaimID}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
