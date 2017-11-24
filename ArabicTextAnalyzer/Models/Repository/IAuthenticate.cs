using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Models.Repository
{
  public   interface IAuthenticate
    {
        ClientKeys GetClientKeysDetailsbyCLientIDandClientSecert(string clientID, string clientSecert);
        bool ValidateKeys(ClientKeys ClientKeys);
        bool IsTokenAlreadyExists(int CompanyID);
        int DeleteGenerateToken(int CompanyID);
        int InsertToken(ClientKeys clientkeys, string tokenExpiry,string newToken);
        string GenerateToken(ClientKeys ClientKeys, DateTime IssuedOn);
        bool IsTokenValid(string token,string methodName, out string errMsg);
        bool IsTokenExpire(string token);
    }
}
