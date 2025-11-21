using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claims : ViewModelBase
    {
        [Key]
        public int ClaimID { get; set; }

        [Required]
        public int LecturerID { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public int StatusID { get; set; }

        // Part 3: NEW Code
        private bool _isVerified = false;
        public bool IsVerified
        {
            get => _isVerified;
            set
            {
                if (_isVerified != value)
                {
                    _isVerified = value;
                    OnPropertyChanged();
                }
            }
        }
        public string VerificationNotes { get; set; }

        // Part 2: UPDATED Code
        [NotMapped]
        public string LecturerName { get; set; }

        // NEW: Property to support Manager Dashboard detailed view
        [NotMapped]
        public string LecturerEmail { get; set; }

        [NotMapped]
        public string MonthYearDisplay { get; set; }
        [NotMapped]
        private string _statusName;
        [NotMapped]
        public string StatusName
        {
            get => _statusName;
            set
            {
                if (_statusName != value)
                {
                    _statusName = value;
                    OnPropertyChanged(); // Manual INPC call
                }
            }
        }

        private ObservableCollection<HoursWorked> _hoursWorked;
        [NotMapped]
        public ObservableCollection<HoursWorked> HoursWorked
        {
            get => _hoursWorked;
            set
            {
                // Note: ObservableCollection must be used here and must be initialized from the service
                if (_hoursWorked != value)
                {
                    _hoursWorked = value;
                    OnPropertyChanged(); // Manual INPC call
                }
            }
        }

        private ObservableCollection<SupportingDocument> _supportingDocuments;
        [NotMapped]
        public ObservableCollection<SupportingDocument> SupportingDocuments
        {
            get => _supportingDocuments;
            set
            {
                if (_supportingDocuments != value)
                {
                    _supportingDocuments = value;
                    OnPropertyChanged(); // Manual INPC call
                }
            }
        }

        public virtual Lecturer Lecturer { get; set; }
        public virtual ClaimStatus Status { get; set; }
    }
}
