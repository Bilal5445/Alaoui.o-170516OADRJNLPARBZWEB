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
    }
}
