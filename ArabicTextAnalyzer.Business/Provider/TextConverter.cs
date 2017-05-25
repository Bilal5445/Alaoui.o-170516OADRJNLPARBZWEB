using System.Diagnostics;
using System.IO;
using ArabicTextAnalyzer.Contracts;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextConverter : ITextConverter
    {
        private const string inputFileLocation = @"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\example\small-example.arabizi";

        public string Convert(string source)
        {
            //using (var streamWriter = new StreamWriter(inputFileLocation))
            //{
            //    streamWriter.Write(source);
            //}

            var process = new Process();

            var processInformation = new ProcessStartInfo(@"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f\RUN_transl_pipeline.sh");

            processInformation.WorkingDirectory = @"D:\Projects\Upwork\3\namatedev-17028oadrjnlparbz-991d3268755f\namatedev-17028oadrjnlparbz-991d3268755f";

            processInformation.UseShellExecute = true;
            processInformation.CreateNoWindow = false;

            process.StartInfo = processInformation;

            process.Start();    



            return
                "هناك فصل كامل في حياة أميليا إيرهارت يتجاهل التاريخ، ويقول بحث جديد: توفي الطيار الأمريكي الأسطوري كمرحلة، وليس في حادث تحطم طائرة.";
        }

    }                                                                                                                                                                              
}