using Microsoft.IdentityModel.Tokens;

namespace Login.Services.SigninCredentialsService
{
    public interface ISigningCredentialsService
    {
        void AddSignInCredentials(SigningCredentials credentials);
        IEnumerable<object> GetKeys();
        SigningCredentials GetSignInCredentials();
    }
}