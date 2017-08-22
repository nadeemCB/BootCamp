using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;
using NLog;
using System.ServiceModel.Security.Tokens;

namespace BootCamp.Web.Handler
{
    public class JwtHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        public JwtHandler()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                                              CancellationToken cancellationToken)
        {
            byte[] key = Convert.FromBase64String(ConfigurationManager.AppSettings.Get("SecurityKey"));
            var headers = request.Headers;
            try
            {
                if (headers.Authorization != null)
                {
                    if (headers.Authorization.Scheme.ToLower().Equals("bearer"))
                    {
                        string jwt = request.Headers.Authorization.Parameter;

                        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                        TokenValidationParameters parms = new TokenValidationParameters()
                        {
                            ValidAudience = ConfigurationManager.AppSettings.Get("Audiance"),
                            ValidateAudience = true,
                            ValidIssuers = new List<string>() { ConfigurationManager.AppSettings.Get("ISSUER") },
                            ValidateIssuer = true,
                            ValidateLifetime = true,
                            IssuerSigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(ConfigurationManager.AppSettings.Get("SecurityKey")))
                        };
                        SecurityToken validatedToken;
                        var principal = tokenHandler.ValidateToken(jwt, parms,out validatedToken);

                        Thread.CurrentPrincipal = principal;

                        if (HttpContext.Current != null)
                            HttpContext.Current.User = principal;
                    }
                }

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "error=\"invalid_token\""));
                }
                return response;
            }
            catch (Exception ex)
            {
                var response = request.CreateResponse(HttpStatusCode.Unauthorized);
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "error=\"invalid_token\""));
                
                logger.Log<Exception>(LogLevel.Error, ex);
                return response;
            }
        }
    }
}
