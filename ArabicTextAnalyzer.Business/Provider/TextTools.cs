﻿using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextTools
    {
        public static MatchCollection ExtractLatinWords(String arabicDarijaText)
        {
            String matchRule = @"\b[A-Za-z0-9]+\b";
            Regex regex = new Regex(matchRule);
            var matches = regex.Matches(arabicDarijaText);
            return matches;
        }

        public static String HighlightExtractedLatinWords(String arabicDarijaText)
        {
            // Highlith and add label of counts of variantes

            String matchRule = @"\b[A-Za-z0-9éèàâê]+\b";
            Regex regex = new Regex(matchRule);
            var matches = regex.Matches(arabicDarijaText);

            //
            foreach (Match match in matches)
            {
                // exclude digits only
                int value;
                if (int.TryParse(match.Value, out value))
                    continue;

                arabicDarijaText = arabicDarijaText.Replace(match.Value, "<b><mark>" + match.Value + "</mark></b>");
            }

            //
            return arabicDarijaText;
        }

        public static String HighlightExtractedLatinWords(String arabicDarijaText, List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords)
        {
            // Highlith and add label of counts of variantes

            String matchRule = @"\b[A-Za-z0-9éèàâê]+\b";
            Regex regex = new Regex(matchRule);
            var matches = regex.Matches(arabicDarijaText);

            //
            foreach (Match match in matches)
            {
                // exclude digits only
                int value;
                if (int.TryParse(match.Value, out value))
                    continue;

                var found = ArabicDarijaEntryLatinWords.Find(m => m.LatinWord == match.Value);
                var count = 0;
                if (found != null)
                    count = found.VariantsCount;
                if (count > 0)
                    arabicDarijaText = arabicDarijaText.Replace(match.Value, "<b><mark>" + match.Value + "</mark></b><span class='badge'>" + count + "</span>");
                else
                    arabicDarijaText = arabicDarijaText.Replace(match.Value, "<b><mark>" + match.Value + "</mark></b>");
            }

            //
            return arabicDarijaText;
            // return "hello";
        }
    }
}
