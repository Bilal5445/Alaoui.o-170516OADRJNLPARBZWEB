using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArabicTextAnalyzer.Domain.Models;
namespace ArabicTextAnalyzer.Models.Repository
{
  public  interface IRegisterApp
    {
        IEnumerable<RegisterApp> ListofApps(string UserID);
        void Add(RegisterApp entity);
        void Delete(RegisterApp entity);
        RegisterApp FindAppByUserId(string UserID);
        bool ValidateAppName(RegisterApp registercompany);
        bool CheckIsAppRegistered(string UserID);
    }
}
