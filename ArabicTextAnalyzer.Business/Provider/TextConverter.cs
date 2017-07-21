using System;
using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextConverter : ITextConverter
    {
        // -> the output file
        private const String pathToArabiziEnv = @"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\";
        // private const String pathToArabiziEnv = @"C:\script\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\";
        private const String pathToExample = pathToArabiziEnv + @"example\";
        // private const string outputFileLocation = pathToExample + "small-example.7.charTransl";
        private const string outputFileLocation = pathToExample + "small-example.7.charWordTransl";
        // -> the input file(saw that the script only works with this file)
        private const string inputFileLocation = pathToExample + "small-example.arabizi";
        // -> the script : full pipeline
        private const string processFileLocation = pathToArabiziEnv + @"RUN_transl_pipeline.sh";
        // -> this is the folder containing the script
        private const string workingDirectoryLocation = pathToArabiziEnv;

        public string Convert(string source)
        {
            //
            File.WriteAllText(inputFileLocation, source);

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(processFileLocation)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();

            var output = File.ReadAllText(outputFileLocation);
            return output.TrimEnd('\r', '\n'); ;
        }

        public List<String> GetAllTranscriptions(String arabiziWord)
        {
            //
            File.WriteAllText(pathToArabiziEnv + "arabiziword", arabiziWord);

            // script to create all variants only
            string variantsProcFileLoc = pathToArabiziEnv + @"RUN_transl_transcm.sh";
            string outputVariantsFileLoc = pathToExample + "out.variants.txt";

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(variantsProcFileLoc)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true,
                // CreateNoWindow = false,
                // WindowStyle = ProcessWindowStyle.Hidden
            };
            // processInformation.Arguments = arabiziWord;
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();

            var output = File.ReadAllLines(outputVariantsFileLoc).ToList<String>();

            return output;
        }

        public void CatCorpusDict()
        {
            String catCorpusProcFileLoc = pathToArabiziEnv + @"cat corpus/170426_extended_dict.txt | perl scripts/create-dictionary.pl > models/moroccan-arabic-dict";

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(catCorpusProcFileLoc)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();
        }

        public void SrilmLmDict()
        {
            String srilmLmProcFileLoc = pathToArabiziEnv + @"srilm-1.7.2/bin/cygwin64/ngram-count -text corpus/170426_extended_dict.txt -order 3 -lm lm/moroccan_arabic_corpus_01.lm";

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(srilmLmProcFileLoc)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();
        }
    }
}