using ArabicTextAnalyzer.Domain.AES256Encryption;
using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ArabicTextAnalyzer.Domain.AES256Encryption.EncryptionLibrary;

namespace ArabicTextAnalyzer.Models.Repository
{
    public class AuthenticateConcrete : IAuthenticate
    {
        ArabiziDbContext _context;

        public AuthenticateConcrete()
        {
            _context = new ArabiziDbContext();
        }

        public ClientKeys GetClientKeysDetailsbyCLientIDandClientSecret(string clientID, string clientSecret)
        {
            try
            {
                var result = (from clientkeys in _context.ClientKeys
                              where clientkeys.ClientId == clientID && clientkeys.ClientSecret == clientSecret
                              select clientkeys).FirstOrDefault();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool ValidateKeys(ClientKeys ClientKeys)
        {
            try
            {
                var result = (from clientkeys in _context.ClientKeys
                              where clientkeys.ClientId == ClientKeys.ClientId && clientkeys.ClientSecret == ClientKeys.ClientSecret
                              select clientkeys).Count();
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool IsTokenAlreadyExists(int CompanyID)
        {
            try
            {
                var result = (from token in _context.TokensManager
                              where token.IsDeleted == false && token.RegisterAppId == CompanyID
                              select token).Count();
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteGenerateToken(int CompanyID)
        {
            try
            {
                // sometimes (why?) we can have 2 open tokens so SingleOrDefault, meanwhile fix by FirstOrDefault
                var token = _context.TokensManager.Where(c => c.IsDeleted == false).FirstOrDefault(x => x.RegisterAppId == CompanyID);
                if (token != null)
                {
                    token.IsDeleted = true;

                    return _context.SaveChanges();
                }
                return 1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GenerateToken(ClientKeys ClientKeys, DateTime IssuedOn)
        {
            try
            {
                string randomnumber =
                   string.Join(":", new string[]
                   {   Convert.ToString(ClientKeys.UserID),
                KeyGenerator.GetUniqueKey(),
                Convert.ToString(ClientKeys.RegisterAppId),
                Convert.ToString(IssuedOn.Ticks),
                ClientKeys.ClientId
                   });

                return EncryptionLibrary.EncryptText(randomnumber);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertToken(ClientKeys clientkeys, string tokenExpiry, string newToken)
        {
            try
            {
                // 
                var IssuedOn = DateTime.Now;
                TokensManager token = new TokensManager();
                token.TokensManagerID = 0;
                token.TokenKey = newToken;
                token.RegisterAppId = clientkeys.RegisterAppId;
                token.IssuedOn = IssuedOn;
                token.ExpiresOn = DateTime.Now.AddMinutes(Convert.ToInt32(tokenExpiry));
                token.CreaatedOn = DateTime.Now;

                _context.TokensManager.Add(token);
                return _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool IsTokenValid(string token, string methodName, out string errMsg)
        {
            // This code checks if a token is valid, and increases the consumption counter

            bool flag = false;
            errMsg = string.Empty;

            // null/empty ?
            if (string.IsNullOrEmpty(token))
            {
                errMsg = "Token is required.";
                return flag;
            }

            // exists or not ?
            var tokenExist = _context.TokensManager.Where(c => c.IsDeleted == false).FirstOrDefault(c => c.TokenKey == token);
            if (tokenExist == null)
            {
                errMsg = "Token is invalid.";
                return flag;
            }

            // expired ?
            if (tokenExist.ExpiresOn <= DateTime.Now)
            {
                tokenExist.IsDeleted = true;
                _context.SaveChanges();
                errMsg = "Token Expired";
                return flag;
            }

            // exceeded limit ?
            var registerApp = tokenExist.RegisterApps;
            if (registerApp == null || registerApp.TotalAppCallLimit <= 0)
            {
                errMsg = "Your call limit is bounced. Please contact to support team.";
                return flag;
            }

            // otherwise increases the consumption counter
            registerApp.TotalAppCallLimit = registerApp.TotalAppCallLimit - 1;
            registerApp.TotalAppCallConsumed = registerApp.TotalAppCallConsumed + 1;

            // save info into log
            var apiCallLog = new RegisterAppCallingLog();
            apiCallLog.RegisterAppId = registerApp.RegisterAppId;
            apiCallLog.TokensManagerID = tokenExist.TokensManagerID;
            apiCallLog.UserId = registerApp.UserID;
            apiCallLog.MethodName = methodName;
            _context.RegisterAppCallingLogs.Add(apiCallLog);

            // save into DB consoumption increase change and new log entry
            _context.SaveChanges();
            flag = true;

            //
            return flag;
        }

        public bool IsTokenExpire(string token)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(token))
            {
                var tokenExist = _context.TokensManager.Where(c => c.IsDeleted == false).FirstOrDefault(c => c.TokenKey == token);
                if (tokenExist != null)
                {
                    if (tokenExist.ExpiresOn > DateTime.Now)
                    {
                    }
                    else
                    {
                        tokenExist.IsDeleted = true;
                        _context.SaveChanges();
                        flag = true;
                    }
                }
            }

            return flag;
        }
    }
}
