using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TwinglyTools
    {
        public void upddateCountTwinglyAccount(String twinglyApi15Url, String twinglyApiKey, String path)
        {
            // upddate count twingly account
            var calls_free = OADRJNLPCommon.Business.Business.getTwinglyAccountInfo_calls_free(twinglyApi15Url, twinglyApiKey);
            // var path = Server.MapPath("~/App_Data/data_M_TWINGLYACCOUN.txt");
            new TextPersist().Serialize_Update_M_TWINGLYACCOUNT_calls_free(new Guid(twinglyApiKey), calls_free, path);
        }
    }
}
