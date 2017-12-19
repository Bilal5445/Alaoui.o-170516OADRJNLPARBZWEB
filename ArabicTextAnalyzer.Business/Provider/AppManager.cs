using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class AppManager
    {
        public ClientKeys CreateApp(RegisterApp app, string userId, bool keyExists, IRegisterApp appRegistrar, IClientKeys clientKeyToolkit, int appCallLimit)
        {
            app.UserID = userId;
            app.CreatedOn = DateTime.Now;
            app.TotalAppCallLimit = appCallLimit;
            appRegistrar.Add(app);  // this code fills RegisterAppId and adds the app to db, and should always auto-increment model.RegisterAppId to > 0

            // Generate Clientid and Secret Key
            // Validating ClientID and ClientSecret already Exists
            ClientKeys clientkeys;
            if (keyExists)
            {
                // Getting Generate ClientID and ClientSecret Key By UserID
                clientkeys = clientKeyToolkit.GetGenerateUniqueKeyByUserID(userId);
            }
            else
            {
                // reload app from DB
                app = appRegistrar.FindAppByUserId(userId);

                // Generate Keys
                String clientSecret, clientID;
                clientKeyToolkit.GenerateUniqueKey(out clientID, out clientSecret);

                // Saving Keys Details in Database
                clientkeys = new ClientKeys();
                clientkeys.ClientKeysID = 0;
                clientkeys.RegisterAppId = app.RegisterAppId;
                clientkeys.CreatedOn = DateTime.Now;
                clientkeys.ClientId = clientID;
                clientkeys.ClientSecret = clientSecret;
                clientkeys.UserID = userId;
                clientKeyToolkit.SaveClientIDandClientSecret(clientkeys);

                // MC121517 quick and dirty hack to address the bug at app creation in ManageApp.cshtml where RegisterApps is null in @clientkeys.RegisterApps.Name
                // otherwise ManageApp.cshtml will crash
                clientkeys.RegisterApps = app;
            }

            return clientkeys;
        }

        public string GetToken(ClientKeys clientKeys, IAuthenticate authenticater, String tokenExpiry, out String sessiontoken)
        {
            //
            sessiontoken = null;

            //
            if (string.IsNullOrEmpty(clientKeys.ClientId) && string.IsNullOrEmpty(clientKeys.ClientSecret))
                return "Not Valid Request";

            //
            string result = string.Empty;
            if (authenticater.ValidateKeys(clientKeys))
            {
                var clientkeys = authenticater.GetClientKeysDetailsbyCLientIDandClientSecret(clientKeys.ClientId, clientKeys.ClientSecret);
                if (clientkeys == null)
                {
                    return "InValid Keys";
                }
                else
                {
                    if (authenticater.IsTokenAlreadyExists(clientkeys.RegisterAppId.Value))
                    {
                        authenticater.DeleteGenerateToken(clientkeys.RegisterAppId.Value);

                        var IssuedOn = DateTime.Now;
                        var newToken = authenticater.GenerateToken(clientkeys, IssuedOn);

                        var status = authenticater.InsertToken(clientkeys, tokenExpiry, newToken);
                        if (status == 1)
                        {
                            // Session["_T0k@n_"] = newToken;
                            sessiontoken = newToken;
                            result = "Token generated successfully !!!";
                        }
                        else
                        {
                            result = "Error in Creating Token";
                        }
                    }
                    else
                    {
                        var issuedOn = DateTime.Now;
                        var newToken = authenticater.GenerateToken(clientkeys, issuedOn);

                        var status = authenticater.InsertToken(clientkeys, tokenExpiry, newToken);
                        if (status == 1)
                        {
                            // Session["_T0k@n_"] = newToken;
                            sessiontoken = newToken;
                            result = "Token generated successfully !!!";
                        }
                        else
                        {
                            result = "Error in Creating Token";
                        }
                    }
                }
            }
            else
            {
                result = "Invalid Keys!!!";
            }

            return result;
        }
    }
}
