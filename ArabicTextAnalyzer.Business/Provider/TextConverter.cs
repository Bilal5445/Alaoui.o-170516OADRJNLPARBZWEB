using System;
using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextConverter : ITextConverter
    {
        // -> the output file
        private const String pathToArabiziEnv = @"C:\Users\Yahia Alaoui\Desktop\DEV\17028OADRJNLPARBZ\";
        // private const String pathToArabiziEnv = @"C\script\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\";
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
            // preprocess (eg: ma/ch)
            source = Preprocess_ma_ch(source);
            source = Preprocess_al_wa(source);

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

        public string Preprocess_ma_ch(string arabizi)
        {
            // this step re-assemble "ma VERBch" => "maVERBmc"
            String pattern = @"\bma \b(.+)ch\b";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "ma$1ch");

            return miniArabiziKeyword;
        }

        public string Preprocess_al_wa(string arabizi)
        {
            // this step re-assemble "al WORD1 wa WORD2" => "alWORD1 wa alWORD2"
            //al3adala wa atanmia
            //al 3adala wa atanmia
            //al 3adala o atanmia
            //al 3adala ou atanmia
            //al 3adala wl atanmia
            //al 3adala wlatanmia
            //l 3adala wlatanmia
            // Al houb wa al hazka
            // Al hmak ou dsara
            // Al houb wa el hazka
            // String pattern = @"\b(al|l|el) *([A-Za-z0-9éèàâê]+\b) (wou|wal|wel|wl|ou|o|wa) *([A-Za-z0-9éèàâê]+\b)";
            String pattern = @"\b(al|l|el) *([A-Za-z0-9éèàâê]+\b) ((wou|wal|wel|wl|ou|o|wa) *(al|l|el|)) *([A-Za-z0-9éèàâê]+\b)";
            // String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al$2 wa al$4", RegexOptions.IgnoreCase);
            // String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al$2 wa al$6", RegexOptions.IgnoreCase);
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al$2 wal$6", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
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
            string corpusProcFileLoc = pathToArabiziEnv + @"RUN_transl_cat.sh";

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(corpusProcFileLoc)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true,
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();
        }

        public void SrilmLmDict()
        {
            String srilmLmProcFileLoc = pathToArabiziEnv + @"RUN_transl_srilm.sh";

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(srilmLmProcFileLoc)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true,
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();
        }
    }
}