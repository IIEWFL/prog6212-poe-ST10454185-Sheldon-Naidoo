using ContractMonthlyClaimSystem.Common;
using ContractMonthlyClaimSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContractMonthlyClaimSystem.Services
{
    public class ClaimService
    {
        // Private fields that serve as in-memory data repositories.
        private readonly List<Lecturer> lecturersList = new List<Lecturer>();
        private readonly List<Claims> claimsList = new List<Claims>();
        private readonly List<ClaimStatus> claimStatusList = new List<ClaimStatus>();
        private readonly List<HoursWorked> hoursWorkedList = new List<HoursWorked>();
        private readonly List<SupportingDocument> documentsList = new List<SupportingDocument>();

        // Constructor for mock data
        private readonly int currentLoggedInLecturerID = 1;

        public ClaimService()
        {
            lecturersList.Add(new Lecturer { LecturerID = 1, FirstName = "Steven", LastName = "Pro", HourlyRate = 500.00m, Role = "Lecturer", Email = "steven.pro@uni.ac.za" });
            lecturersList.Add(new Lecturer { LecturerID = 2, FirstName = "Alice", LastName = "Smith", HourlyRate = 450.00m, Role = "Lecturer", Email = "alice.smith@uni.ac.za" });
            lecturersList.Add(new Lecturer { LecturerID = 10, FirstName = "Admin", LastName = "User", HourlyRate = 0.00m, Role = "Coordinator", Email = "pc.coord@uni.ac.za" });
            lecturersList.Add(new Lecturer { LecturerID = 11, FirstName = "HR", LastName = "Manager", HourlyRate = 0.00m, Role = "HR", Email = "hr.manager@uni.ac.za" });


            claimStatusList.Add(new ClaimStatus { StatusID = 1, StatusName = "Submitted" });
            claimStatusList.Add(new ClaimStatus { StatusID = 2, StatusName = "Rejected" });
            claimStatusList.Add(new ClaimStatus { StatusID = 3, StatusName = "Pending Review" });
            claimStatusList.Add(new ClaimStatus { StatusID = 4, StatusName = "Approved" });

            // Part 2: NEW Code
            SetupMockClaims();
        }

        // Part 2: NEW Code
        private void SetupMockClaims()
        {
            // Code Attribution
            // This method was adapted from Dev
            // https://dev.to/dianaiminza/creating-mock-data-in-net-with-bogus-5bah
            // Captain Iminza
            // https://dev.to/dianaiminza

            // --- First Claim: Pending Review ---
            var claim1 = new Claims
            {
                ClaimID = 1,
                LecturerID = 1,
                Month = 10,
                Year = 2025,
                SubmissionDate = DateTime.Now.AddDays(-5),
                StatusID = 3,
                TotalAmount = 5000.00m
            };
            claimsList.Add(claim1);

            hoursWorkedList.Add(new HoursWorked { HoursWorkedID = 101, ClaimID = 1, DateWorked = new DateTime(2025, 10, 1), Hours = 10, Description = "Lecture - C# Programming" });
            hoursWorkedList.Add(new HoursWorked { HoursWorkedID = 102, ClaimID = 1, DateWorked = new DateTime(2025, 10, 2), Hours = 5, Description = "Marking - Exam Papers" });
            documentsList.Add(new SupportingDocument { DocumentID = 1, ClaimID = 1, FileName = "Attendance_Oct_1.pdf", FilePath = "fake/path" });
            documentsList.Add(new SupportingDocument { DocumentID = 2, ClaimID = 1, FileName = "Teaching_Log_Oct.docx", FilePath = "fake/path" });

            // --- Second Claim: Approved ---
            var claim2 = new Claims
            {
                ClaimID = 2,
                LecturerID = 2,
                Month = 9,
                Year = 2025,
                SubmissionDate = DateTime.Now.AddDays(-30),
                StatusID = 4,
                TotalAmount = 9000.00m
            };
            claimsList.Add(claim2);

            // Hours for claim 2
            hoursWorkedList.Add(new HoursWorked { HoursWorkedID = 103, ClaimID = 2, DateWorked = new DateTime(2025, 9, 15), Hours = 15, Description = "Seminar Prep" });
            hoursWorkedList.Add(new HoursWorked { HoursWorkedID = 104, ClaimID = 2, DateWorked = new DateTime(2025, 9, 20), Hours = 5, Description = "Student Consultation" });

            // --- Third Claim (Lecturer 1): Rejected ---
            var claim3 = new Claims
            {
                ClaimID = 3,
                LecturerID = 1,
                Month = 8,
                Year = 2025,
                SubmissionDate = DateTime.Now.AddDays(-60),
                StatusID = 2,
                TotalAmount = 4000.00m
            };
            claimsList.Add(claim3);
        }

        // Part 3: NEW Code
        // Method to retrieve the details of the currently logged-in lecturer.
        public async Task<Lecturer> GetCurrentLecturer()
        {
            return await Task.FromResult(lecturersList.FirstOrDefault(l => l.LecturerID == currentLoggedInLecturerID));
        }

        // Part 2
        // Helper method to get status ID consistently
        private int GetStatusIdByName(string statusName)
        {
            // Default to 1 (Submitted) if not found
            return claimStatusList.FirstOrDefault(s => s.StatusName.Equals(statusName, StringComparison.OrdinalIgnoreCase))?.StatusID ?? 1;
        }

        // Part 3: UPDATE CODE
        // Method to retrieve all claims for a specific lecturer
        public async Task<List<Claims>> GetClaimsByLecturer(int v)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.fromresult?view=net-9.0
            // Microsoft Learn

            // Use the mock logged-in ID
            var lecturerClaims = claimsList.Where(c => c.LecturerID == currentLoggedInLecturerID).ToList();

            // Enhance claims with display data
            var lecturer = lecturersList.FirstOrDefault(l => l.LecturerID == currentLoggedInLecturerID);
            var lecturerName = $"{lecturer?.FirstName} {lecturer?.LastName}";

            foreach (var claim in lecturerClaims)
            {
                claim.LecturerName = lecturerName;
                claim.MonthYearDisplay = new DateTime(claim.Year, claim.Month, 1).ToString("MMMM yyyy");
                claim.StatusName = await GetStatusNameById(claim.StatusID);
            }

            return await Task.FromResult(lecturerClaims);
        }

        // Part 2: UPDATED Code
        // Method to retrieve all claims for pending review statuses
        public async Task<List<Claims>> GetAllPendingClaims()
        {
            // Gets claims with StatusID = 3 (Pending Review)
            var pendingClaims = claimsList.Where(c => c.StatusID == 3).ToList();

            // Populate LecturerName and MonthYearDisplay for AdminViewModel's DataGrid
            foreach (var claim in pendingClaims)
            {
                var lecturer = lecturersList.FirstOrDefault(l => l.LecturerID == claim.LecturerID);
                claim.LecturerName = $"{lecturer?.FirstName} {lecturer?.LastName}";
                claim.MonthYearDisplay = new DateTime(claim.Year, claim.Month, 1).ToString("MMMM yyyy");
                claim.StatusName = await GetStatusNameById(claim.StatusID);
            }

            return await Task.FromResult(pendingClaims);
        }

        // Part 2: UPDATED Code
        // Method that allows for submission of new claim along with its associated hours worked and documents.
        public async Task<int> SubmitNewClaim(Claims newClaim, List<HoursWorked> hours, List<SupportingDocument> documents)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements
            // Microsoft Learn

            newClaim.ClaimID = claimsList.Count > 0 ? claimsList.Max(c => c.ClaimID) + 1 : 1;
            newClaim.SubmissionDate = DateTime.Now;
            newClaim.StatusID = GetStatusIdByName("Pending Review");

            var lecturer = lecturersList.FirstOrDefault(l => l.LecturerID == newClaim.LecturerID);

            // Validation: Lecturer must exist
            if (lecturer == null)
            {
                throw new InvalidOperationException($"Lecturer ID {newClaim.LecturerID} not found.");
            }

            // Part 3: UPDATE CODE
            // Automation
            newClaim.TotalAmount = hours.Sum(h => (decimal)h.Hours) * lecturer.HourlyRate;

            claimsList.Add(newClaim);

            // Link hours and documents to the new claim ID
            hours.ForEach(h => {
                h.ClaimID = newClaim.ClaimID;
                // Assign new ID to HoursWorked item for uniqueness (WPF scenario)
                h.HoursWorkedID = hoursWorkedList.Count > 0 ? hoursWorkedList.Max(hw => hw.HoursWorkedID) + 1 : 1;
                hoursWorkedList.Add(h);
            });
            documents.ForEach(d => {
                d.ClaimID = newClaim.ClaimID;
                d.DocumentID = documentsList.Count > 0 ? documentsList.Max(doc => doc.DocumentID) + 1 : 1;
                documentsList.Add(d);
            });

            Messenger.PublishClaimSubmitted(newClaim); 

            return await Task.FromResult(newClaim.ClaimID);
        }

        // --- Manager/Coordinator Methods ---

        // NEW: Method to retrieve ALL claims, used by the Manager View
        public async Task<List<Claims>> GetAllClaims()
        {
            var allClaims = claimsList.ToList();

            // Enhance claims with display data needed by the ManagerViewModel
            foreach (var claim in allClaims)
            {
                var lecturer = lecturersList.FirstOrDefault(l => l.LecturerID == claim.LecturerID);
                if (lecturer != null)
                {
                    claim.LecturerName = $"{lecturer.FirstName} {lecturer.LastName}";
                    claim.LecturerEmail = lecturer.Email; // Populate the new property
                }
                claim.MonthYearDisplay = new DateTime(claim.Year, claim.Month, 1).ToString("MMMM yyyy");
                claim.StatusName = await GetStatusNameById(claim.StatusID);

                // Recalculate Total Amount just in case it was missed during submission
                decimal rate = lecturer?.HourlyRate ?? 0.00m;
                claim.TotalAmount = hoursWorkedList.Where(h => h.ClaimID == claim.ClaimID).Sum(h => (decimal)h.Hours) * rate;
            }

            return await Task.FromResult(allClaims);
        }

        // NEW: Method to retrieve a specific lecturer by ID
        public async Task<Lecturer> GetLecturerById(int lecturerId)
        {
            return await Task.FromResult(lecturersList.FirstOrDefault(l => l.LecturerID == lecturerId));
        }

        // PART 3
        // NEW: Method to update the manager's verification notes and status
        public async Task<bool> UpdateClaimVerificationDetails(int claimId, bool isVerified, string notes)
        {
            var claim = claimsList.FirstOrDefault(c => c.ClaimID == claimId);

            if (claim == null)
            {
                throw new InvalidOperationException($"Claim ID {claimId} not found for verification update.");
            }

            claim.IsVerified = isVerified;
            claim.VerificationNotes = notes;

            return await Task.FromResult(true);
        }

        // OVERLOAD: Method that updates the claim status using the Status ID (preferred for Manager View)
        public async Task<bool> UpdateClaimStatus(int claimId, int newStatusId)
        {
            var claim = claimsList.FirstOrDefault(c => c.ClaimID == claimId);

            if (claim == null)
            {
                throw new InvalidOperationException($"Claim ID {claimId} not found.");
            }

            claim.StatusID = newStatusId;
            return await Task.FromResult(true);
        }

        // Method that updates the claim status
        public async Task<bool> UpdateClaimStatus(int claimId, string newStatusName)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements
            // Microsoft Learn

            var claim = claimsList.FirstOrDefault(c => c.ClaimID == claimId);

            if (claim == null)
            {
                // Error Handling
                throw new InvalidOperationException($"Claim ID {claimId} not found.");
            }

            var newStatusId = GetStatusIdByName(newStatusName);

            if (newStatusId == 0)
            {
                // Error Handling
                throw new ArgumentException($"Invalid status name: {newStatusName}.");
            }

            claim.StatusID = newStatusId;
            return await Task.FromResult(true);
        }

        // Method to retrieve the hours worked for a specific claim.
        public async Task<List<HoursWorked>> GetHoursWorkedByClaim(int claimId)
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.fromresult?view=net-9.0
            // Microsoft Learn

            return await Task.FromResult(hoursWorkedList.Where(h => h.ClaimID == claimId).ToList());
        }

        // Method to retrieve the supporting documents for a specific claim.
        public async Task<List<SupportingDocument>> GetDocumentsByClaim(int claimId)
        {
            return await Task.FromResult(documentsList.Where(d => d.ClaimID == claimId).ToList());
        }

        // Method to retrieve the status name based on its ID.
        public async Task<string> GetStatusNameById(int statusId)
        {
            return await Task.FromResult(claimStatusList.FirstOrDefault(s => s.StatusID == statusId)?.StatusName);
        }
    }
}
