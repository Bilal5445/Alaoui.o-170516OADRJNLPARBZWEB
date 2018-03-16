using ArabicTextAnalyzer.Content.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace ArabicTextAnalyzer.BO
{
    /*public class LocalizationHelper
    {
    }*/

    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string resourceId)
            : base(GetMessageFromResource(resourceId))
        { }

        private static string GetMessageFromResource(string resourceId)
        {
            // TODO: Return the string from the resource file
            return R.ResourceManager.GetString(resourceId);
        }
    }
}