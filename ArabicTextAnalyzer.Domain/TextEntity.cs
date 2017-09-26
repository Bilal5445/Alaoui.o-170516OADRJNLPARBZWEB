namespace ArabicTextAnalyzer.Domain
{
    public class TextEntity
    {
        public long Count { get; set; }
        public string EntityId { get; set; }
        public string Mention { get; set; }
        public string Normalized { get; set; }
        public string Type { get; set; }
    }
}