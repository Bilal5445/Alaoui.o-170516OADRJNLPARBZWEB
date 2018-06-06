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

        public static String HighlightExtractedLatinWords(Guid ID_ARABIZIENTRY, List<M_ARABICDARIJAENTRY> arabicDarijaEntrys, List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords)
        {
            // in this method ArabicDarijaEntryLatinWords are not pre-filtered

            // Highlith and add label of counts of variantes

            // find arabic darija text
            var arabicDarijaEntry = arabicDarijaEntrys.Find(m => m.ID_ARABIZIENTRY == ID_ARABIZIENTRY);
            String arabicDarijaText = arabicDarijaEntry.ArabicDarijaText;

            // limit to concerned latin words
            List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = ArabicDarijaEntryLatinWords.FindAll(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntry.ID_ARABICDARIJAENTRY);

            foreach (var arabicDarijaEntryLatinWord in arabicDarijaEntryLatinWords)
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

        public static String HighlightExtractedLatinWords(M_ARABICDARIJAENTRY arabicDarijaEntry, List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords)
        {
            // in this method ArabicDarijaEntryLatinWords are not pre-filtered

            // Highlith and add label of counts of variantes

            // find arabic darija text
            String arabicDarijaText = arabicDarijaEntry.ArabicDarijaText;

            // limit to concerned latin words
            List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = ArabicDarijaEntryLatinWords.FindAll(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntry.ID_ARABICDARIJAENTRY);

            foreach (var arabicDarijaEntryLatinWord in arabicDarijaEntryLatinWords)
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

        public static String HighlightExtractedLatinWords(String arabicDarijaText, Guid ID_ARABICDARIJAENTRY, List<M_ARABICDARIJAENTRY_LATINWORD> ArabicDarijaEntryLatinWords)
        {
            // in this method ArabicDarijaEntryLatinWords are not pre-filtered

            // Highlith and add label of counts of variantes

            // limit to concerned latin words
            List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = ArabicDarijaEntryLatinWords.FindAll(m => m.ID_ARABICDARIJAENTRY == ID_ARABICDARIJAENTRY);

            foreach (var arabicDarijaEntryLatinWord in arabicDarijaEntryLatinWords)
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
                String badgeCounter = textEntity.TextEntity.Count > 1 ? "(" + textEntity.TextEntity.Count + ")" : String.Empty;
                if (TextEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayEntities(Guid ID_ARABIZIENTRY, List<M_ARABICDARIJAENTRY> arabicDarijaEntrys, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            // find concerned arabic darija
            var arabicDarijaEntry = arabicDarijaEntrys.Find(m => m.ID_ARABIZIENTRY == ID_ARABIZIENTRY);

            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntry.ID_ARABICDARIJAENTRY);

            String entitiesString = String.Empty;
            foreach (var textEntity in textEntities)
            {
                String badgeCounter = textEntity.TextEntity.Count > 1 ? "(" + textEntity.TextEntity.Count + ")" : String.Empty;
                if (TextEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayEntities(M_ARABICDARIJAENTRY arabicDarijaEntry, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntry.ID_ARABICDARIJAENTRY);

            String entitiesString = String.Empty;
            foreach (var textEntity in textEntities)
            {
                String badgeCounter = textEntity.TextEntity.Count > 1 ? "(" + textEntity.TextEntity.Count + ")" : String.Empty;
                if (TextEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (TextEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayEntities(Guid ID_ARABICDARIJAENTRY, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == ID_ARABICDARIJAENTRY);

            String entitiesString = String.Empty;
            foreach (var textEntity in textEntities)
            {
                String badgeCounter = textEntity.TextEntity.Count > 1 ? "(" + textEntity.TextEntity.Count + ")" : String.Empty;
                if (textEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayEntities(String FK_ENTRY, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            String entitiesString = String.Empty;

            // check before
            if (String.IsNullOrWhiteSpace(FK_ENTRY))
                return entitiesString;

            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.FK_ENTRY == FK_ENTRY);

            foreach (var textEntity in textEntities)
            {
                String badgeCounter = textEntity.TextEntity.Count > 1 ? "(" + textEntity.TextEntity.Count + ")" : String.Empty;
                if (textEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\" style='font-size: 13px; font-weight: 400;'>" + textEntity.TextEntity.Mention + " " + badgeCounter + "</span> ";
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

        public static String DisplayEntitiesType(Guid ID_ARABIZIENTRY, List<M_ARABICDARIJAENTRY> arabicDarijaEntrys, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            // find concerned arabic darija
            var arabicDarijaEntry = arabicDarijaEntrys.Find(m => m.ID_ARABIZIENTRY == ID_ARABIZIENTRY);

            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntry.ID_ARABICDARIJAENTRY);

            String entitiesString = String.Empty;
            foreach (var textEntity in textEntities)
            {
                if (textEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Type + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Type + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Type + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Type + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayEntitiesType(M_ARABICDARIJAENTRY arabicDarijaEntry, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntry.ID_ARABICDARIJAENTRY);

            String entitiesString = String.Empty;
            foreach (var textEntity in textEntities)
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

        public static String DisplayEntitiesType(Guid ID_ARABICDARIJAENTRY, List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities)
        {
            // limit to concerned text entities
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = TextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == ID_ARABICDARIJAENTRY);

            String entitiesString = String.Empty;
            foreach (var textEntity in textEntities)
            {
                if (textEntities.IndexOf(textEntity) % 4 == 0)
                    entitiesString += "<span class=\"label label-primary\">" + textEntity.TextEntity.Type + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 1)
                    entitiesString += "<span class=\"label label-default\">" + textEntity.TextEntity.Type + "</span> ";
                else if (textEntities.IndexOf(textEntity) % 4 == 2)
                    entitiesString += "<span class=\"label label-success\">" + textEntity.TextEntity.Type + "</span> ";
                else
                    entitiesString += "<span class=\"label label-info\">" + textEntity.TextEntity.Type + "</span> ";
            }

            return entitiesString;
        }

        public static String DisplayRemoveAndApplyTagCol(Guid ID_ARABIZIENTRY, Guid ID_ARABICDARIJAENTRY, List<M_XTRCTTHEME> mainEntities)
        {
            // remove button
            String newhtml1 = $@"<a href='/Train/Train_DeleteEntry/?arabiziWordGuid={ID_ARABIZIENTRY}' class='btn btn-danger btn-xs small'><span class='glyphicon glyphicon-remove small' aria-hidden='true'></span></a>";

            // refresh button
            String newhtml11 = $@"<a href='/Train/Train_RefreshEntry/?arabiziWordGuid={ID_ARABIZIENTRY}' class='btn btn-info btn-xs small'><span class='glyphicon glyphicon-refresh small' aria-hidden='true'></span></a>";

            // dropdown tags
            String newhtml2 = $@"<div class='dropdown'>
                                    <button class='btn btn-warning btn-xs dropdown-toggle' type='button' data-toggle='dropdown'>
                                        <span class='caret'></span>
                                    </button>";
            newhtml2 += "<ul class='dropdown-menu'>";
            foreach (var mainEntity in mainEntities)
            {
                newhtml2 += $@"<li><a href = '/Train/Train_ApplyNewMainTag/?idArabicDarijaEntry={ID_ARABICDARIJAENTRY}&mainEntity={mainEntity.ThemeName.Trim()}'> {mainEntity.ThemeName.Trim()} </a></li>";
            }
            newhtml2 += $@"</ul>
                    </div>";

            //
            return newhtml1 + newhtml11 + newhtml2;
        }
    }
}
