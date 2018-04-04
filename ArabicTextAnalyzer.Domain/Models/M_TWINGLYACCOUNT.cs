using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_TWINGLYACCOUNT")]
    public class M_TWINGLYACCOUNT
    {
        [Key]
        public Guid ID_TWINGLYACCOUNT_API_KEY { get; set; }    // PK
        public String UserName { get; set; }
        public int calls_free { get; set; }
        public String CurrentActive { get; set; }
    }
}
