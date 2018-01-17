using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabicTextAnalyzer.ViewModels
{
    // class for the table in the partialview
    public class ArabiziToArabicViewModel
    {
        public Guid ID_ARABIZIENTRY { get; set; }
        public DateTime ArabiziEntryDate { get; set; }
        public String ArabiziText { get; set; }
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public String ArabicDarijaText { get; set; }
        public int PositionHash { get; set; }
        public String FormattedArabiziEntryDate { get; set; }
        public String FormattedArabicDarijaText { get; set; }
        public String FormattedEntitiesTypes { get; set; }
        public String FormattedEntities { get; set; }
        public String FormattedRemoveAndApplyTagCol { get; set; }
    }

    /*public class ArabiziToArabicViewModel2
    {
        public Guid ID_ARABIZIENTRY { get; set; }
        public DateTime ArabiziEntryDate { get; set; }
        public String ArabiziText { get; set; }
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public String ArabicDarijaText { get; set; }
        public int PositionHash { get; set; }
        public String FormattedArabiziEntryDate { get; set; }
        public String FormattedArabicDarijaText { get; set; }
        public String FormattedEntitiesTypes { get; set; }
        public String FormattedEntities { get; set; }
        public String FormattedRemoveAndApplyTagCol { get; set; }
        public String TextEntity_Mention { get; set; }
    }*/
}