using System;
using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextConverter : ITextConverter
    {
        private const string outputFileLocation = @"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\example\small-example.7.charTransl";
        private const string inputFileLocation = @"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\example\small-example.arabizi";
        private const string processFileLocation = @"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\RUN_transl_pipeline.sh";
        private const string workingDirectoryLocation = @"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f";

        public string Convert(string source)
        {
            File.WriteAllText(inputFileLocation, source);

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