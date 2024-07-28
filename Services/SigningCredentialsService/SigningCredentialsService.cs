using Microsoft.IdentityModel.Tokens;

namespace Login.Services.SigninCredentialsService
{
    public class SigningCredentialsService : ISigningCredentialsService
    {
        private List<SigningCredentials> _signinCredentials;
        public SigningCredentialsService()
        {
            _signinCredentials = new();
        }
        public void AddSignInCredentials(SigningCredentials credentials) => _signinCredentials.Add(credentials);
        public IEnumerable<object> GetKeys() => _signinCredentials.Select(s => (JsonWebKey)s.Key).Select(s => new { s.Kty, s.Kid, s.E, s.N, s.Alg });
        public SigningCredentials GetSignInCredentials() => _signinCredentials.First();
    }
}
