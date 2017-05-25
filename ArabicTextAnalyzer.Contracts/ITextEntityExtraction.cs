using System.Collections.Generic;
using ArabicTextAnalyzer.Domain;

namespace ArabicTextAnalyzer.Contracts
{
    public interface ITextEntityExtraction
    {
        IEnumerable<TextEntity> GetEntities(string source);
    }
}
