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
using OADRJNLPCommon.Business;
using ArabicTextAnalyzer.Domain.Models;
using System.Xml.Serialization;
using ArabicTextAnalyzer.Models;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.CSharp;

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

            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";

            //
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);

            Assert.AreEqual("ينعل", mostPopularKeyword);
        }

        [TestMethod()]
        public void ut_170721_test_search_a_post_given_a_keyword_via_twingly()
        {
            String keyword = "ينعل";

            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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
            String pattern = RegexConstant.maChRule;
            String miniArabiziKeyword = Regex.Replace(arabiziKeyword, pattern, "$1");
            Assert.AreEqual("katji", miniArabiziKeyword);
        }

        [TestMethod()]
        public void ut_170725_test_getPostBasedOnKeywordFromFBViaTwingly_no_index_was_out_of_range()
        {
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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
        public void ut_170727_test_Preprocess_al_wa()
        {
            var textConverter = new TextConverter();

            String arabizi = "al 3adala wa atanmia";
            String arabizi2 = textConverter.Preprocess_al_wa(arabizi);

            String expected = "al3adala wa alatanmia";
            Assert.AreEqual(expected, arabizi2);
        }

        [TestMethod()]
        public void ut_170727_test_make_sure_hazka_variants_are_less_than_35()
        {
            String word = "hazka";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 35, "nbr variants : " + variants.Count);
        }

        [TestMethod()]
        public void ut_170727_test_recompile_corpus_full_loop_hazka_under_35_variants()
        {
            // this one is the sentence supposedely returned by twingly for keyword : suposoefely one line
            var expectedpostText = @"انا القوة الخارقة لي عندي هي فاش كندوز من حدا شي سعاي و تيقولي شي درهم الله يرحم الواليدين الله ينجحك الله يطول فعمرك .. تنقول امين فنفسي و تنزطم .. بحال الى خديت دعوة فابور .. و متنعطيهش درهم حيت تنكون حازق و يلا كانت عندي 2 دراهم تنصرفها و تنعطي لواحد درهم حتى كيدعي معايا و تنعطي لشي واحد اخر .. ليكونومي";

            // 0 drop the test pharase from the dict
            var textFrequency = new TextFrequency();
            textFrequency.DropPhraseFromCorpus(expectedpostText);

            // 1 arabizi
            String arabizi = "Al houb wa al hazka";
            String arabiziKeyword = "hazka";

            // 2 convert first pass
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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
            Assert.AreEqual(expectedpostText, postText);

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

        [TestMethod()]
        public void ut_170727_test_make_sure_dsara_variants_are_less_than_100()
        {
            String word = "dsara";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 100, "nbr variants : " + variants.Count);
        }

        [TestMethod()]
        public void ut_170727_test_make_sure_we_can_catch_mamkhalassch_as_ma_mkhalass_ch()
        {
            String arabizi = "ha kolchi kaysowal, wach chra 7adraf, okaydwi mamkhalassch, ach kata3ni hadi ?";
            String target = "ha kolchi kaysowal, wach chra 7adraf, okaydwi ma mkhalass ch, ach kata3ni hadi ?";

            // 3 preprocess if ma/ch
            String miniArabiziKeyword = new TextConverter().Preprocess_ma_ch(arabizi);
            Assert.AreEqual(target, miniArabiziKeyword);
        }

        [TestMethod()]
        public void ut_170727_test_recompile_corpus_maxna_under_10_variants()
        {
            // 1 arabizi
            String arabiziKeyword = "maxna";

            // 2 convert first pass
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";

            // 4 get all variants
            var variants = textConverter.GetAllTranscriptions(/*miniA*/arabiziKeyword);
            Assert.IsTrue(variants.Count < 10);

            // 5 get most popular keyword
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);

            // 7 get a post containing this keyword
            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(mostPopularKeyword, twinglyApi15Url, twinglyApiKey, true);

            // 8 add this post to dict
            var textFrequency = new TextFrequency();
            if (textFrequency.CorpusContainsSentence(postText) == false)
                textFrequency.AddPhraseToCorpus(postText);

            // 9 recompile the dict
            textConverter.CatCorpusDict();
            textConverter.SrilmLmDict();

            // 10 assert it is now converted
            var arabicKeyword = textConverter.Convert(arabiziKeyword);
            // Assert.AreEqual(completeArabicKeyword, arabicKeyword);
            Assert.AreEqual(mostPopularKeyword, arabicKeyword);
        }

        [TestMethod()]
        public void ut_170727_test_recompile_corpus_rtbdaw_under_40_variants()
        {
            // 1 arabizi
            String arabiziKeyword = "rtbdaw";

            // 2 convert first pass
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";

            // 4 get all variants
            var variants = textConverter.GetAllTranscriptions(/*miniA*/arabiziKeyword);
            Assert.IsTrue(variants.Count < 40);

            // 5 get most popular keyword
            var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);
            Assert.AreNotEqual(String.Empty, mostPopularKeyword, "most popular");

            // 7 get a post containing this keyword
            var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(mostPopularKeyword, twinglyApi15Url, twinglyApiKey, true);
            if (postText == String.Empty) // if no results, look everywhere
                postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(mostPopularKeyword, twinglyApi15Url, twinglyApiKey, false);
            Assert.AreNotEqual(String.Empty, postText, "post");

            // 8 add this post to dict
            var textFrequency = new TextFrequency();
            if (textFrequency.CorpusContainsSentence(postText) == false)
                textFrequency.AddPhraseToCorpus(postText);

            // 9 recompile the dict
            textConverter.CatCorpusDict();
            textConverter.SrilmLmDict();

            // 10 assert it is now converted
            var arabicKeyword = textConverter.Convert(arabiziKeyword);
            // Assert.AreEqual(completeArabicKeyword, arabicKeyword);
            Assert.AreEqual(mostPopularKeyword, arabicKeyword);
        }

        [TestMethod()]
        public void ut_170729_test_make_sure_mzyaniin_variants_contains_no_duplicate()
        {
            String word = "mzyaniin";
            var variants = new TextConverter().GetAllTranscriptions(word);

            var nbr = variants.FindAll(m => m.Contains("مزيانين")).Count;

            //
            Assert.IsTrue(nbr == 1);
        }

        [TestMethod()]
        public void ut_170729_test_make_sure_msl7t_variants_contains_مصلحة()
        {
            // Something wrong with the taa marbouta final ? they are somehow dropped ?

            String word = "msl7t";
            var variants = new TextConverter().GetAllTranscriptions(word);

            var nbr = variants.FindAll(m => m.Contains("مصلحة")).Count;

            //
            Assert.IsTrue(nbr >= 1);
        }

        [TestMethod()]
        public void ut_170729_test_make_sure_okyl3bo_variants_are_less_than_15()
        {
            String word = "okyl3bo";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 15, "nbr variants : " + variants.Count);
        }

        [TestMethod()]
        public void ut_170729_test_make_sure_swlnak_variants_are_less_than_10()
        {
            String word = "swlnak";
            var variants = new TextConverter().GetAllTranscriptions(word);

            //
            Assert.IsTrue(variants.Count < 10, "nbr variants : " + variants.Count);
        }

        [TestMethod()]
        public void ut_170809_test_getTwinglyAccountInfo_calls_free()
        {
            // String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiUrl = "https://data.twingly.net/socialfeed/a/api/";

            // obtain current twingly key
            // String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
            List<M_TWINGLYACCOUNT> twinglyAccounts = new List<M_TWINGLYACCOUNT>();
            // var path = Server.MapPath("~/App_Data/data_" + typeof(M_TWINGLYACCOUNT).Name + ".txt");
            var path = @"C:\Users\Yahia Alaoui\Desktop\DEV\170516OADRJNLPARBZWEB\ArabicTextAnalyzer\App_Data\data_M_TWINGLYACCOUNT.txt";
            var serializer = new XmlSerializer(twinglyAccounts.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                twinglyAccounts = (List<M_TWINGLYACCOUNT>)serializer.Deserialize(reader);
            }
            String twinglyApiKey = twinglyAccounts.Find(m => m.CurrentActive == "active").ID_TWINGLYACCOUNT_API_KEY.ToString();

            var calls_free = OADRJNLPCommon.Business.Business.getTwinglyAccountInfo_calls_free(twinglyApiUrl, twinglyApiKey);
            // new TwinglyTools().upddateCountTwinglyAccount(twinglyApi15Url, twinglyApiKey, Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt"));

            //
            Assert.IsTrue(calls_free < 1000);
        }

        [TestMethod()]
        public void ut_170809_test_remove_dynamic_data()
        {
            Guid id_entry = new Guid("a04291d8-60b0-4179-bc4b-c755c34a1fac");
            // var path = @"C:\Users\Yahia Alaoui\Desktop\DEV\170516OADRJNLPARBZWEB\ArabicTextAnalyzer\App_Data\data_M_ARABIZIENTRY.txt";
            var path = @"C:\Users\Yahia Alaoui\Desktop\DEV\170516OADRJNLPARBZWEB\ArabicTextAnalyzer\App_Data\";
            // List<M_ARABIZIENTRY> entries = new TextPersist().Deserialize2<M_ARABIZIENTRY>(path);
            List<M_ARABIZIENTRY> entries = new TextPersist().Deserialize<M_ARABIZIENTRY>(path);
            Assert.IsTrue(entries.SingleOrDefault(m => m.ID_ARABIZIENTRY == id_entry) != null);
            var size = entries.Count;

            new TextPersist().RemoveItemFromList(entries, id_entry);
            Assert.IsTrue(entries.SingleOrDefault(m => m.ID_ARABIZIENTRY == id_entry) == null);
            var newsize = entries.Count;
            Assert.IsTrue(size == newsize + 1);
        }

        [TestMethod()]
        public void ut_170809_test_remove_dynamic_linked_data()
        {
            var dataPath = @"C:\Users\Yahia Alaoui\Desktop\DEV\170516OADRJNLPARBZWEB\ArabicTextAnalyzer\App_Data\";

            List<M_ARABIZIENTRY> arabizientries = new TextPersist().Deserialize<M_ARABIZIENTRY>(dataPath);
            List<M_ARABICDARIJAENTRY> arabicdarijaentries = new TextPersist().Deserialize<M_ARABICDARIJAENTRY>(dataPath);
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordEntries = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_LATINWORD>(dataPath);

            var size10 = arabizientries.Count;
            var size20 = arabicdarijaentries.Count;
            var size30 = latinWordEntries.Count;

            Guid id_arabizi = new Guid("3e72b496-7791-4903-b882-816249f26c4e");
            new TextPersist().Serialize_Delete_M_ARABIZIENTRY_Cascading(id_arabizi, dataPath);

            arabizientries = new TextPersist().Deserialize<M_ARABIZIENTRY>(dataPath);
            arabicdarijaentries = new TextPersist().Deserialize<M_ARABICDARIJAENTRY>(dataPath);
            latinWordEntries = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_LATINWORD>(dataPath);

            var size11 = arabizientries.Count;
            var size21 = arabicdarijaentries.Count;
            var size31 = latinWordEntries.Count;

            Assert.IsTrue(size11 == size10 - 1);
            Assert.AreEqual(size10, size20);
            Assert.AreEqual(size11, size21);
            Assert.IsTrue(size21 == size20 - 1);
            Assert.IsTrue(size31 == size30 - 2);
        }

        [TestMethod()]
        public void ut_170821_test_bidictContainsWord()
        {
            var contains = new TextFrequency().BidictContainsWord("inwi");

            Assert.AreEqual(true, contains);
        }

        [TestMethod()]
        public void ut_170822_test_make_sure_la_4G_does_not_get_preprocessed_to_al4g()
        {
            //
            String arabizi = "Salam Abdelilah El Hafidi, inwi fi khidmatikoum. Bach takhdem likoum la 4G awalan khasekoum tkounou tatwajdou be madina fiha taghtya dial la 4G ou lhatef dialekoum khas tekoun fihe lkhidma dial la 4G metwafra ou akhiran khase tkoun 3andkoum albitaka SIM fiha hata hya lkhidma dial la 4G, bach tghayerou lbitaka dial inwi le 4G twajhou men fadlkoum lwakala dial inwi, taghyire be 30dh, ila kan 3andkoum ichtirak taghyire al bitaka majani. inwi tatchkarkoum 3la ikhlas dialkoum.";
            String nottarget = "al4g";

            //
            var textConverter = new TextConverter(String.Empty);
            arabizi = textConverter.Preprocess_ma_ch(arabizi);
            // arabizi = textConverter.Preprocess_le(arabizi);
            arabizi = textConverter.Preprocess_al_wa(arabizi);
            arabizi = textConverter.Preprocess_al(arabizi);
            arabizi = textConverter.Preprocess_bezzaf(arabizi);
            arabizi = textConverter.Preprocess_ahaha(arabizi);
            String miniArabiziKeyword = textConverter.Preprocess_al(arabizi);

            //
            Assert.IsFalse(miniArabiziKeyword.Contains(nottarget));
        }

        [TestMethod()]
        public void ut_170829_test_Preprocess_emoticons()
        {
            String arabizi = "kangololkom😂😂😂😂😂";
            String expected = "kangololkom";

            //
            var textConverter = new TextConverter();
            var cleansed = textConverter.Preprocess_emoticons(arabizi);

            Assert.AreEqual(expected, cleansed);
        }

        [TestMethod()]
        public void ut_171018_test_firstpass_and_Preprocess_arabic_comma_and_unicode_special_chars()
        {
            String arabizi = "wla twil,drari";
            String expected = "twil WLA، Drari";

            // consume google/bing apis
            var BingSpellcheckAPIKey = "1e14edea7a314d469541e8ced0af38c9";
            var GoogleTranslationApiKey = "AIzaSyBqnBEi2fRhKRRpcPCJ-kwTl0cJ2WcQRJI";
            var arabiziTextToMsaFirstPass = new TranslationTools(BingSpellcheckAPIKey, GoogleTranslationApiKey).CorrectTranslate(arabizi);

            Assert.AreEqual("twil WLA، Drari", arabiziTextToMsaFirstPass);

            arabizi = "twil WLA، Drari";
            expected = "twil WLA ,  Drari";

            //
            var textConverter = new TextConverter(String.Empty);
            var cleansed = textConverter.Preprocess_arabic_comma(arabizi);

            Assert.AreEqual(expected, cleansed);
        }

        [TestMethod()]
        public void ut_171023_test_firstpass_bing_on_text_with_quote()
        {
            String arabizi = "Bsahtek 7biba dyali btol l3mer inchalah wbax matmaniti lah ykhalilek marwan w ya39ob walidik khotek je t'embrasse bonne voyage bb	";
            // String expected = "Bsahtek 7biba dyali btol l3mer inchalah wbax matmaniti lah ykhalilek marwan w ya39ob walidik khotek je t'embrasse bonne voyage bb";
            String expected = "Bsahtek 7biba dyali bitola 3amer inchallah wbaxmatmaniti lah ykhalik marwan w ya39ob walidik khotek je t'embrasse bonne voyage bb";

            // consume google/bing apis
            var BingSpellcheckAPIKey = "1e14edea7a314d469541e8ced0af38c9";
            var correctedWord = new BingSpellCheckerApiTools().bingSpellcheckApi(arabizi, BingSpellcheckAPIKey);

            Assert.AreEqual(expected, correctedWord);
        }

        [TestMethod()]
        public void ut_171023_test_firstpass_google_on_text_with_lah_and_w()
        {
            // FAIL : should work when we add a bidict preprocess before google/bing
            // See VSO 562 (https://namatedev.visualstudio.com/170105OADRJNLP/_workitems/edit/562)

            String arabizi = "btol l3mer inchalah wbax matmaniti lah ykhalilek marwan w ya39ob bonne voyage bb	";
            String minexpected = "btol l3mer inchalah wbax matmaniti لاه ykhalilek مروان ث ya39ob جيدة ب رحلة";
            String preferablyexpected = "btol l3mer inchalah wbax matmaniti الله ykhalilek مروان و ya39ob جيدة bébé رحلة";

            var GoogleTranslationApiKey = "AIzaSyBqnBEi2fRhKRRpcPCJ-kwTl0cJ2WcQRJI";
            var translatedLatinWord = new GoogleTranslationApiTools(GoogleTranslationApiKey).getArabicTranslatedWord(arabizi);

            Assert.AreEqual(minexpected, translatedLatinWord);
            Assert.AreEqual(preferablyexpected, translatedLatinWord);
        }

        [TestMethod()]
        public void ut_171023_test_firstpass_bing_should_work_when_no_suggestions_available()
        {
            String arabizi = "Salam Houditta Houda, inwi fi khidmatikoum.Bach telghiw techghil l automatiqui dyal bedel sotek, ma3likoum ghir tresslou STOP f sms l ra9m 789.";
            String expected = "Salam Houditta Houda, inwi fi khidmatikoum.Bach telghiw techghil l automatiqui dyal bedel sotek, ma3likoum ghir tresslou STOP f sms l ra9m 789.";

            // consume google/bing apis
            var BingSpellcheckAPIKey = "1e14edea7a314d469541e8ced0af38c9";
            var correctedWord = new BingSpellCheckerApiTools().bingSpellcheckApi(arabizi, BingSpellcheckAPIKey);

            // bing finds nothing so no change
            Assert.AreEqual(expected, correctedWord);
        }

        [TestMethod()]
        public void ut_171023_test_firstpass_bing_should_work_when_one_suggestion_available()
        {
            //
            String arabizi = "je t'emebrasse bonne voyage";
            String expected = "je t'embrasse bonne voyage";

            // consume google/bing apis
            var BingSpellcheckAPIKey = "1e14edea7a314d469541e8ced0af38c9";
            var correctedWord = new BingSpellCheckerApiTools().bingSpellcheckApi(arabizi, BingSpellcheckAPIKey);

            // bing finds one so change
            Assert.AreEqual(expected, correctedWord);
        }

        [TestMethod()]
        public void ut_171023_test_firstpass_google_on_mispelled_word_ex_automatiqui_make_sure_it_does_not_work()
        {
            String arabizi = "automatiqui";
            String expected = "automatiqui";

            var GoogleTranslationApiKey = "AIzaSyBqnBEi2fRhKRRpcPCJ-kwTl0cJ2WcQRJI";
            var translatedLatinWord = new GoogleTranslationApiTools(GoogleTranslationApiKey).getArabicTranslatedWord(arabizi);

            Assert.AreEqual(expected, translatedLatinWord);
        }

        [TestMethod()]
        public void ut_171023_test_firstpass_bing_and_google_on_mispelled_word_ex_automatiqui()
        {
            String arabizi = "automatiqui";
            String expected = "تلقائي";

            // consume bing apis
            var BingSpellcheckAPIKey = "1e14edea7a314d469541e8ced0af38c9";
            arabizi = new BingSpellCheckerApiTools().bingSpellcheckApi(arabizi, BingSpellcheckAPIKey);

            // google
            var GoogleTranslationApiKey = "AIzaSyBqnBEi2fRhKRRpcPCJ-kwTl0cJ2WcQRJI";
            var translatedLatinWord = new GoogleTranslationApiTools(GoogleTranslationApiKey).getArabicTranslatedWord(arabizi);

            Assert.AreEqual(expected, translatedLatinWord);
        }

        [TestMethod()]
        public void ut_171025_test_google_translate_api_on_notranslate()
        {
            String arabizi = "Ana khdit f7alom mn <span class='notranslate'>pull and bear</span>";
            String expected = "khdit آنا f7alom دقيقة <span class='notranslate'>pull and bear</span>";

            var GoogleTranslationApiKey = "AIzaSyBqnBEi2fRhKRRpcPCJ-kwTl0cJ2WcQRJI";
            var translatedLatinWord = new GoogleTranslationApiTools(GoogleTranslationApiKey).getArabicTranslatedWord(arabizi, "html");

            Assert.AreEqual(expected, translatedLatinWord);
        }

        [TestMethod()]
        public void ut_171025_test_clean_multiple_occurrences_notranslate()
        {
            String arabizi = "<span class='notranslate'>Ana</span> khdit f7alom mn <span class='notranslate'>pull and bear</span>";
            String expected = "Ana khdit f7alom mn pull and bear";

            // clean <span class='notranslate'>
            arabizi = new Regex(@"<span class='notranslate'>(.*?)</span>").Replace(arabizi, "$1");

            Assert.AreEqual(expected, arabizi);
        }

        [TestMethod()]
        public void ut_171025_test_ReplaceArzByArFromBidict_on_two_words_entry()
        {
            String arabizi = "maroc telecom";
            String expected = "<span class='notranslate'>IAM</span>";

            // clean before google/bing : o w
            arabizi = new TextFrequency(@"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\").ReplaceArzByArFromBidict(arabizi);

            Assert.AreEqual(expected, arabizi);
        }

        [TestMethod()]
        public void ut_171025_test_ReplaceArzByArFromBidict_on_two_words_entries_in_a_phrase()
        {
            String arabizi = "Le maroc zwyine wa maroc telecom mjahda";
            String expected = "Le <span class='notranslate'>المغرب</span> zwyine wa <span class='notranslate'>IAM</span> mjahda";

            // clean before google/bing : o w
            arabizi = new TextFrequency(@"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\").ReplaceArzByArFromBidict(arabizi);

            Assert.AreEqual(expected, arabizi);
        }

        [TestMethod()]
        public void ut_171114_test_CountStringOccurrences_should_find_full_words_not_within_words()
        {
            String arabicsource = "خويا أنا راجوى ولكن زكريا ما بقا ش كيلعب بسيفه رسمية عيدان دقيقة الحسن قلب على راسو أكثر إدارة تعليمية ما كاينا ش التي تحمى لعب دقيقة بحال زيارتها الانتقالة خصوصا الغريم مرحاض و بالفعل وقعنا فأخطأ بحال هادوك مثل صبحي";
            String nerword = "س";

            // clean before google/bing : o w
            var count = TextFrequency.CountStringOccurrences(arabicsource, nerword);
            var expectedcount = 0;

            Assert.AreEqual(expectedcount, count, "1");

            //
            nerword = "الحسن";
            count = TextFrequency.CountStringOccurrences(arabicsource, nerword);
            expectedcount = 1;

            Assert.AreEqual(expectedcount, count, "2");
        }

        [TestMethod()]
        public void ut_171117_test_Preprocess_SilentVowels()
        {
            String arabicsource = "3i9o barak";
            String expectedoutarabicsource = "3i9o barak_VOW_";

            //
            arabicsource = new TextConverter().Preprocess_SilentVowels(arabicsource);

            Assert.AreEqual(expectedoutarabicsource, arabicsource, "1");

            //
            arabicsource = "k";
            expectedoutarabicsource = "k";
            arabicsource = new TextConverter().Preprocess_SilentVowels(arabicsource);

            Assert.AreEqual(expectedoutarabicsource, arabicsource, "2");
        }

        [TestMethod()]
        public void ut_171123_test_speed_read_arabic_dict()
        {
            // On c# reading a file of 564000 words take usually less than 1 sec
            // Vs +1.5 sec on perl ??!

            var watch = Stopwatch.StartNew();
            var vocab = new Dictionary<String, int>();
            string workingDirectoryLocation = @"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\";
            var pathToModels = workingDirectoryLocation + @"models\";
            var pathToArabicDictFile = pathToModels + @"moroccan-arabic-dict";
            foreach (string line in File.ReadLines(pathToArabicDictFile))
            {
                // chomp($line);
                var trimmedline = line.TrimEnd(new char[] { '\n' });

                // $vocab{$line}= 1;
                vocab[trimmedline] = 1;
            }
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 1000, watch.ElapsedMilliseconds.ToString());
        }

        [TestMethod()]
        public void ut_171127_test_arabiziapi_getToken_and_use_it()
        {
            // get token
            var clientId = "f6dkqniUpUskiJA";
            var clientSecret = "6sUKVfD1UQOYWe5";
            var authenticateUrl = "http://localhost:50037/api/Arabizi/Authenticate/?" + "clientId=" + clientId + "&clientSecret=" + clientSecret;
            HttpResponseMessage tokenResponse = new HttpClient().GetAsync(authenticateUrl).Result;
            if (tokenResponse.IsSuccessStatusCode)
            {
                var data = tokenResponse.Content.ReadAsStringAsync().Result;
                // Assert.AreEqual("\"LzY2XeLWu1IjCtsmGboDkppSRb8GWGfi0A/8ndFeHwJ3UqFwmCpP6wngxDk7GNabLrJ20+u7NN1obPuDIdxhE+4wv6++0embViFHYDPEfpCJknaOVXuhNSyqVQEjMErT\"", data);
                // data = data.Trim(new char[] { '\"', '\'' });
                data = data.Substring(3, 128);

                //
                var arabiziUrl = "http://localhost:50037/api/Arabizi/GetArabicDarijaEntry/?text=tajriba"
                    + "&token="
                    + data;
                // var arabiziUrl = "http://localhost:50037/api/Arabizi?text=maghrib";
                HttpResponseMessage response = new HttpClient().GetAsync(arabiziUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var data2 = response.Content.ReadAsStringAsync().Result;
                    // var dynamicObject = JsonConvert.DeserializeObject(data2);
                    dynamic dynamicObject = JObject.Parse(data2);
                    String trad = Convert.ToString(dynamicObject.M_ARABICDARIJAENTRY.ArabicDarijaText);
                    Assert.IsTrue(("تجريبى" == trad) || ("تجريبة" == trad));
                }
            }
        }

        [TestMethod()]
        public void ut_171130_test_ReplaceArzByArFromBidict_with_BUTTRANSLATEPERL()
        {
            String arabizi = "Hadi bant l9ahba kokchi katbakkih bl3ani, j'ai dit kokchi";
            String expected = "<span class='notranslate'>هذه</span> bant l9ahba <span class='notranslate BUTTRANSLATEPERL'>kolchi</span> katbakkih bl3ani, j'ai dit <span class='notranslate BUTTRANSLATEPERL'>kolchi</span>";

            //
            arabizi = new TextFrequency(@"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\").ReplaceArzByArFromBidict(arabizi);

            Assert.AreEqual(expected, arabizi);
        }

        [TestMethod()]
        public void ut_171130_test_restore_BUTTRANSLATEPERL_in_perl()
        {
            String arabizi = "<span class='notranslate'>هذه</span> bant l9ahba <span class='notranslate BUTTRANSLATEPERL'>kolchi</span> katbakkih bl3ani, j'ai dit <span class='notranslate BUTTRANSLATEPERL'>kolchi</span>";
            String expected = "<span class='notranslate'>هذه</span> bant l9ahba kolchi katbakkih bl3ani, j'ai dit kolchi";

            //
            arabizi = new Regex(@"<span class='notranslate BUTTRANSLATEPERL'>(.*?)</span>").Replace(arabizi, "$1");

            Assert.AreEqual(expected, arabizi);
        }

        
    }
}
