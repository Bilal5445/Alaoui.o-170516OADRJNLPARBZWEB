using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_XTRCTTHEME
    {
        public Guid ID_XTRCTTHEME { get; set; }    // PK
        public String ThemeName { get; set; }
        public String CurrentActive { get; set; }
    }
}
