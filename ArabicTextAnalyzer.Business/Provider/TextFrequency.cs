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
            // make it one line
            post = post.Replace("\r\n", " ");

            // script to add a sentence to corpus
            File.AppendAllText(pathToDictFile, Environment.NewLine + post); 
        }

        public void DropPhraseFromCorpus(String post)
        {
            var fileName = pathToDictFile;

            var tempFile = Path.GetTempFileName();
            var linesToKeep = File.ReadLines(fileName).Where(l => l != post);

            File.WriteAllLines(tempFile, linesToKeep);

            File.Delete(fileName);
            File.Move(tempFile, fileName);
        }

        public int GetCorpusNumberOfLine()
        {
            return File.ReadLines(pathToDictFile).Count();
        }

        public bool CorpusContainsSentence(string domain)
        {
            foreach (string line in File.ReadLines(pathToDictFile))
                if (domain == line)
                    return true; // and stop reading lines

            return false;
        }
    }
}
