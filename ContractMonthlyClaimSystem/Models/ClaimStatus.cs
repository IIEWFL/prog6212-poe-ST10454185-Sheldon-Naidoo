using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        public string StatusName { get; set; }

        // NEW
        public virtual ICollection<Claims> Claims { get; set; }
    }
}
