using ArabicTextAnalyzer.Domain.Models;
using OADRJNLPCommon.Business;
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
            String matchRule = RegexConstant.ExtractLatinWordsAndExcludeNumOnlyRule; // exclude digits only
            Regex regex = new Regex(matchRule);
            var matches = regex.Matches(arabicDarijaText);
            return matches;
        }

        public static String HighlightExtractedLatinWords(String arabicDarijaText)
        {
            // Highlith and add label of counts of variantes

            var matches = ExtractLatinWords(arabicDarijaText);

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

            foreach (var arabicDarijaEntryLatinWord in ArabicDarijaEntryLatinWords)
            {
                var mostPopularVariant = arabicDarijaEntryLatinWord.MostPopularVariant;
                var latinWord = arabicDarijaEntryLatinWord.LatinWord;
                var count = arabicDarijaEntryLatinWord.VariantsCount;
                var guid = arabicDarijaEntryLatinWord.ID_ARABICDARIJAENTRY_LATINWORD;
                String newhtml;
                if (count > 0)
                {
                    if (guid != Guid.Empty)
                        newhtml = $@"<b><mark data-toggle='tooltip' title='{mostPopularVariant}'>{latinWord}</mark></b>
                                    <a href='/Train/X/?arabiziWord={latinWord}&arabiziWordGuid={guid}'>
                                        <span class='badge'>{count}</span>
                                    </a>";
                    else
                        newhtml = $@"<b><mark data-toggle='tooltip' title='{mostPopularVariant}'>{latinWord}</mark></b>
                                    <span class='badge'>{count}</span>";
                }
                else
                    newhtml = $@"<b><mark>" + latinWord + "</mark></b>";
                var regex = new Regex(RegexConstant.notPreceededByMark + latinWord, RegexOptions.IgnoreCase);
                arabicDarijaText = regex.Replace(arabicDarijaText, newhtml, 1);
            }

            //
            return arabicDarijaText;
        }
    }
}
