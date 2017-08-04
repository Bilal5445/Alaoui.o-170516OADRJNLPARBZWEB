using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabicTextAnalyzer.ViewModels
{
    public class Class2
    {
        public M_ARABICDARIJAENTRY ArabicDarijaEntry { get; set; }
        public List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords { get; set; }
        public String ArabiziEntryText { get; set; }
    }
}