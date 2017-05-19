using System;
using System.Collections.Generic;
using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Models;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextEntityExtraction : ITextEntityExtraction
    {
        public IEnumerable<TextEntity> GetEntities(string source)
        {
            throw new NotImplementedException();
        }
    }
}