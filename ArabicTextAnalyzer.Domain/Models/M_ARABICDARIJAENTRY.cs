using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_ARABICDARIJAENTRY")]
    public class M_ARABICDARIJAENTRY
    {
        [Key]
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public Guid ID_ARABIZIENTRY { get; set; }
        public String ArabicDarijaText { get; set; }
    }
}
