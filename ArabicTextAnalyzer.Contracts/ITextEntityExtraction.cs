using System.Collections.Generic;
using ArabicTextAnalyzer.Models;

namespace ArabicTextAnalyzer.Contracts
{
    public interface ITextEntityExtraction
    {
        IEnumerable<TextEntity> GetEntities(string source);
    }
}
