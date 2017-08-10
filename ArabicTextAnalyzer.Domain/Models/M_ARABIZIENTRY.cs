using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_ARABIZIENTRY
    {
        public Guid ID_ARABIZIENTRY { get; set; }
        public String ArabiziText { get; set; }
        public DateTime ArabiziEntryDate { get; set; }
    }
}
