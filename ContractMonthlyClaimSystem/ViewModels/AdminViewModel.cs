using ContractMonthlyClaimSystem.Common;
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


namespace ContractMonthlyClaimSystem.ViewModels
{
    public class AdminViewModel : ViewModelBase
    {
        // Code Attribution
        // This method was adapted from Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        // Microsoft Learn

        // Part 2: NEW Code
        // Action to signal the View to close the Window (AdminView)
        public Action CloseWindowAction { get; set; }

        private readonly ClaimService claimService;
        private ObservableCollection<Claims> pendingClaims;

        public ObservableCollection<Claims> PendingClaims
        {
            get => pendingClaims;
            set
            {
                pendingClaims = value;
                OnPropertyChanged();        // Notifies the UI to update when the value changes
            }
        }

        private Claims selectedClaim;

        // Part 2: UPDATED Code
        // When a new claim is selected, it asynchronously loads its associated hours and documents.
        public Claims SelectedClaim
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher?view=windowsdesktop-9.0
            // Microsoft Learn

            get => selectedClaim;
            set
            {
                selectedClaim = value;
                OnPropertyChanged();

                // This manually triggers command re-evaluation when selection changes
                (ApproveClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();

                if (value != null)
                {
                    Task.Run(async () =>
                    {
                        var hours = await claimService.GetHoursWorkedByClaim(value.ClaimID);
                        var documents = await claimService.GetDocumentsByClaim(value.ClaimID);

                        // A Dispatcher is used to update ObservableCollections and maintains the prioritized queues of the work items
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SelectedClaimHours = new ObservableCollection<HoursWorked>(hours);
                            SelectedClaimDocuments = new ObservableCollection<SupportingDocument>(documents);
                        });
                    });
                }
                else
                {
                    // Clear details when selection is removed
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedClaimHours = new ObservableCollection<HoursWorked>();
                        SelectedClaimDocuments = new ObservableCollection<SupportingDocument>();
                    });
                }
            }
        }

        // Part 2: UPDATED Code
        // Binds to the GUI
        private ObservableCollection<HoursWorked> selectedClaimHours;
        public ObservableCollection<HoursWorked> SelectedClaimHours
        {
            get => selectedClaimHours;
            set
            {
                selectedClaimHours = value;
                OnPropertyChanged(); 
            }
        }
        private ObservableCollection<SupportingDocument> selectedClaimDocuments;
        public ObservableCollection<SupportingDocument> SelectedClaimDocuments
        {
            get => selectedClaimDocuments;
            set
            {
                selectedClaimDocuments = value;
                OnPropertyChanged(); 
            }
        }

        // Code Attribution
        // This method was adapted from Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.icommand?view=net-9.0
        // Microsoft Learn

        // ICommand properties for UI buttons to approve or reject a claim.
        public ICommand ApproveClaimCommand { get; }
        public ICommand RejectClaimCommand { get; }

        // Part 2: NEW Code
        public ICommand GoHomeCommand { get; }

        // Constructor for ViewModel
        public AdminViewModel()
        {
            claimService = new ClaimService();

            SelectedClaimHours = new ObservableCollection<HoursWorked>();
            SelectedClaimDocuments = new ObservableCollection<SupportingDocument>();

            // Initialize the Approve and Reject commands.
            // The commands will only be executable if a claim is currently selected.
            ApproveClaimCommand = new RelayCommand(async _ => await ApproveClaimAsync(), _ => SelectedClaim != null);
            RejectClaimCommand = new RelayCommand(async _ => await RejectClaimAsync(), _ => SelectedClaim != null);

            // Part 2: NEW Code
            // Command for navigation
            GoHomeCommand = new RelayCommand(_ => ExecuteGoHome());

            // This calls the Messenger method to allow for real-time updates
            Messenger.ClaimSubmitted += OnClaimSubmitted;
            Task.Run(LoadPendingClaimsAsync);
        }

        // Part 2: NEW Code
        // Method that executes the logic in the AdminView for the 'Back to Main Menu' button
        private void ExecuteGoHome()
        {
            // Invokes the Action delegate, which the AdminView will handle by closing itself.
            CloseWindowAction?.Invoke();
        }

        // Part 2: NEW Code
        // A handler method that is triggered when a new claim is submitted by the LecturerView
        private void OnClaimSubmitted(Claims newClaim)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher?view=windowsdesktop-9.0
            // Microsoft Learn

            // Only adds the claim if it is set to "Pending Review" (StatusID = 3)
            if (newClaim.StatusID == 3)
            {
                // Ensures UI update happens on the main thread
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    // Populates lecturer name and month display for the DataGrid
                    var claims = await claimService.GetAllPendingClaims();
                    // Finds the submitted claim in the updated list 
                    var updatedClaim = claims.FirstOrDefault(c => c.ClaimID == newClaim.ClaimID);

                    if (updatedClaim != null)
                    {
                        PendingClaims.Add(updatedClaim);
                    }

                    // Automatically selects the new claim here
                    SelectedClaim = newClaim; 
                });
            }
        }

        // Part 2: UPDATED Code
        // Asynchronous method to load all pending claims from the service.
        private async Task LoadPendingClaimsAsync()
        {
            var claims = await claimService.GetAllPendingClaims();
            Application.Current.Dispatcher.Invoke(() =>
            {
                PendingClaims = new ObservableCollection<Claims>(claims);

                // Clear selection after a refresh (e.g., after approval/rejection)
                if (claims.Count > 0)
                {
                    SelectedClaim = claims[0];
                }
                else
                {
                    SelectedClaim = null;
                }
            });
        }

        // Part 2: UPDATED Code
        // Asynchronous method to approve the selected claim.
        private async Task ApproveClaimAsync()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.messagebox?view=windowsdesktop-9.0
            // Microsoft Learn

            if (SelectedClaim != null)
            {
                var claimToUpdate = SelectedClaim;
                await claimService.UpdateClaimStatus(SelectedClaim.ClaimID, "Approved");
                System.Windows.MessageBox.Show($"Claim ID {SelectedClaim.ClaimID} approved successfully.", "Claim Action", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                Application.Current.Dispatcher.Invoke(() => PendingClaims.Remove(claimToUpdate));
                SelectedClaim = PendingClaims.FirstOrDefault();
                await LoadPendingClaimsAsync();
            }
        }

        // Part 2: UPDATED Code
        // Asynchronous method to reject the selected claim.
        private async Task RejectClaimAsync()
        {
            if (SelectedClaim != null)
            {
                var claimToUpdate = SelectedClaim;
                await claimService.UpdateClaimStatus(SelectedClaim.ClaimID, "Rejected");
                System.Windows.MessageBox.Show($"Claim ID {SelectedClaim.ClaimID} rejected successfully.", "Claim Action", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                Application.Current.Dispatcher.Invoke(() => PendingClaims.Remove(claimToUpdate));
                SelectedClaim = PendingClaims.FirstOrDefault();

                await LoadPendingClaimsAsync();
            }
        }

        // Part 2: NEW Code
        ~AdminViewModel()
        {
            // Used to exit the Messenger method
            Messenger.ClaimSubmitted -= OnClaimSubmitted;
        }
    }
}
