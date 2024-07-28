using Login.Services.SigninCredentialsService;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

namespace Login.DevTools
{
    public static class DevelopmentCertificateExtension
    {
        public static IServiceCollection AddDevelopmentSignKey(this IServiceCollection services)
        {
            SigningCredentials signingCredentials = CreateDevelopmentSecurityKey();
            SigningCredentialsService signingCredentialsService = new();
            signingCredentialsService.AddSignInCredentials(signingCredentials);
            services.AddSingleton<ISigningCredentialsService>(signingCredentialsService);
            return services;
        }
        private static SigningCredentials CreateDevelopmentSecurityKey()
        {
            string filename = Path.Combine(Directory.GetCurrentDirectory(), "devkey.jwk");
            JsonWebKey jwk;
            if (File.Exists(filename))
            {
                string json = File.ReadAllText(filename);
                jwk = new JsonWebKey(json);
            }
            else
            {
                var key = new RsaSecurityKey(RSA.Create(2048))
                {
                    KeyId = Guid.NewGuid().ToString("N").ToUpperInvariant()
                };
                jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
                jwk.Alg = SecurityAlgorithms.RsaSha256;

                File.WriteAllText(filename, JsonSerializer.Serialize(jwk));
            }
            SigningCredentials credential = new (jwk, jwk.Alg);
            return credential;
        }
    }
}
