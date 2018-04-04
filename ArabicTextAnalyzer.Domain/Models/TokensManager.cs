using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class TokensManager
    {
        public int? TokensManagerID { get; set; }
        public string TokenKey { get; set; }
        public DateTime? IssuedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public DateTime? CreaatedOn { get; set; }

        public int? RegisterAppId { get; set; }
        [ForeignKey("RegisterAppId")]
        public virtual RegisterApp RegisterApps { get; set; }

        public bool IsDeleted { get; set; }
    }
}
