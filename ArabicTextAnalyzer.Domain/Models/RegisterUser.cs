using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("RegisterUser")]
    public class RegisterUser
    {
        [Key]
        public int UserID { get; set; }

        // [Required(ErrorMessage = "Required Username")]
        // [StringLength(30, MinimumLength = 2, ErrorMessage = "Username Must be Minimum 2 Charaters")]
        public string Username { get; set; }

        // [DataType(DataType.Password)]
        // [Required(ErrorMessage = "Required Password")]
        // [MaxLength(30, ErrorMessage = "Password cannot be Greater than 30 Charaters")]
        // [StringLength(31, MinimumLength = 7, ErrorMessage = "Password Must be Minimum 7 Charaters")]
        public string Password { get; set; }
        public DateTime CreateOn { get; set; }

        // [Required(ErrorMessage = "Required EmailID")]
        // [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter Valid Email ID")]
        public string EmailID { get; set; }

        // extend Identity User to save last login time
        public virtual DateTime? LastLoginTime { get; set; }
        public virtual Guid UserGuid { get; set; }
    }
}
