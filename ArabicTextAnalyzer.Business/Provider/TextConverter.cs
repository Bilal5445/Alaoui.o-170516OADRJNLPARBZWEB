using System;
using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OADRJNLPCommon.Business;
using System.Text;
using System.Globalization;
using System.Web;
using System.Security;

namespace ArabicTextAnalyzer.Business.Provider
{
    public static class Securer
    {
        public static SecureString ConvertToSecureString(this string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    var securePassword = new SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }
    }

    public class TextConverter : ITextConverter
    {
        // -> this is the folder containing the script
        private string workingDirectoryLocation = new PathConstant().pathToArabiziEnv;

        // -> the output file
        private String pathToExample;
        private string outputFileLocation;
        // -> the input file(saw that the script only works with this file)
        private string inputFileLocation;
        private String inputFileLocationFileOnly;
        // -> the script : full pipeline
        private string translPipelineScript;
        private string translPipelineScriptFileNdExtOnly;

        // for UT only
        public TextConverter(String utWorkingDirectoryLocation)
        {
            workingDirectoryLocation = utWorkingDirectoryLocation;
        }

        public TextConverter()
        {
            pathToExample = workingDirectoryLocation + @"example\";
            inputFileLocation = pathToExample + "small-example.arabizi";
            outputFileLocation = pathToExample + "small-example.7.charTransl";
            translPipelineScriptFileNdExtOnly = "RUN_transl_pipeline.sh";
            translPipelineScript = workingDirectoryLocation + @"RUN_transl_pipeline.sh";
        }

        public string Convert(string source)
        {
            Stopwatch watch = Stopwatch.StartNew();
            return Convert(null, watch, source);
        }

        public string Convert(HttpServerUtilityBase Server, Stopwatch watch, string source)
        {
            // preprocess unicode arabic comma (sould be done at perl level, but somehow does not work)
            source = Preprocess_arabic_comma(source);
            Logging.Write(Server, "train - after train_saveperl > Convert >  Preprocess_arabic_comma : " + watch.ElapsedMilliseconds);
            source = Preprocess_unicode_special_chars(source);
            Logging.Write(Server, "train - after train_saveperl > Convert >  Preprocess_unicode_special_chars : " + watch.ElapsedMilliseconds);
            source = Preprocess_arabic_questionmark(source);

            // preprocess silent wovelles : create artificially vowells (alef) between consonns because we know that there is no more french words that we can distrurb,
            // plus any one kept by bidict is converted temp to 001000100, so no risk,
            // plus we added _VOY_ in ptable to be one of the 3 vowels alef, ya2 or waw
            source = Preprocess_SilentVowels(source);
            Logging.Write(Server, "train - after train_saveperl > Convert >  Preprocess_SilentVowels : " + watch.ElapsedMilliseconds);

            // preprocess &quot;
            source = Preprocess_quotes(source);

            // random naming to avoid access to same file and deadlock eventually
            String randomsuffix = Guid.NewGuid().ToString();
            inputFileLocationFileOnly = randomsuffix;
            inputFileLocation = pathToExample + inputFileLocationFileOnly + ".arabizi";

            // build cygwin cmd with arg included and change slashes
            // target : RUN_transl_pipeline.sh example/small-example_9493cac0-eac6-40db-8be5-1b8a594df13b
            translPipelineScript = translPipelineScriptFileNdExtOnly + " " + "example/" + inputFileLocationFileOnly;

            // to arabizi (INPUT) file
            File.WriteAllText(inputFileLocation, source);
            Logging.Write(Server, "train - after train_saveperl > Convert >  WriteAllText : " + watch.ElapsedMilliseconds);

            //
            var process = new Process();
            var processInformation = new ProcessStartInfo("c:\\cygwin64\\bin\\sh.exe", translPipelineScript)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = false,
                UserName = "user",
                Password = "Ayasfeli%7".ConvertToSecureString(),
                LoadUserProfile = true
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();
            Logging.Write(Server, "train - after train_saveperl > Convert >  ProcessStartInfo : " + watch.ElapsedMilliseconds);

            // random naming to avoid access to same file and deadlock eventually
            outputFileLocation = pathToExample + randomsuffix + ".7.charTransl";

            // read arabic (OUTPUT) file
            var output = File.ReadAllText(outputFileLocation);
            Logging.Write(Server, "train - after train_saveperl > Convert >  ReadAllText : " + watch.ElapsedMilliseconds);

            // delete output files and input files and interediated
            var dir = new DirectoryInfo(pathToExample);
            foreach (var file in dir.EnumerateFiles(randomsuffix + ".*"))
                file.Delete();

            // post-process (eg : hna => nahnou)
            output = Postprocess_slash_r_slash_n(output);
            output = Postprocess_حنا_to_نحن(output);
            output = Postprocess_هاد_to_هذا(output);
            output = Postprocess_هادي_to_هذه(output);
            output = Postprocess_وهيا_to_وهي(output);
            output = Postprocess_أل_to_ال(output);

            return output;
        }

