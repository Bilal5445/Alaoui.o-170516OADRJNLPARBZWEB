using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_XTRCTTHEME_CONFIG_KEYWORD")]
    public class M_XTRCTTHEME_CONFIG_KEYWORD
    {
        [Key]
        public Guid ID_XTRCTTHEME_CONFIG_KEYWORD { get; set; } // PK
        public Guid ID_XTRCTTHEME { get; set; }         // FK one-to-many
        public String Keyword { get; set; }
        public int IsDeleted { get; set; }
    }
}
