using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("T_ARABICDARIJAENTRY_TEXTENTITY")]
    public class M_ARABICDARIJAENTRY_TEXTENTITY
    {
        [Key]
        public Guid ID_ARABICDARIJAENTRY_TEXTENTITY { get; set; }    // PK
        public Guid ID_ARABICDARIJAENTRY { get; set; }              // FK one-to-many
        public TextEntity TextEntity { get; set; }
    }
}
