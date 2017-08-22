using OADRJNLPCommon.Business;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextFrequency
    {
        public String pathToCorpus { get; }
        public String pathToDictFile { get; }
        private String pathToBidict;
        private String pathToBidictFile;

        public TextFrequency ()
        {
            pathToCorpus = PathConstant.pathToArabiziEnv + @"corpus\";
            pathToDictFile = pathToCorpus + @"170426_extended_dict.txt";
            pathToBidict = PathConstant.pathToArabiziEnv + @"arabizi-arabic-bitext\";
            pathToBidictFile = pathToBidict + @"arabizi-arabic-bitext.arz";
        }

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

        public int GetCorpusWordCount()
        {
            return File.ReadAllText(pathToDictFile).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public bool CorpusContainsSentence(string domain)
        {
            // make it one line
            domain = domain.Replace("\r\n", " ");

            foreach (string line in File.ReadLines(pathToDictFile))
                if (domain == line)
                    return true; // and stop reading lines

            return false;
        }

        public bool CorpusContainsWord(string domain)
        {
            // make it one line
            domain = domain.Replace("\r\n", " ");

            foreach (string line in File.ReadLines(pathToDictFile))
                if (Regex.IsMatch(line, @"\b" + domain + @"\b", RegexOptions.IgnoreCase))
                    return true; // and stop reading lines

            return false;
        }

        public bool BidictContainsWord(string domain)
        {
            // make it one line
            domain = domain.Replace("\r\n", " ");

            foreach (string line in File.ReadLines(pathToBidictFile))
                if (Regex.IsMatch(line, @"\b" + domain + @"\b", RegexOptions.IgnoreCase))
                    return true; // and stop reading lines

            return false;
        }
    }
}
