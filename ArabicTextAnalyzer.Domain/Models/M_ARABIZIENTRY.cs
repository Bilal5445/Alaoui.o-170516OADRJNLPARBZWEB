using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_ARABIZIENTRY")]
    public class M_ARABIZIENTRY
    {
        [Key]
        public Guid ID_ARABIZIENTRY { get; set; }
        public String ArabiziText { get; set; }
        public DateTime ArabiziEntryDate { get; set; }
        public bool IsFR { get; set; }
        public Guid ID_XTRCTTHEME { get; set; }    // FK
        public int IsDeleted { get; set; }
    }
}
