using BootCamp.DomainObjects;
using System;
using System.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;

using System.Security.Claims;


namespace BootCamp.Web.Helpers
{
    public static class TokenGenerator
    {
        
        public static string ProcessRequest(User user)
        {
            // Use a hard-coded key
            byte[] key = Convert.FromBase64String(ConfigurationManager.AppSettings.Get("SecurityKey"));

            var signingCredentials = new SigningCredentials(
                                                           new InMemorySymmetricSecurityKey(key),
                                                                  SecurityAlgorithms.HmacSha256Signature,
                                                                             SecurityAlgorithms.Sha256Digest);

            var descriptor = new SecurityTokenDescriptor()
            {
                TokenIssuerName = ConfigurationManager.AppSettings.Get("ISSUER"),
                AppliesToAddress = ConfigurationManager.AppSettings.Get("Audiance"),
                Lifetime = new Lifetime(DateTime.UtcNow, DateTime.Now.AddDays(1)),
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(
                new Claim[]
                {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.MobilePhone,user.PhoneNumber),
                        new Claim(ClaimTypes.SerialNumber,Convert.ToString(user.Id))
                })
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
