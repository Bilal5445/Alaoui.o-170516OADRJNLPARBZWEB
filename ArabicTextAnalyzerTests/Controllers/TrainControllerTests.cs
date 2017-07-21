using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArabicTextAnalyzer.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ArabicTextAnalyzer.Business.Provider;
using OADRJNLPCommon.Models;

namespace ArabicTextAnalyzer.Controllers.Tests
{
    [TestClass()]
    public class TrainControllerTests
    {
        [TestMethod()]
        public void ut_170718_test_match_latin_words()
        {
            String arabicDarijaEntry_ArabicDarijaText = "بتوفيق لقرد .... وصل عمالك حسبان او تلتفت الجمهور يغرد خرج سرب او يفقه شيئا في قانون المناصر waghalibiyatohom تابعين المارق و زرق ... المناصر دور تشجع فقط ولا بغا تغير ينخرط flmaktab ويترشح";

            // latin words
            var matches = TextTools.ExtractLatinWords(arabicDarijaEntry_ArabicDarijaText);

            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("waghalibiyatohom", matches[0].Value);
            Assert.AreEqual("flmaktab", matches[1].Value);
        }

        [TestMethod()]
        public void ut_170718_test_matches_foreach()
        {
            String arabicDarijaEntry_ArabicDarijaText = "بتوفيق لقرد .... وصل عمالك حسبان او تلتفت الجمهور يغرد خرج سرب او يفقه شيئا في قانون المناصر waghalibiyatohom تابعين المارق و زرق ... المناصر دور تشجع فقط ولا بغا تغير ينخرط flmaktab ويترشح";

            // latin words
            var matches = TextTools.ExtractLatinWords(arabicDarijaEntry_ArabicDarijaText);

            //
            foreach (Match match in matches)
            {
                Assert.AreEqual("waghalibiyatohom", match.Value);
                break;
            }
        }

        [TestMethod()]
        public void ut_170718_test_add_highlithing_html_code_to_latin_words_in_converted_string()
        {
            String arabicDarijaText = "بتوفيق لقرد .... وصل عمالك حسبان او تلتفت الجمهور يغرد خرج سرب او يفقه شيئا في قانون المناصر waghalibiyatohom تابعين المارق و زرق ... المناصر دور تشجع فقط ولا بغا تغير ينخرط flmaktab ويترشح";

            var arabicDarijaText2 = TextTools.HighlightExtractedLatinWords(arabicDarijaText);

            Assert.AreNotEqual(arabicDarijaText, arabicDarijaText2);
        }

        [TestMethod()]
        public void ut_170721_test_get_keyword_frequencies()
        {
            // "yan3al"
            String fbKeywordKeyword1 = "ينعل";
            String fbKeywordKeyword2 = "ينعآل";

            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";

            //
            FB_KEYWORD fbKeyword1 = OADRJNLPCommon.Business.Business.getFBKeywordInfoFromFBViaTwingly(fbKeywordKeyword1, twinglyApiKey, twinglyApi15Url);
            FB_KEYWORD fbKeyword2 = OADRJNLPCommon.Business.Business.getFBKeywordInfoFromFBViaTwingly(fbKeywordKeyword2, twinglyApiKey, twinglyApi15Url);


            //
            int occurrenceKeyword1 = fbKeyword1.matched_total_count_ma;
            int occurrenceKeyword2 = fbKeyword2.matched_total_count_ma;

            //
            Assert.IsTrue(occurrenceKeyword1 > occurrenceKeyword2);

            // TODO assert "way greater"
            Assert.IsTrue((occurrenceKeyword1 - occurrenceKeyword2) > occurrenceKeyword2 / 2);
        }

        [TestMethod()]
        public void ut_170721_test_convert_arabizi_to_variants()
        {
            var variants = new TextConverter().GetAllTranscriptions("yan3al");

            //
            Assert.IsTrue(variants.Count > 1);
        }

        [TestMethod()]
        public void ut_170721_test_convert_arabizi_to_variants_haz9ane()
        {
            var variants = new TextConverter().GetAllTranscriptions("haz9ane");

            //
            Assert.IsTrue(variants.Count > 1);
        }

        [TestMethod()]
        public void ut_170721_test_pick_variants_with_most_popularity()
        {
            var variants = new TextConverter().GetAllTranscriptions("yan3al");

            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";

            //
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);

            Assert.AreEqual("ينعل", mostPopularKeyword);
        }

        [TestMethod()]
        public void ut_170721_test_search_a_post_given_a_keyword_via_twingly()
        {
            String keyword = "ينعل";

            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";

            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(keyword, twinglyApi15Url, twinglyApiKey, true);

            Assert.AreNotEqual(String.Empty, postText);
            Assert.IsTrue(postText.Contains(keyword));
        }

        [TestMethod()]
        public void ut_170721_test_add_a_post_to_corpus()
        {
            String keyword = "ينعل";
            String post = @"+هدا واحد خينا مربي بزاااااااف كايدق باب التلاجة قبل ما يحلها.
+ هدا واحد سراق الزيت(صرصور) طاح ف مولينيكس ملي خرج قال الله ينعل اللي باقي يمشي ل لافوار";

            //
            Assert.IsTrue(post.Contains(keyword));

            TextFrequency textFrequency = new TextFrequency();

            var before = textFrequency.GetCorpusNumberOfLine();
            textFrequency.AddPhraseToCorpus(post);
            var after = textFrequency.GetCorpusNumberOfLine();

            //
            Assert.AreEqual(after, before + 1);
        }

        [TestMethod()]
        public void ut_170721_test_recompile_corpus()
        {
            //
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            String arabiziKeyword = "haz9ane";

            // assert it is not converted at start
            var arabicKeyword = textConverter.Convert(arabiziKeyword);
            Assert.AreEqual(arabiziKeyword, arabicKeyword);

            // 1 get all variants
            var variants = textConverter.GetAllTranscriptions("haz9ane");

            // 2 get most popular keyword
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);

            // 3 get a post containthing this keyword
            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(mostPopularKeyword, twinglyApi15Url, twinglyApiKey, true);

            // 4 add this post to dict
            new TextFrequency().AddPhraseToCorpus(postText);

            // 5 recompile the dict
            textConverter.CatCorpusDict();
            textConverter.SrilmLmDict();

            // assert it is now converted
            arabicKeyword = textConverter.Convert(arabiziKeyword);
            Assert.AreEqual(mostPopularKeyword, arabicKeyword);
        }
    }
}