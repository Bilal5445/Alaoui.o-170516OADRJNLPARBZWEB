using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArabicTextAnalyzer.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ArabicTextAnalyzer.Business.Provider;

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
        public void ut_170718_test_replace_latin_words()
        {
            String arabicDarijaText = "بتوفيق لقرد .... وصل عمالك حسبان او تلتفت الجمهور يغرد خرج سرب او يفقه شيئا في قانون المناصر waghalibiyatohom تابعين المارق و زرق ... المناصر دور تشجع فقط ولا بغا تغير ينخرط flmaktab ويترشح";

            var arabicDarijaText2 = TextTools.HighlightExtractedLatinWords(arabicDarijaText);

            Assert.AreNotEqual(arabicDarijaText, arabicDarijaText2);
        }
    }
}