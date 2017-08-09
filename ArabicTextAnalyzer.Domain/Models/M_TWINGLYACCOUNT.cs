using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_TWINGLYACCOUNT
    {
        public Guid ID_TWINGLYACCOUNT_API_KEY { get; set; }    // PK
        public String UserName { get; set; }
        public int calls_free { get; set; }
        public String CurrentActive { get; set; }
    }
}
