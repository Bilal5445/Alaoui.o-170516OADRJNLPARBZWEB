using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Models;

namespace ArabicTextAnalyzer.Contracts
{
    public interface ITextSentimentAnalyzer
    {
        TextSentiment GetSentiment(string source);
    }
}
