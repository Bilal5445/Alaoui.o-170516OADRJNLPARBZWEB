using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_XTRCTTHEME")]
    public class M_XTRCTTHEME
    {
        [Key]
        public Guid ID_XTRCTTHEME { get; set; }    // PK
        public String ThemeName { get; set; }
        public String CurrentActive { get; set; }
        public String UserID { get; set; }
    }
}
