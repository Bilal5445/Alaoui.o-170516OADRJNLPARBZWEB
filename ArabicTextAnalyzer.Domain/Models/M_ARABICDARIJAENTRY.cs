﻿using System;
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
        public Guid ID_ARABICDARIJAENTRY { get; set; }  // PK
        public Guid ID_ARABIZIENTRY { get; set; }       // FK one-to-one
        public String ArabicDarijaText { get; set; }
        public String ContribArabicDarijaText { get; set; }
        public int IsDeleted { get; set; }
    }
}
