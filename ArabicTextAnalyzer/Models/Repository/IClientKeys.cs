using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Models.Repository
{
    public interface IClientKeys
    {
        bool IsUniqueKeyAlreadyGenerate(string UserID);
        void GenerateUniqueKey(out string ClientID, out string ClientSecret);
        int SaveClientIDandClientSecret(ClientKeys ClientKeys);
        int UpdateClientIDandClientSecret(ClientKeys ClientKeys);
        ClientKeys GetGenerateUniqueKeyByUserID(string UserID);
        bool IsAppValid(ClientKeys ClientKeys);
    }
}
