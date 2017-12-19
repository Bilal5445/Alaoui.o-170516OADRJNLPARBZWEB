using ArabicTextAnalyzer.Domain.Models;
using System;

namespace ArabicTextAnalyzer.Contracts
{
    public interface IAuthenticate
    {
        ClientKeys GetClientKeysDetailsbyCLientIDandClientSecret(string clientID, string clientSecret);
        bool ValidateKeys(ClientKeys ClientKeys);
        bool IsTokenAlreadyExists(int CompanyID);
        int DeleteGenerateToken(int CompanyID);
        int InsertToken(ClientKeys clientkeys, string tokenExpiry, string newToken);
        string GenerateToken(ClientKeys ClientKeys, DateTime IssuedOn);
        bool IsTokenValid(string token, string methodName, out string errMsg);
        bool IsTokenExpire(string token);
    }
}
