using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Models
{
    public class HoursWorked
    {
        [Key]
        public int HoursWorkedID { get; set; }

        [Required]
        public int ClaimID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateWorked { get; set; }

        [Required]
        [Range(0.1, 24.0, ErrorMessage = "Hours must be greater than zero.")]
        public double Hours { get; set; }
        public string Description { get; set; }

        // NEW
        public virtual Claims Claim { get; set; }
    }
}
