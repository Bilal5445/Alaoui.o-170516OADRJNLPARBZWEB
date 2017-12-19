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
        public HttpResponseMessage Authenticate(string ClientId, string ClientSecret)
        {
            ClientKeys ClientKeys = new ClientKeys() { ClientId = ClientId, ClientSecret = ClientSecret };

            if (string.IsNullOrEmpty(ClientKeys.ClientId) && string.IsNullOrEmpty(ClientKeys.ClientSecret))
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Not Valid Request");
                return response;
            }
            else if (_IAuthenticate.ValidateKeys(ClientKeys))
            {
                var clientkeys = _IAuthenticate.GetClientKeysDetailsbyCLientIDandClientSecret(ClientKeys.ClientId, ClientKeys.ClientSecret);

                if (clientkeys == null)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response = Request.CreateResponse(HttpStatusCode.NotFound, "InValid Keys");
                    return response;
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
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.NotFound, "InValid Keys");
                return response;
            }
        }

        [NonAction]
        private HttpResponseMessage GenerateandSaveToken(ClientKeys clientkeys)
        {
            var IssuedOn = DateTime.Now;
            var newToken = _IAuthenticate.GenerateToken(clientkeys, IssuedOn);
            var result = _IAuthenticate.InsertToken(clientkeys, ConfigurationManager.AppSettings["TokenExpiry"], newToken);

            if (result == 1)
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.OK, newToken);
                response.Headers.Add("Token", newToken);
                response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["TokenExpiry"]);
                response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");
                return response;
            }
            else
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Error in Creating Token");
                return response;
            }
        }

        public HttpResponseMessage ValidateToken(string token, string methodTocall)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            string errMessage = string.Empty;

            if (!string.IsNullOrEmpty(token))
            {
                token = token.Replace(" ", "+");
                token = token.Replace(@"\", "");
            }
            if (_IAuthenticate.IsTokenValid(Convert.ToString(token), methodTocall, out errMessage))
            {

                response = Request.CreateResponse(HttpStatusCode.OK, "Success");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, errMessage);
            }
            return response;
        }
    }
}
