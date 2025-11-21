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
    // PART 3: NEW Code
    public class ManagerViewModel : ViewModelBase
    {
        // Code Attribution
        // This method was adapted from Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged?view=net-9.0
        // Microsoft Learn

        // Action delegate to close the Window
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
                // Automatically updates the filtered list when the main list changes
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

        // Filtering/Search Properties
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

        // Detail View Properties

        private Claims _selectedClaim;
        public Claims SelectedClaim
        {
            get => _selectedClaim;
            set
            {
                _selectedClaim = value;
                OnPropertyChanged();
                // Loads details when a claim is selected
                if (value != null)
                {
                    Task.Run(LoadClaimDetailsAsync);
                }
                else
                {
                    // Clears details if selection is cleared
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

        // PART 3: NEW Code
        // Command Properties

        // Code Attribution
        // This method was adapted from Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.icommand?view=net-9.0
        // Microsoft Learn

        public ICommand LoadClaimsCommand { get; }
        public ICommand ApproveClaimCommand => _approveClaimCommand;
        public ICommand RejectClaimCommand => _rejectClaimCommand;
        public ICommand VerifyClaimCommand => _verifyClaimCommand;
        public ICommand GoHomeCommand { get; }

        // PART 3
        // Constructor
        public ManagerViewModel()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/relaycommand
            // Microsoft Learn

            claimService = new ClaimService();
            _allClaims = new ObservableCollection<Claims>();
            _filteredClaims = new ObservableCollection<Claims>();

            // Initializes commands
            LoadClaimsCommand = new RelayCommand(async _ => await LoadAllClaimsAsync());

            // Approve/Reject commands can only execute if a claim is selected AND it is currently pending review
            _approveClaimCommand = new RelayCommand(async _ => await UpdateClaimStatusAsync(4, "Approved"),
                _ => SelectedClaim != null && SelectedClaim.StatusID == 3);

            _rejectClaimCommand = new RelayCommand(async _ => await UpdateClaimStatusAsync(2, "Rejected"),
                _ => SelectedClaim != null && SelectedClaim.StatusID == 3);

            // Verifies commands run if a claim is selected
            _verifyClaimCommand = new RelayCommand(async _ => await ExecuteVerifyClaimAsync(),
                _ => SelectedClaim != null);

            GoHomeCommand = new RelayCommand(_ => CloseView());

            // Loads claims on startup
            Task.Run(LoadAllClaimsAsync);
        }

        // Core Methods

        private void ExecuteGoHome()
        {
            CloseWindowAction?.Invoke();
        }

        // PART 3: NEW Code
        // Loads all claims from the service
        private async Task LoadAllClaimsAsync()
        {
            try
            {
                var claimsList = await claimService.GetAllClaims();

                // Fetches lecturer names for display in the grid
                foreach (var claim in claimsList)
                {
                    var lecturer = await claimService.GetLecturerById(claim.LecturerID);

                    // Recalculates Total Amount
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

        // PART 3: NEW Code
        // Filters the claims based on the current search text and status ID
        private void FilterClaims()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/linq/standard-query-operators/filtering-data
            // Microsoft Learn

            if (AllClaims == null) return;

            var filtered = AllClaims.AsEnumerable();

            // Filters by Status
            if (SelectedStatusId.HasValue && SelectedStatusId.Value > 0)
            {
                filtered = filtered.Where(c => c.StatusID == SelectedStatusId.Value);
            }

            // Filters by Search Text
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

        // PART 3: NEW Code
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

        // PART 3: NEW Code
        // Saves the manager's verification notes and status
        private async Task ExecuteVerifyClaimAsync()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool
            // Microsoft Learn

            if (SelectedClaim == null) return;

            try
            {
                // SelectedClaim.IsVerified and SelectedClaim.VerificationNotes are bound directly to the UI fields
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

        // PART 3: NEW Code
        // Updates the status of the selected claim (4 for Approved, 2 for Rejected)
        private async Task UpdateClaimStatusAsync(int newStatusId, string statusName)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool
            // Microsoft Learn

            if (SelectedClaim == null) return;

            try
            {
                bool success = await claimService.UpdateClaimStatus(SelectedClaim.ClaimID, newStatusId);

                if (success)
                {
                    System.Windows.MessageBox.Show($"Claim {SelectedClaim.ClaimID} has been successfully {statusName}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Forces UI update and reload the list
                    SelectedClaim.StatusID = newStatusId;
                    SelectedClaim.StatusName = await claimService.GetStatusNameById(newStatusId);

                    await LoadAllClaimsAsync();

                    // Clears the selection after action
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
