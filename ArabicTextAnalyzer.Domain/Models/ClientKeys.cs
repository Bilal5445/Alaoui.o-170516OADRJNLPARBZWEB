using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace ArabicTextAnalyzer.Domain.Models
{
    public class ClientKeys
    {
        public int ClientKeysID { get; set; }
        public int? RegisterAppId { get; set; }
        [ForeignKey("RegisterAppId")]
        public virtual RegisterApp RegisterApps { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UserID { get; set; }
        //[ForeignKey("UserID")]
        //public virtual RegisterUser RegisterUsers { get; set; }


      

    }
}
