using System;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Models
{
    public class Lecturer
    {
        [Key]
        public int LecturerID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; }

        // NEW
        [Required, MaxLength(50)]
        public string Role { get; set; } = "Lecturer";

        // NEW
        [Required]
        public string Email { get; set; }

        public virtual ICollection<Claims> Claims { get; set; }
    }
}
