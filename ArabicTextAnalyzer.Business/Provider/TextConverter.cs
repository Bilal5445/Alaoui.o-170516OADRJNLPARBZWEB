﻿using System;
using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OADRJNLPCommon.Business;
using System.Text;
using System.Globalization;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextConverter : ITextConverter
    {
        // -> this is the folder containing the script
        private const string workingDirectoryLocation = PathConstant.pathToArabiziEnv;

        // -> the output file
        private const String pathToExample = workingDirectoryLocation + @"example\";
        // private const string outputFileLocation = pathToExample + "small-example.7.charTransl";
        private const string outputFileLocation = pathToExample + "small-example.7.charWordTransl";
        // -> the input file(saw that the script only works with this file)
        private const string inputFileLocation = pathToExample + "small-example.arabizi";
        // -> the script : full pipeline
        private const string processFileLocation = workingDirectoryLocation + @"RUN_transl_pipeline.sh";

        public string Convert(string source)
        {
            // preprocess (eg: ma/ch)
            source = Preprocess_wa_ma_ch(source);
            source = Preprocess_li_ma_ch(source);
            source = Preprocess_ma_ch(source);
            source = Preprocess_le(source);
            source = Preprocess_al_wa(source);
            source = Preprocess_al(source);
            source = Preprocess_bezzaf(source);
            source = Preprocess_ahaha(source);
            source = Preprocess_3_m_i_f_z_a_j_l_etc(source);
            source = Preprocess_emoticons(source);

            // to arabizi file
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
            return output.TrimEnd('\r', '\n');
        }

        public string Preprocess_wa_ma_ch(string arabizi)
        {
            // on the contrary, we need to separate
            String pattern = RegexConstant.waMaChRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "wa ma$2ch", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_li_ma_ch(string arabizi)
        {
            // on the contrary, we need to separate
            String pattern = RegexConstant.liMaChRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "li ma$2ch", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_ma_ch(string arabizi)
        {
            // on the contrary, we need to separate
            String pattern = RegexConstant.maChRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "ma $1 ch", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_le(string arabizi)
        {
            // ex : on mix le fr avec l'arabe : 'le' dans le sens l'article 'al'
            String pattern = RegexConstant.leRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al", RegexOptions.IgnoreCase);

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

            //
            String pattern = RegexConstant.alWaRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al$2 wal$6", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_al(string arabizi)
        {
            // wa3acha al malik => wa3acha almalik
            String pattern = RegexConstant.alRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al$2", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_bezzaf(string arabizi)
        {
            //bzf
            //bezzaf
            //bezzaaaaaaaaaaaaf
            //beeeeezzaf
            //bezzzzzzzzzaf
            //beeeeeezzzzzzzzzzaaaaaaaaaf
            
            String pattern = @"\bbe*z+a*f\b";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "bzf", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_3_m_i_f_z_a_j_l_etc(string arabizi)
        {
            //bzf
            //bezzaf
            //bezzaaaaaaaaaaaaf
            //beeeeezzaf
            //bezzzzzzzzzaf
            //beeeeeezzzzzzzzzzaaaaaaaaaf

            String pattern = @"m{3,}";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "mm", RegexOptions.IgnoreCase);

            pattern = @"i{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "ii", RegexOptions.IgnoreCase);

            pattern = @"f{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "ff", RegexOptions.IgnoreCase);

            pattern = @"z{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "zz", RegexOptions.IgnoreCase);

            pattern = @"a{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "aa", RegexOptions.IgnoreCase);

            pattern = @"j{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "jj", RegexOptions.IgnoreCase);

            pattern = @"l{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "ll", RegexOptions.IgnoreCase);

            pattern = @"o{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "oo", RegexOptions.IgnoreCase);

            pattern = @"n{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "nn", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_ahaha(string arabizi)
        {
            //hahahahahahaha
            //ahahahaha
            //ahahahahahahahahaha
            //hahahaha
            //ahahah

            String pattern = @"\b(a|h){0,1}(ha|ha){3,}(h|a){0,1}\b";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "ههه", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        public string Preprocess_emoticons(string arabizi)
        {
            // string text = "a\u2705b\U0001f52ec\u26f1d\U0001F602e\U00010000";
            string cleansed = RemoveOtherSymbols(arabizi);
            // Console.WriteLine(cleansed);

            return cleansed;
        }

        static string RemoveOtherSymbols(string text)
        {
            // TODO: Handle malformed strings (e.g. those
            // with mismatched surrogate pairs)
            StringBuilder builder = new StringBuilder();
            int index = 0;
            while (index < text.Length)
            {
                // Full Unicode character
                int units = char.IsSurrogate(text, index) ? 2 : 1;
                UnicodeCategory category = char.GetUnicodeCategory(text, index);
                int ch = char.ConvertToUtf32(text, index);
                if (category == UnicodeCategory.OtherSymbol)
                {
                    Console.WriteLine($"Skipping U+{ch:x} {category}");
                }
                else
                {
                    Console.WriteLine($"Keeping U+{ch:x} {category}");
                    builder.Append(text, index, units);
                }
                index += units;
            }
            return builder.ToString();
        }

        public List<String> GetAllTranscriptions(String arabiziWord)
        {
            //
            File.WriteAllText(workingDirectoryLocation + "arabiziword", arabiziWord);

            // script to create all variants only
            string variantsProcFileLoc = workingDirectoryLocation + @"RUN_transl_transcm.sh";
            string outputVariantsFileLoc = pathToExample + "out.variants.txt";

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo(variantsProcFileLoc)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true,
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();

            var output = File.ReadAllLines(outputVariantsFileLoc).ToList<String>();

            return output;
        }

        public void CatCorpusDict()
        {
            string corpusProcFileLoc = workingDirectoryLocation + @"RUN_transl_cat.sh";

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
            String srilmLmProcFileLoc = workingDirectoryLocation + @"RUN_transl_srilm.sh";

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