using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class RegisterApp
    {
        public int RegisterAppId { get; set; }
        [Required(ErrorMessage = "Required Name")]
        public string Name { get; set; }
        public int TotalAppCallLimit { get; set; }
        public int TotalAppCallConsumed { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UserID { get; set; }
    }

    public class RegisterAppCallingLog
    {
        public RegisterAppCallingLog()
        {
            DateCreatedOn = DateTime.Now;
        }
        public int RegisterAppCallingLogId { get; set; }
        public string UserId { get; set; }
        public int? RegisterAppId { get; set; }
        [ForeignKey("RegisterAppId")]
        public virtual RegisterApp RegisterApps { get; set; }
        public int? TokensManagerID { get; set; }
        [ForeignKey("TokensManagerID")]
        public virtual TokensManager TokensManagers { get; set; }
        public DateTime? DateCreatedOn { get; set; }
        public string MethodName { get; set; }
    }
}
