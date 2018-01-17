using ArabicTextAnalyzer.Domain;

namespace ArabicTextAnalyzer.Contracts
{
    public interface ITextSentimentAnalyzer
    {
        TextSentiment GetSentiment(string source);
    }
}
