using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
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
        private String pathToBidictFileArz;
        private String pathToBidictFileAr;
        private String pathToNER;
        private String pathToNERFile_brands;
        private String pathToNotNERFile;

        // for UT only
        public TextFrequency(String pathToArabiziEnv)
        {
            string workingDirectoryLocation = pathToArabiziEnv;

            //
            pathToCorpus = workingDirectoryLocation + @"corpus\";
            pathToDictFile = pathToCorpus + @"170426_extended_dict.txt";
            pathToBidict = workingDirectoryLocation + @"arabizi-arabic-bitext\";
            pathToBidictFileArz = pathToBidict + @"arabizi-arabic-bitext.arz";
            pathToBidictFileAr = pathToBidict + @"arabizi-arabic-bitext.ar";
            pathToNER = workingDirectoryLocation + @"ner\";
            pathToNERFile_brands = pathToNER + @"entities-brand.txt";
            pathToNotNERFile = pathToNER + @"not-entities.txt";
        }

        public TextFrequency()
        {
            // -> this is the folder containing the script
            string workingDirectoryLocation = new PathConstant().pathToArabiziEnv;

            //
            pathToCorpus = workingDirectoryLocation + @"corpus\";
            pathToDictFile = pathToCorpus + @"170426_extended_dict.txt";
            pathToBidict = workingDirectoryLocation + @"arabizi-arabic-bitext\";
            pathToBidictFileArz = pathToBidict + @"arabizi-arabic-bitext.arz";
            pathToBidictFileAr = pathToBidict + @"arabizi-arabic-bitext.ar";
            pathToNER = workingDirectoryLocation + @"ner\";
            pathToNERFile_brands = pathToNER + @"entities-brand.txt";
            pathToNotNERFile = pathToNER + @"not-entities.txt";
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

        public int GetBidictNumberOfLine()
        {
            return File.ReadLines(pathToBidictFileArz).Count();
        }

        public bool BidictContainsWord(string domain)
        {
            // make it one line
            domain = domain.Replace("\r\n", " ");

            foreach (string line in File.ReadLines(pathToBidictFileArz))
                if (Regex.IsMatch(line, @"\b" + domain + @"\b", RegexOptions.IgnoreCase))
                    return true; // and stop reading lines

            return false;
        }

        public String ReplaceArzByArFromBidict(String arabiziText)
        {
            var arzlines = File.ReadAllLines(pathToBidictFileArz);
            var arlines = File.ReadAllLines(pathToBidictFileAr);

            //
            for (int i = 0; i < arzlines.Count(); i++)
            {
                var arz = arzlines[i];

                // contains case insensitive
                var regex = new Regex(@"\b" + arz + @"\b(?![^<>]*</|[^><]*>)", RegexOptions.IgnoreCase);    // ie : find word but anot already surrended by tags
                MatchCollection matches = regex.Matches(arabiziText);
                foreach (Match match in matches)
                {
                    // MC301117 default bidict become 'notranslate' in both binggoogle and in perl
                    // but when the arlines contains a BUTTRANSLATEPERL, then become 'notranslate' in binggoogle, but 'translate' in perl
                    // if (match.Value.StartsWith("<span class='notranslate'>"))
                    if (match.Value.StartsWith("<span class='notranslate"))
                        continue;

                    var arLineCouple = arlines[i].Split(new char[] { '\t' });
                    var arRemplacment = arLineCouple[0];
                    if (arLineCouple.Length > 1 && arLineCouple[1] == "BUTTRANSLATEPERL")
                        arabiziText = regex.Replace(arabiziText, "<span class='notranslate BUTTRANSLATEPERL'>" + arRemplacment + "</span>");
                    else
                        arabiziText = regex.Replace(arabiziText, "<span class='notranslate'>" + arRemplacment + "</span>");
                }
            }

            return arabiziText;
        }

        public String MarkAsIgnore_URL(String arabiziText)
        {
            var pattern = @"\b(http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?)\b";

            arabiziText = Regex.Replace(arabiziText, pattern, "<span class='notranslate'>$1</span>", RegexOptions.IgnoreCase);

            return arabiziText;
        }

        public bool NERContainsWord_brands(string domain)
        {
            // make it one line
            domain = domain.Replace("\r\n", " ").Trim();

            foreach (string line in File.ReadLines(pathToNERFile_brands))
                if (Regex.IsMatch(line.Trim(), @"\b" + domain + @"\b", RegexOptions.IgnoreCase))
                    return true; // and stop reading lines

            return false;
        }

        public void GetManualEntities(String source, List<TextEntity> rosetteNERs)
        {
            List<TextEntity> manualNERs = new List<TextEntity>();

            // search in manual NERs
            foreach (string line in File.ReadLines(pathToNERFile_brands))
            {
                var wordSlashType = line.Split(new char[] { '\t' });
                var nerword = wordSlashType[0].Trim();
                var nertype = wordSlashType[1].Trim();

                // contains case insensitive
                // if (source.IndexOf(nerword, StringComparison.InvariantCultureIgnoreCase) >= 0)
                var occurrences = CountStringOccurrences(source, nerword);
                if (occurrences > 0)
                {
                    // add only if not already in manual entities
                    TextEntity existingManualEntity = manualNERs.FirstOrDefault(m => m.Mention == nerword);
                    if (existingManualEntity == null)
                    {
                        manualNERs.Add(new TextEntity
                        {
                            Count = occurrences,
                            Mention = nerword,
                            Type = nertype
                        });
                    }
                }
            }

            // merge with NERs from previous stage : rosette
            // add only if not already in entities (by previous stage : rosette)
            //// otherwise increment
            // otherwise do not increment because we do reset anymore count to 0 after rosette step
            foreach (var manualNER in manualNERs)
            {
                TextEntity existingEntity = rosetteNERs.FirstOrDefault(rosetteNER => rosetteNER.Mention == manualNER.Mention);
                if (existingEntity == null)
                    rosetteNERs.Add(manualNER);
                /*else
                    existingEntity.Count += manualNER.Count;*/
            }
        }

        public static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            /*int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;*/
            return Regex.Matches(text, @"\b" + pattern + @"\b", RegexOptions.IgnoreCase).Count;
        }

        public bool NERStartsWithWord_brands(string domain, out String type)
        {
            // make it one line
            domain = domain.Replace("\r\n", " ");

            foreach (string line in File.ReadLines(pathToNERFile_brands))
            {
                // if (line.StartsWith(domain, StringComparison.InvariantCultureIgnoreCase))
                var wordSlashType = line.Split(new char[] { '\t' });
                if (wordSlashType[0].Trim().Equals(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    type = line.Split(new char[] { '\t' })[1].Trim();
                    return true; // and stop reading lines
                }
            }

            type = String.Empty;
            return false;
        }

        public bool NotNERContainsWord(string word)
        {
            // make it one line
            word = word.Replace("\r\n", " ");

            foreach (string line in File.ReadLines(pathToNotNERFile))
                if (Regex.IsMatch(line, @"\b" + word + @"\b", RegexOptions.IgnoreCase))
                    return true; // and stop reading lines

            return false;
        }

        public int GetEntitiesCount()
        {
            return File.ReadLines(pathToNERFile_brands).Count();
        }

        public int GetArabiziEntriesCount(String dataPath)
        {
            List<M_ARABIZIENTRY> arabiziEntries = new TextPersist().Deserialize<M_ARABIZIENTRY>(dataPath);

            return arabiziEntries.Count;
        }

        public int GetLatinEntriesCount(String dataPath)
        {
            // load/deserialize M_ARABICDARIJAENTRY_LATINWORD
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordEntries = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_LATINWORD>(dataPath);

            // MC092617 do not count latin word that google has translated
            return latinWordEntries.Where(m => String.IsNullOrWhiteSpace(m.Translation) == true).ToList().Count;
        }

        public double GetRatioLatinWordsOnEntries(String dataPath)
        {
            double ratio = GetLatinEntriesCount(dataPath) / (double)GetArabiziEntriesCount(dataPath);

            return ratio;
        }
    }
}
