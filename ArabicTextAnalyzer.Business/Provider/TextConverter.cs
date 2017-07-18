using System;
using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;

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
        // -> the script
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
            return output;
        }
    }
}