using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_XTRCTTHEME_KEYWORD")]
    public class M_XTRCTTHEME_KEYWORD
    {
        [Key]
        public Guid ID_XTRCTTHEME_KEYWORD { get; set; } // PK
        public Guid ID_XTRCTTHEME { get; set; }         // FK one-to-many
        public String Keyword { get; set; }
        public String Keyword_Type { get; set; }
        public int Keyword_Count { get; set; }
        public int IsDeleted { get; set; }
    }

    public class THEMETAGSCOUNT
    { 
        public String TextEntity_Mention { get; set; }
        public int SUM_TextEntity_Count { get; set; }
        public string TextEntity_Type { get; set; }
    }
}
