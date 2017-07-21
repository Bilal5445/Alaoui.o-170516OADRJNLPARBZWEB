using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextFrequency
    {
        //
        const String pathToArabiziEnv = @"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\";
        const String pathToCorpus = pathToArabiziEnv + @"corpus\";
        const String pathToDictFile = pathToCorpus + @"170426_extended_dict.txt";

        // FB_KEYWORD getFBKeywordInfoFromFBViaTwingly(String fbKeywordKeyword)

        public void AddPhraseToCorpus(String post)
        {
            // script to add a sentence to corpus
            File.AppendAllText(pathToDictFile, post + Environment.NewLine);
        }

        public int GetCorpusNumberOfLine()
        {
            return File.ReadLines(pathToDictFile).Count();
        }
    }
}
