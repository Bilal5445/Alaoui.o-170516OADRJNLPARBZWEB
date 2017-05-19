using ArabicTextAnalyzer.Contracts;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextConverter : ITextConverter
    {
        public string Convert(string source)
        {
            return
                "هناك فصل كامل في حياة أميليا إيرهارت يتجاهل التاريخ، ويقول بحث جديد: توفي الطيار الأمريكي الأسطوري كمرحلة، وليس في حادث تحطم طائرة.";
        }
    }                                                                                                                                                                              
}