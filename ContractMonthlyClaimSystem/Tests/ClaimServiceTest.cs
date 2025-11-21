using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;

namespace ContractMonthlyClaimSystem.Tests
{
    // Code Attribution
    // This method was adapted from Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.assert?view=mstest-net-4.0
    // Microsoft Learn

    // A class to house all unit tests.
    public class ClaimServiceTest
    {
        private static int passedCount = 0;
        private static int failedCount = 0;

        // --- Core Test Runner and Assertion Helpers ---

        public static void RunAllTests()
        {
            Console.WriteLine("--- Starting ClaimService Simple Unit Tests ---");
            passedCount = 0;
            failedCount = 0;

            // Execute all test methods
            TestRetrievalConsistency();
            TestSubmissionLogic();
            TestStatusUpdateAndErrorHandling();

            Console.WriteLine("\n--- Test Summary ---");
            Console.WriteLine($"Total Tests Run: {passedCount + failedCount}");
            Console.WriteLine($"Tests Passed: {passedCount}");
            Console.WriteLine($"Tests Failed: {failedCount}");
            Console.WriteLine("--------------------");
        }

        // Helper method to run an individual async test method and handle reporting
        private static void RunTest(string testName, Func<Task> testAction)
        {
            try
            {
                // Executes the asynchronous test action synchronously for reporting purposes
                testAction.Invoke().GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] {testName}");
                passedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {testName} - Error: {ex.Message}");
                failedCount++;
            }
        }

        // A basic assertion method for checking expected equality
        private static void AssertAreEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"{message} Expected: '{expected}', Actual: '{actual}'");
            }
        }

        // A basic assertion method for checking truthiness
        private static void AssertIsTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"{message} Expected: True, Actual: False");
            }
        }

        // A basic assertion for checking if an exception is thrown
        private static void AssertThrowsAsync<TException>(Func<Task> action, string message) where TException : Exception
        {
            try
            {
                action.Invoke().GetAwaiter().GetResult();
                throw new Exception($"Expected exception of type {typeof(TException).Name} was not thrown.");
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TException) && (ex.InnerException?.GetType() != typeof(TException)))
                {
                    // Catches cases
                    throw new Exception($"Wrong exception type thrown. Expected {typeof(TException).Name}, but got {ex.GetType().Name}.");
                }
                // Success: The expected exception was caught.
            }
        }

        // --- Test Group 1: Data Retrieval Consistency ---

        private static void TestRetrievalConsistency()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.assert?view=mstest-net-4.0
            // Microsoft Learn

            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code?view=vs-2022
            // Microsoft Learn

            RunTest("GetClaimsByLecturer: Correct Count and Name", async () =>
            {
                var service = new ClaimService();
                var claims = await service.GetClaimsByLecturer(1);

                AssertIsTrue(claims != null, "Claims list should not be null.");
                AssertAreEqual(1, claims.Count, "Expected 1 claim for Lecturer ID 1.");
                AssertAreEqual("Steven Pro", claims.First().LecturerName, "Lecturer name must be correctly enriched.");
            });

            RunTest("GetAllPendingClaims: Returns Only Pending Claims", async () =>
            {
                var service = new ClaimService();
                var pendingClaims = await service.GetAllPendingClaims();

                AssertIsTrue(pendingClaims != null, "Pending claims list should not be null.");
                AssertAreEqual(1, pendingClaims.Count, "Expected 1 claim with 'Pending Review' status (ID 3).");
                AssertAreEqual(3, pendingClaims.First().StatusID, "The retrieved claim must have StatusID 3.");
            });

            RunTest("GetHoursWorkedByClaim: Correct Hours Sum", async () =>
            {
                var service = new ClaimService();
                var hours = await service.GetHoursWorkedByClaim(1);

                AssertAreEqual(2, hours.Count, "Expected 2 hours entries for Claim ID 1.");
                AssertAreEqual(15.0, hours.Sum(h => h.Hours), "Expected total hours to be 15 (10 + 5).");
            });
        }

        // --- Test Group 2: Submission Logic ---

        private static void TestSubmissionLogic()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.assert?view=mstest-net-4.0
            // Microsoft Learn

            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code?view=vs-2022
            // Microsoft Learn

            RunTest("SubmitNewClaim: Creates Claim with Correct Total and Status", async () =>
            {
                var service = new ClaimService();
                var initialClaimCount = (await service.GetClaimsByLecturer(1)).Count;

                var newClaim = new Claims { LecturerID = 1, Month = 11, Year = 2025 };
                var hours = new List<HoursWorked> { new HoursWorked { Hours = 8 }, new HoursWorked { Hours = 2 } };
                var documents = new List<SupportingDocument>();

                decimal expectedTotal = 10 * 500.00m; // 10 hours * 500.00m rate (Steven Pro)

                int newClaimId = await service.SubmitNewClaim(newClaim, hours, documents);
                var submittedClaims = await service.GetClaimsByLecturer(1);
                var submittedClaim = submittedClaims.FirstOrDefault(c => c.ClaimID == newClaimId);

                AssertIsTrue(newClaimId > 0, "Claim ID should be assigned and positive.");
                AssertIsTrue(submittedClaim != null, "Submitted claim should be retrievable.");
                AssertAreEqual(initialClaimCount + 1, submittedClaims.Count, "Claim count should increase by one.");
                AssertAreEqual(expectedTotal, submittedClaim.TotalAmount, "Total amount calculated incorrectly.");
                AssertAreEqual(3, submittedClaim.StatusID, "New claim status should be 'Pending Review' (ID 3).");
            });

            RunTest("UpdateClaimStatus: Successfully Changes Status", async () =>
            {
                var service = new ClaimService();
                int claimId = 1; // Initially Pending Review (ID 3)
                string newStatus = "Approved";

                bool success = await service.UpdateClaimStatus(claimId, newStatus);
                var updatedClaims = await service.GetClaimsByLecturer(1);
                var updatedClaim = updatedClaims.First(c => c.ClaimID == claimId);

                AssertIsTrue(success, "Status update should return true.");
                AssertAreEqual(4, updatedClaim.StatusID, "Claim status should be updated to Approved (ID 4).");
                AssertAreEqual(newStatus, updatedClaim.StatusName, "Claim status name should be updated.");
            });
        }

        // --- Test Group 3: Error Handling ---

        private static void TestStatusUpdateAndErrorHandling()
        {
            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.assert?view=mstest-net-4.0
            // Microsoft Learn

            // Code Attribution
            // This method was adapted from Microsoft Learn
            // https://learn.microsoft.com/en-us/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code?view=vs-2022
            // Microsoft Learn

            RunTest("UpdateClaimStatus: Throws Exception When Claim Not Found", async () =>
            {
                var service = new ClaimService();
                int invalidClaimId = 999;
                string newStatus = "Rejected";

                AssertThrowsAsync<InvalidOperationException>(async () =>
                {
                    await service.UpdateClaimStatus(invalidClaimId, newStatus);
                }, "Expected InvalidOperationException for non-existent claim.");
            });

            RunTest("UpdateClaimStatus: Throws Exception When Status is Invalid", async () =>
            {
                var service = new ClaimService();
                int claimId = 1;
                string invalidStatusName = "NonExistentStatus";

                AssertThrowsAsync<ArgumentException>(async () =>
                {
                    await service.UpdateClaimStatus(claimId, invalidStatusName);
                }, "Expected ArgumentException for invalid status name.");
            });
        }
    }
}
