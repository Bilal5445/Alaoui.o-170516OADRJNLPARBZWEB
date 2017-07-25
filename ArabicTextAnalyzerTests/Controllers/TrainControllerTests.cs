﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void ut_170721_test_recompile_corpus_full_loop()
        {
            //
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            // String arabiziKeyword = "netrecheh"; // > 2000 variantes !!?
            String arabiziKeyword = "makatjich";    // > 448 variantes !!?

            // assert it is not converted at start
            var arabicKeyword = textConverter.Convert(arabiziKeyword);
            Assert.AreEqual(arabiziKeyword, arabicKeyword);

            // 1 get all variants
            var variants = textConverter.GetAllTranscriptions(arabiziKeyword);

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

        [TestMethod()]
        public void ut_170723_test_recompile_corpus_simple_cat_corpus()
        {
            var textConverter = new TextConverter();

            // 5 recompile the dict
            textConverter.CatCorpusDict();
        }

        [TestMethod()]
        public void ut_170723_test_recompile_corpus_simple_srilm_dict()
        {
            var textConverter = new TextConverter();

            // 5 recompile the dict
            textConverter.SrilmLmDict();
        }

        [TestMethod()]
        public void ut_170725_test_convert_arabizi_katji_to_variants_inf_65()
        {
            String word = "katji";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 65);
        }

        [TestMethod()]
        public void ut_170725_test_extract_without_initial_ma_and_final_ch()
        {
            String arabiziKeyword = "makatjich";

            // 3 preprocess if ma/ch
            // String pattern = "\bma{[a-z].+}ch\b";
            String pattern = @"\bma(.+)ch\b";
            String miniArabiziKeyword = Regex.Replace(arabiziKeyword, pattern, "$1");
            // String miniArabiziKeyword = Regex.Replace(arabiziKeyword, pattern, m => m.Groups[0].Captures[0].Value);
            Assert.AreEqual("katji", miniArabiziKeyword);
        }

        [TestMethod()]
        public void ut_170725_test_getPostBasedOnKeywordFromFBViaTwingly_no_index_was_out_of_range()
        {
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            var mostPopularKeyword = "كتجي";
            var completeArabicKeyword = "ما" + mostPopularKeyword + "ش";
            Assert.AreEqual("ماكتجيش", completeArabicKeyword);

            //
            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(completeArabicKeyword, twinglyApi15Url, twinglyApiKey, true);
            Assert.IsTrue(postText.Contains(completeArabicKeyword));
        }

        [TestMethod()]
        public void ut_170725_test_recompile_corpus_full_loop_makatjich_under_50_variants()
        {
            // this one is the sentence supposedely returned by twingly for keyword ماكتجيش
            var expectedpostText = @"هاد البومب ماكتجيش عشوائية 😁 ، كتوجد ليها من الليلة ديال لبارح  ✍️
يا بومبييناااي 💪";

            // make it one line
            var onelineexpectedpostText = expectedpostText.Replace("\r\n", " ");
            var expectedonelineexpectedpostText = "هاد البومب ماكتجيش عشوائية 😁 ، كتوجد ليها من الليلة ديال لبارح  ✍️ يا بومبييناااي 💪";
            Assert.AreEqual(expectedonelineexpectedpostText, onelineexpectedpostText);

            // 0 drop the test pharase from the dict
            var textFrequency = new TextFrequency();
            textFrequency.DropPhraseFromCorpus(onelineexpectedpostText);

            // 1 arabizi
            String arabizi = "Ya wlad lkhab nta li kadwi makatjich lwa9afat w kadwi ya terikt jradistat";
            String arabiziKeyword = "makatjich";    // > 448 variantes !!?

            // 2 convert first pass
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            var arabic = textConverter.Convert(arabizi);

            // 2 latin words
            var matches = TextTools.ExtractLatinWords(arabic);
            Assert.AreEqual(arabiziKeyword, matches[0].Value);

            // 3 preprocess if ma/ch
            String pattern = @"\bma(.+)ch\b";
            String miniArabiziKeyword = Regex.Replace(arabiziKeyword, pattern, "$1");
            Assert.AreEqual("katji", miniArabiziKeyword);

            // 4 get all variants
            var variants = textConverter.GetAllTranscriptions(miniArabiziKeyword);
            Assert.IsTrue(variants.Count < 50);

            // 5 get most popular keyword
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);
            var expectedmostPopularKeyword = "كتجي";
            Assert.AreEqual(expectedmostPopularKeyword, mostPopularKeyword);
            // 6 re-add "ma" & "ch"
            var completeArabicKeyword = "ما" + mostPopularKeyword + "ش";
            Assert.AreEqual("ماكتجيش", completeArabicKeyword);

            // 7 get a post containing this keyword
            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(completeArabicKeyword, twinglyApi15Url, twinglyApiKey, true);
            Assert.AreEqual(onelineexpectedpostText, postText);

            // 8 add this post to dict
            textFrequency.AddPhraseToCorpus(postText);

            // 9 recompile the dict
            textConverter.CatCorpusDict();
            textConverter.SrilmLmDict();

            // 10 assert it is now converted
            var arabicKeyword = textConverter.Convert(arabiziKeyword);
            Assert.AreEqual(completeArabicKeyword, arabicKeyword);
        }

        [TestMethod()]
        public void ut_170725_test_delete_sentence_from_corpus()
        {
            // this one is the sentence supposedely returned by twingly for keyword ماكتجيش
            var expectedpostText = @"هاد البومب ماكتجيش عشوائية 😁 ، كتوجد ليها من الليلة ديال لبارح  ✍️ يا بومبييناااي 💪";

            var textFrequency = new TextFrequency();
            var contains = textFrequency.CorpusContainsSentence(expectedpostText);

            // 0 drop the test pharase from the dict
            textFrequency.DropPhraseFromCorpus(expectedpostText);

            contains = textFrequency.CorpusContainsSentence(expectedpostText);
            Assert.AreEqual(false, contains);

            // 8 add this post to dict
            textFrequency.AddPhraseToCorpus(expectedpostText);
        }

        [TestMethod()]
        public void ut_170725_test_make_sure_variants_netrecheh_are_less_than_175()
        {
            String word = "netrecheh";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 175);
        }

        [TestMethod()]
        public void ut_170725_test_make_sure_motalat_variants_are_less_than_75()
        {
            String word = "motalat";
            var variants = new TextConverter().GetAllTranscriptions(word);

            // shoudl not convert medial o to haa
            Assert.IsFalse(variants.Contains("مهتلت"));

            //
            Assert.IsTrue(variants.Count < 75, "nbr variants : " + variants.Count);
        }

        [TestMethod()]
        public void ut_170725_test_make_sure_we_can_catch_ma_kherjoch_as_makherjoch()
        {
            String arabizi = "Banet lia hadi sora dial derby lekher melli lwez ma kherjoch men lcarré dialhom";
            String target = "Banet lia hadi sora dial derby lekher melli lwez makherjoch men lcarré dialhom";

            // 3 preprocess if ma/ch
            String miniArabiziKeyword = new TextConverter().Preprocess_ma_ch(arabizi);
            Assert.AreEqual(target, miniArabiziKeyword);
        }

        [TestMethod()]
        public void ut_170725_test_make_sure_kherjo_variants_are_less_than_35()
        {
            String word = "kherjo";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 35, "nbr variants : " + variants.Count);
        }

        [TestMethod()]
        public void ut_170725_test_add_a_post_to_corpus_after_a_newline()
        {
            String post = @"علاش المسلمين في بروكسيل ماخرجوش يستنكرو الارهاب";

            TextFrequency textFrequency = new TextFrequency();

            var before = textFrequency.GetCorpusNumberOfLine();
            textFrequency.AddPhraseToCorpus(post);
            var after = textFrequency.GetCorpusNumberOfLine();

            //
            Assert.AreEqual(after, before + 1);
        }

        [TestMethod()]
        public void ut_170725_test_recompile_corpus_full_loop_kherjo_under_35_variants()
        {
            // TODO

            // this one is the sentence supposedely returned by twingly for keyword ماكتجيش
            /*var expectedpostText = @"تلقا واحد كيكلاشي فيها حينت متلات واليوم فالعشيةتلقاه كيقلب على ختها فالحقيقة..... الله يلطف";

            // make it one line
            var onelineexpectedpostText = expectedpostText.Replace("\r\n", " ");
            var expectedonelineexpectedpostText = "تلقا واحد كيكلاشي فيها حينت متلات واليوم فالعشيةتلقاه كيقلب على ختها فالحقيقة..... الله يلطف";
            Assert.AreEqual(expectedonelineexpectedpostText, onelineexpectedpostText);

            // 0 drop the test pharase from the dict */
            var textFrequency = new TextFrequency();
            /*textFrequency.DropPhraseFromCorpus(onelineexpectedpostText);
            */

            // 1 arabizi
            String arabizi = "Krik w rwida sokor w motalat en panne w jili w dozane w bolat jdad w bidon d zit";
            String arabiziKeyword = "kherjo";

            // 2 convert first pass
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "246229A7-86D2-4199-8D6E-EF406E7F3728";
            var arabic = textConverter.Convert(arabizi);

            // 2 latin words
            var matches = TextTools.ExtractLatinWords(arabic);
            Assert.AreEqual(arabiziKeyword, matches[0].Value);

            // 3 preprocess if ma/ch
            /*String pattern = @"\bma(.+)ch\b";
            String miniArabiziKeyword = Regex.Replace(arabiziKeyword, pattern, "$1");
            Assert.AreEqual("katji", miniArabiziKeyword);*/

            // 4 get all variants
            var variants = textConverter.GetAllTranscriptions(/*miniA*/arabiziKeyword);
            Assert.IsTrue(variants.Count < 50);

            // 5 get most popular keyword
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);
            /*var expectedmostPopularKeyword = "متلات";
            Assert.AreEqual(expectedmostPopularKeyword, mostPopularKeyword);*/
            // 6 re-add "ma" & "ch"
            // var completeArabicKeyword = "ما" + mostPopularKeyword + "ش";
            // Assert.AreEqual("ماكتجيش", completeArabicKeyword);

            // 7 get a post containing this keyword
            // var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(completeArabicKeyword, twinglyApi15Url, twinglyApiKey, true);
            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(mostPopularKeyword, twinglyApi15Url, twinglyApiKey, true);
            // Assert.AreEqual(onelineexpectedpostText, postText);

            // 8 add this post to dict
            textFrequency.AddPhraseToCorpus(postText);

            // 9 recompile the dict
            textConverter.CatCorpusDict();
            textConverter.SrilmLmDict();

            // 10 assert it is now converted
            var arabicKeyword = textConverter.Convert(arabiziKeyword);
            // Assert.AreEqual(completeArabicKeyword, arabicKeyword);
            Assert.AreEqual(mostPopularKeyword, arabicKeyword);
        }
    }
}