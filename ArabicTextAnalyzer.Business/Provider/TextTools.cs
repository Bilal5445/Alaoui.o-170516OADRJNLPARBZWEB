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

            var matches = ExtractLatinWords(arabicDarijaText);

            //
            foreach (Match match in matches)
            {
                // exclude digits only
                int value;
                if (int.TryParse(match.Value, out value))
                    continue;

                var found = ArabicDarijaEntryLatinWords.Find(m => m.LatinWord == match.Value);
                var count = 0;
                Guid guid = Guid.Empty;
                var mostPopularVariant = String.Empty;
                if (found != null)
                {
                    count = found.VariantsCount;
                    guid = found.ID_ARABICDARIJAENTRY_LATINWORD;
                    mostPopularVariant = found.MostPopularVariant;
                }

                //
                if (count > 0)
                {
                    string newhtml;
                    if (guid != Guid.Empty)
                        newhtml = $@"<b><mark data-toggle='tooltip' title='{mostPopularVariant}'>{match.Value}</mark></b>
                                    <a href='/Train/X/?arabiziWord={match.Value}&arabiziWordGuid={guid}'>
                                        <span class='badge'>{count}</span>
                                    </a>";
                    else
                        newhtml = $@"<b><mark data-toggle='tooltip' title='{mostPopularVariant}'>{match.Value}</mark></b>
                                    <span class='badge'>{count}</span>";
                    arabicDarijaText = arabicDarijaText.Replace(match.Value, newhtml);
                }
                else
                    arabicDarijaText = arabicDarijaText.Replace(match.Value, "<b><mark>" + match.Value + "</mark></b>");
            }

            //
            return arabicDarijaText;
            // return "hello";
        }
    }
}
