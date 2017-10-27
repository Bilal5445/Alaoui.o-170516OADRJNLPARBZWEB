using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabicTextAnalyzer.ViewModels
{
    public class ArabiziViewModel
    {
        public List<M_ARABIZIENTRY> ArabiziEntrys { get; set; }
        public List<M_ARABICDARIJAENTRY> ArabicDarijaEntrys { get; set; }
        public List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords { get; set; }
        public List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities { get; set; }
        public List<M_XTRCTTHEME> MainEntities { get; set; }
    }

    public class Class2
    {
        public M_ARABICDARIJAENTRY ArabicDarijaEntry { get; set; }
        public List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords { get; set; }
        public M_ARABIZIENTRY ArabiziEntry { get; set; }
        public List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities { get; set; }
    }

    public class Class1
    {
        public List<Class2> Classes2 { get; set; }
        // public IEnumerable<M_ARABICDARIJAENTRY_TEXTENTITY> MainEntities { get; set; }
        public IEnumerable<M_XTRCTTHEME> MainEntities { get; set; }
    }

    // class for the table in the partialview
    public class ArabiziToArabicViewModel
    {
        public Guid ID_ARABIZIENTRY { get; set; }
        public DateTime ArabiziEntryDate { get; set; }
        public String FormattedArabiziEntryDate { get; set; }
        public String ArabiziText { get; set; }
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public String ArabicDarijaText { get; set; }
        public String FormattedArabicDarijaText { get; set; }
        public String FormattedEntitiesTypes { get; set; }
        public String FormattedEntities { get; set; }
        public String FormattedRemoveAndApplyTagCol { get; set; }
    }
}