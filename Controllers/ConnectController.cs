using Login.Services.SigninCredentialsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;

namespace Login.Controllers
{
    [ApiController]
    [Route("connect")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes("application/x-www-form-urlencoded", "application/json")]

    public class ConnectController : Controller
    {
        private readonly ISigningCredentialsService _signingCredentialsService;

        public ConnectController(ISigningCredentialsService signingCredentialsService)
        {
            _signingCredentialsService = signingCredentialsService;
        }
        [AllowAnonymous]
        [HttpGet("jwks")]
        public IActionResult GetKeys()
        {
            return Ok(_signingCredentialsService.GetKeys());
        }
        [Authorize]
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
                return Unauthorized();

            ClaimsIdentity? subject = User.Identities.FirstOrDefault();
            if (subject == null)
                return Unauthorized();

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Claims = new Dictionary<string, object>()
            };
            tokenDescriptor.Subject = subject;
            tokenDescriptor.Claims.Add("sub", subject.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).Select(s => s.Value).FirstOrDefault());
            tokenDescriptor.Expires = DateTime.UtcNow.AddHours(1);
            tokenDescriptor.Issuer = $"https://{HttpContext.Request.Host}";
            tokenDescriptor.IssuedAt = DateTime.UtcNow;
            tokenDescriptor.NotBefore = DateTime.UtcNow;
            tokenDescriptor.TokenType = JwtConstants.TokenType;
            tokenDescriptor.SigningCredentials = _signingCredentialsService.GetSignInCredentials();


            JwtSecurityTokenHandler handler = new();
            string encodedJwt = handler.CreateEncodedJwt(tokenDescriptor);
            return Ok(encodedJwt);

        }
    }
}
