using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;

// Part 2: NEW Code

namespace ContractMonthlyClaimSystem.Common
{
    public static class Messenger
    {
        // Event for when a new claim is submitted
        public static event Action<Claims> ClaimSubmitted;

        // The newly submitted claim object.
        public static void PublishClaimSubmitted(Claims claim)
        {
            ClaimSubmitted?.Invoke(claim);
        }
    }
}
