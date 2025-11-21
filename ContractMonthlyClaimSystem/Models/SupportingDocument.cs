using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Models
{
    public class SupportingDocument
    {
        [Key]
        public int DocumentID { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public int ClaimID { get; set; }

        // NEW Code
        public virtual Claims Claim { get; set; }
    }
}
