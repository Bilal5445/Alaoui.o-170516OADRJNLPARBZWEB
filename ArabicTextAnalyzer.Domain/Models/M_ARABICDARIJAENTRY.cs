using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_ARABICDARIJAENTRY
    {
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public Guid ID_ARABIZIENTRY { get; set; }
        public String ArabicDarijaText { get; set; }
    }
}
