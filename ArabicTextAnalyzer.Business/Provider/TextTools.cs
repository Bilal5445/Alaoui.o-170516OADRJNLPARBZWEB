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

            //
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
                // do not consider words in the bidict as latin words
                if (new TextFrequency().BidictContainsWord(match.Value))
                    continue;

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
                var translation = arabicDarijaEntryLatinWord.Translation;
                var count = arabicDarijaEntryLatinWord.VariantsCount;
                var guid = arabicDarijaEntryLatinWord.ID_ARABICDARIJAENTRY_LATINWORD;
                String newhtml;
                if (count > 0)
                {
                    if (String.IsNullOrEmpty(translation) == false)
                    {
                        newhtml = $@"&rlm;<b><mark>" + translation + "</mark></b>";
                    }
                    else
                    {
                        if (guid != Guid.Empty)
                            newhtml = $@"&rlm;<b><mark data-toggle='tooltip' title='{mostPopularVariant}'>{latinWord}</mark></b>
                                    <a href='/Train/Train_AddToCorpus/?arabiziWord={latinWord}&arabiziWordGuid={guid}'>
                                        <span class='badge'>{count}</span>
                                    </a>";
                        else
                            newhtml = $@"&rlm;<b><mark data-toggle='tooltip' title='{mostPopularVariant}'>{latinWord}</mark></b>
                                    <span class='badge'>{count}</span>";
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(translation) == false)
                        newhtml = $@"&rlm;<b><mark>" + translation + "</mark></b>";
                    else
                        newhtml = $@"&rlm;<b><mark>" + latinWord + "</mark></b>";
                }
                var regex = new Regex(RegexConstant.notPreceededByMark + latinWord, RegexOptions.IgnoreCase);
                arabicDarijaText = regex.Replace(arabicDarijaText, newhtml, 1);
            }

            //
            return arabicDarijaText;
        }

        public static String DisplayEntities(List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            String entitiesString = String.Empty;
            foreach (var textEntity in TextEntities)
            {
                if (TextEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Mention + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Mention + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Mention + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Mention + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayEntitiesType(List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            String entitiesString = String.Empty;
            foreach (var textEntity in TextEntities)
            {
                if (TextEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Type + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Type + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Type + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Type + "</span> ";
            }

            return entitiesString;
        }
    }
}
