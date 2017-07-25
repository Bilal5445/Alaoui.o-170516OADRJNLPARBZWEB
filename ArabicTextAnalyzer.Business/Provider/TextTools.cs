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
            String matchRule = @"\b[A-Za-z0-9éèàâê]+\b";
            Regex regex = new Regex(matchRule);
            var matches = regex.Matches(arabicDarijaText);

            foreach (Match match in matches)
            {
                arabicDarijaText = arabicDarijaText.Replace(match.Value, "<b><mark>" + match.Value + "</mark></b>");
            }
            return arabicDarijaText;
        }
    }
}
