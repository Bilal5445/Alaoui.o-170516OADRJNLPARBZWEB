using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Contracts
{
    public interface IRegisterApp
    {
        IEnumerable<RegisterApp> ListofApps(string UserID);
        void Add(RegisterApp entity);
        void Delete(RegisterApp entity);
        RegisterApp FindAppByUserId(string UserID);
        bool ValidateAppName(RegisterApp registercompany);
        bool CheckIsAppRegistered(string UserID);
    }
}
