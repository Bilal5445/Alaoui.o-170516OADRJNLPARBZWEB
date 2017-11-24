using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ArabicTextAnalyzer.Controllers
{
    public class AuthenticateController : ApiController
    {
        IAuthenticate _IAuthenticate;
        public AuthenticateController()
        {
            _IAuthenticate = new AuthenticateConcrete();
        }

        // POST: api/Authenticate
        public HttpResponseMessage Authenticate([FromBody]ClientKeys ClientKeys)
        {
            if (string.IsNullOrEmpty(ClientKeys.ClientId) && string.IsNullOrEmpty(ClientKeys.ClientSecret))
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                message.Content = new StringContent("Not Valid Request");
                return message;
            }
            else
            {
                if (_IAuthenticate.ValidateKeys(ClientKeys))
                {
                    var clientkeys = _IAuthenticate.GetClientKeysDetailsbyCLientIDandClientSecert(ClientKeys.ClientId, ClientKeys.ClientSecret);

                    if (clientkeys == null)
                    {
                        var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                        message.Content = new StringContent("InValid Keys");
                        return message;
                    }
                    else
                    {
                        if (_IAuthenticate.IsTokenAlreadyExists(clientkeys.RegisterAppId.Value))
                        {
                            _IAuthenticate.DeleteGenerateToken(clientkeys.RegisterAppId.Value);

                            return GenerateandSaveToken(clientkeys);
                        }
                        else
                        {
                            return GenerateandSaveToken(clientkeys);
                        }
                    }
                }
                else
                {
                    var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                    message.Content = new StringContent("InValid Keys");
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable };
                }
            }
        }


        [NonAction]
        private HttpResponseMessage GenerateandSaveToken(ClientKeys clientkeys)
        {
            var IssuedOn = DateTime.Now;
            var newToken = _IAuthenticate.GenerateToken(clientkeys, IssuedOn);
            //TokensManager token = new TokensManager();
            //token.TokensManagerID = 0;
            //token.TokenKey = newToken;
            //token.RegisterAppId = clientkeys.RegisterAppId;
            //token.IssuedOn = IssuedOn;
            //token.ExpiresOn = DateTime.Now.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["TokenExpiry"]));
            //token.CreaatedOn = DateTime.Now;
            var result = _IAuthenticate.InsertToken(clientkeys, ConfigurationManager.AppSettings["TokenExpiry"], newToken);

            if (result == 1)
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.OK, "Authorized");
                response.Headers.Add("Token", newToken);
                response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["TokenExpiry"]);
                response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");
                return response;
            }
            else
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                message.Content = new StringContent("Error in Creating Token");
                return message;
            }
        }
    }
}
