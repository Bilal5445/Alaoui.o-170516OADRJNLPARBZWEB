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
        public int IsDeleted { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(150)]
        public string FK_ENTRY { get; set; }
        public int ENTRY_type { get; set; }
    }

    [Table("T_ARABICDARIJAENTRY_TEXTENTITY")]
    public class M_ARABICDARIJAENTRY_TEXTENTITY_FLAT
    {
        [Key]
        public Guid ID_ARABICDARIJAENTRY_TEXTENTITY { get; set; }    // PK
        public Guid ID_ARABICDARIJAENTRY { get; set; }              // FK one-to-many
        public long TextEntity_Count { get; set; }
        public string TextEntity_EntityId { get; set; }
        public string TextEntity_Mention { get; set; }
        public string TextEntity_Normalized { get; set; }
        public string TextEntity_Type { get; set; }
        public int IsDeleted { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(150)]
        public string FK_ENTRY { get; set; }
        public int ENTRY_type { get; set; }
    }
}