        #region BACK YARD Preprocess UPSTREAM
        public string Preprocess_upstream(string source)
        {
            // preprocess (eg: ma/ch)
            source = Preprocess_wa_ma_ch(source);
            source = Preprocess_li_ma_ch(source);
            source = Preprocess_ma_ch(source);
            // source = Preprocess_le(source);
            // source = Preprocess_al_wa(source);
            source = Preprocess_wal(source);
            source = Preprocess_al(source);
            source = Preprocess_dl(source);
            source = Preprocess_bezzaf(source);
            source = Preprocess_ahaha(source);
            source = Preprocess_3_m_i_f_z_a_j_l_etc(source);
            source = Preprocess_emoticons(source);
            source = Preprocess_underscore(source);
            source = Preprocess_questionmark(source);
            source = Preprocess_wierdquote(source);
            
            //
            return source;
        }
        #endregion

        #region BACK YARD Preprocess
        private string Preprocess_wa_ma_ch(string arabizi)
        {
            // on the contrary, we need to separate
            String pattern = RegexConstant.waMaChRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "wa ma$2ch", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_li_ma_ch(string arabizi)
        {
            // on the contrary, we need to separate
            String pattern = RegexConstant.liMaChRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "li ma$2ch", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        // public for UT only
        public string Preprocess_ma_ch(string arabizi)
        {
            // on the contrary, we need to separate
            String pattern = RegexConstant.maChRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "ما $1 ش", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        /*public string Preprocess_le(string arabizi)
        {
            // ex : on mix le fr avec l'arabe : 'le' dans le sens l'article 'al'
            String pattern = RegexConstant.leRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }*/

        // public for UT only
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

        // public for UT only
        public string Preprocess_al(string arabizi)
        {
            // wa3acha al malik => wa3acha almalik
            String pattern = RegexConstant.alRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "al$2", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_wal(string arabizi)
        {
            // wa3acha al malik => wa3acha almalik
            String pattern = RegexConstant.walRule;
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "wa al", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_dl(string arabizi)
        {
            // wa3acha al malik => wa3acha almalik
            String pattern = RegexConstant.dlRule;

            // buid exceptions : dlam, ...
            // pattern = @"\b(?!" + "dlam" + @")(dl|del).+?\b";
            pattern = @"\b(?!" + "dlam" + @")(dl|del)(.+?)\b";

            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "dyal al$2", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        // public for UT only
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

        private string Preprocess_3_m_i_f_z_a_j_l_etc(string arabizi)
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

            pattern = @"u{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "uu", RegexOptions.IgnoreCase);

            pattern = @"n{3,}";
            miniArabiziKeyword = Regex.Replace(miniArabiziKeyword, pattern, "nn", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        // public for UT only
        public string Preprocess_ahaha(string arabizi)
        {
            //hahahahahahaha
            //ahahahaha
            //ahahahahahahahahaha
            //hahahaha
            //ahahah
            //ahhahahahhahaha

            // String pattern = @"\b(a|h){0,1}(ha|ha){3,}(h|a){0,1}\b";
            String pattern = @"\b(a|h){0,1}(h|a){3,}(h|a){0,1}\b";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "ههههه", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        // public for UT only
        public string Preprocess_emoticons(string arabizi)
        {
            // string text = "a\u2705b\U0001f52ec\u26f1d\U0001F602e\U00010000";
            string cleansed = RemoveOtherSymbols(arabizi);
            // Console.WriteLine(cleansed);

            return cleansed;
        }

        private string Preprocess_underscore(string arabizi)
        {
            String pattern = @"\b(.+)_\b";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "$1 _", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_questionmark(string arabizi)
        {
            String pattern = @"\b(.+)\b\?";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, "$1 ?", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_wierdquote(string arabizi)
        {
            String miniArabiziKeyword = arabizi.Replace("´", "'");

            return miniArabiziKeyword;
        }

        // public for UT only
        public string Preprocess_arabic_comma(string arabizi)
        {
            String pattern = @"\u060C+";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, " , ", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_arabic_questionmark(string arabizi)
        {
            String pattern = @"\u061F+";
            String miniArabiziKeyword = Regex.Replace(arabizi, pattern, " ؟ ", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }

        private string Preprocess_unicode_special_chars(string arabizi)
        {
            String pattern = @"\u0F20+";    // space
            arabizi = Regex.Replace(arabizi, pattern, " ", RegexOptions.IgnoreCase);

            pattern = @"\uFFFE+";   // space
            var miniArabiziKeyword = Regex.Replace(arabizi, pattern, " ", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }
        #endregion

        #region BACK YARD Preprocess SILENT VOWELLS
        public string Preprocess_SilentVowels(string arabizi)
        {
            // n d => n _VOY_ d
            arabizi = Regex.Replace(arabizi, "nd", "n_VOW_d", RegexOptions.IgnoreCase);

            // l k => l _VOY_ k
            arabizi = Regex.Replace(arabizi, "lk", "l_VOW_k", RegexOptions.IgnoreCase);

            // h l => h _VOY_ l
            arabizi = Regex.Replace(arabizi, "hl", "h_VOW_l", RegexOptions.IgnoreCase);

            // final k => k _VOY
            String miniArabiziKeyword = Regex.Replace(arabizi, @"(\w+k\b)", "$1_VOW_", RegexOptions.IgnoreCase);

            return miniArabiziKeyword;
        }
        #endregion

        #region Preprocess QUOTE
        public string Preprocess_quotes(string arabizi)
        {
            // quote : bing/google converts " to &quot; so we need to convert it back to "
            arabizi = Regex.Replace(arabizi, "&quot;", "\'", RegexOptions.IgnoreCase);

            return arabizi;
        }
        #endregion

        #region BACK YARD Postprocess
        public string Postprocess_slash_r_slash_n(string arabic)
        {
            return arabic.TrimEnd('\r', '\n');
        }

        public string Postprocess_حنا_to_نحن(string arabic)
        {
            // replace whole-word only
            // return arabic.Replace("حنا", "نحن");
            return Regex.Replace(arabic, @"\bحنا\b", "نحن");
        }

        public string Postprocess_هاد_to_هذا(string arabic)
        {
            // replace whole-word only
            // return arabic.Replace("هاد", "هذا");
            return Regex.Replace(arabic, @"\bهاد\b", "هذا");
        }

        public string Postprocess_هادي_to_هذه(string arabic)
        {
            // replace whole-word only
            // return arabic.Replace("هاد", "هذا");
            return Regex.Replace(arabic, @"\bهادي\b", "هذه");
        }

        public string Postprocess_وهيا_to_وهي(string arabic)
        {
            // replace whole-word only
            // return arabic.Replace("هاد", "هذا");
            return Regex.Replace(arabic, @"\bوهيا\b", "وهي");
        }

        public string Postprocess_أل_to_ال(string arabic)
        {
            // replace whole-word only
            return Regex.Replace(arabic, @"\bأل", "ال");
        }
        #endregion

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
            var output = new List<string>();

            //
            File.WriteAllText(workingDirectoryLocation + "arabiziword", arabiziWord);

            // script to create all variants only
            string translTranscmScriptFileNdExtOnly = "RUN_transl_transcm.sh";
            string variantsProcFileLoc = workingDirectoryLocation + translTranscmScriptFileNdExtOnly;
            string outputVariantsFileLoc = pathToExample + "out.variants.txt";

            // build cygwin cmd with arg included and change slashes
            // target : RUN_transl_transcm.sh
            var translTranscmScript = translTranscmScriptFileNdExtOnly;

            //
            var process = new Process();
            // var processInformation = new ProcessStartInfo(variantsProcFileLoc)
            var processInformation = new ProcessStartInfo("sh.exe", translTranscmScript)
            {
                WorkingDirectory = workingDirectoryLocation,
                UseShellExecute = true,
            };
            process.StartInfo = processInformation;
            process.Start();
            process.WaitForExit();

            //
            if (File.Exists(outputVariantsFileLoc))
            {
                output = File.ReadAllLines(outputVariantsFileLoc).ToList<String>();
            }

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