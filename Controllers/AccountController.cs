using Login.Models;
using Login.Services;
using Login.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;


namespace Login.Controllers
{

    [Route("account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IdentityOptions _identityOptions;
        private readonly IStringLocalizer<AccountController> _localizer;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        public List<string> Errors { get; set; } = new();

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityOptions> identityOptions,
            IStringLocalizer<AccountController> localizer,
            IEmailService emailService,
            IWebHostEnvironment env
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityOptions = identityOptions.Value;
            _localizer = localizer;
            _emailService = emailService;
            _env = env;

        }
        [HttpGet("login"), AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost("login"), AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPost(LoginViewModel viewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            ApplicationUser? user = await _userManager.FindByNameAsync(viewModel.UserName);
            if (user == null)
                return RedirectToAction(nameof(Login), new { returnUrl });

            SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName!, viewModel.Password, false, true);


            if (result.IsNotAllowed)
            {
                if (_identityOptions.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToAction(nameof(AccountNotConfirmed), new { user.UserName, returnUrl });
                }

                ModelState.AddModelError("IsNotAllowed", _localizer["IsNotAllowed"]);
                return View(viewModel);
            }
            else if (result.IsLockedOut)
            {
                // TODO : Fix à reporter sur le framework
                int timeSpan = _identityOptions.Lockout.DefaultLockoutTimeSpan.Minutes;
                ModelState.AddModelError(nameof(LoginViewModel.Password), _localizer["IsLockedOut", timeSpan]);
                return View(viewModel);
            }
            else if (result.RequiresTwoFactor)
            {
                throw new NotImplementedException();
                //IList<string> providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
                //if (user.TwoFactorsAuthenticationProvider == "Email" || user.TwoFactorsAuthenticationProvider == "Phone")
                //{
                //    string token = await _userManager.GenerateTwoFactorTokenAsync(user, user.TwoFactorsAuthenticationProvider!);

                //    IMFASender sender;
                //    if (user.TwoFactorsAuthenticationProvider == "Email")
                //    {
                //        sender = _mfaSenders.First(s => s is EmailService);
                //        await sender.SendCode(token, user.Email!);

                //    }
                //    else if (user.TwoFactorsAuthenticationProvider == "Phone")
                //    {
                //        sender = _mfaSenders.First(s => s is SmsService);
                //        await sender.SendCode(token, user.PhoneNumber!);
                //    }
                //    else
                //        throw new Exception("Invalid authentication provider");

                //    if (_env.IsDevelopment())
                //        return RedirectToAction(nameof(TwoFactorAuthenticationLogin), new { returnUrl, user.TwoFactorsAuthenticationProvider, token });

                //}
                //return RedirectToAction(nameof(TwoFactorAuthenticationLogin), new { returnUrl, user.TwoFactorsAuthenticationProvider });
            }
            else if (result.Succeeded)
            {
                if (user.LockoutEnabled)
                    await _userManager.ResetAccessFailedCountAsync(user);
                return Redirect(returnUrl);
            }
            else
            {
                if (user.LockoutEnabled)
                {
                    int remainingAttempt = _identityOptions.Lockout.MaxFailedAccessAttempts - user.AccessFailedCount;
                    ModelState.AddModelError(nameof(LoginViewModel.Password), _localizer["InvalidPasswordWithLockout", remainingAttempt]);
                }
                else
                {
                    ModelState.AddModelError(nameof(LoginViewModel.Password), _localizer["InvalidPassword"]);
                }
                return View(viewModel);
            }

        }
        [HttpGet("register"), AllowAnonymous]
        public IActionResult Register()
        => View();


        [HttpGet("not-confirmed")]
        public IActionResult AccountNotConfirmed(string userName, string returnUrl)
        {
            return View(new AccountNotConfirmedViewModel(userName, returnUrl));
        }
        [HttpPost("register"), AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPost(RegisterViewModel viewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Account/Register.cshtml", viewModel);
            ApplicationUser user = new(viewModel.UserName);

            IdentityResult result = await _userManager.CreateAsync(user, viewModel.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(nameof(RegisterViewModel.UserName), error.Description);
                    //viewModel.Errors.Add(error.Description);

                }
                return View("~/Views/Account/Register.cshtml", viewModel);
            }

            if (_identityOptions.SignIn.RequireConfirmedAccount)
            {
                Uri uri = await SendAccountConfirmationEmailAsync(user, returnUrl);
                if (_env.IsDevelopment())
                    return RedirectToAction(nameof(Created), new { callbackUri = WebUtility.UrlEncode(uri.AbsoluteUri) });
                else
                    return RedirectToAction(nameof(Created), new { returnUrl });
            }
            await _signInManager.SignInAsync(user, false);
            return Redirect(returnUrl);
        }

        private async Task<Uri> SendAccountConfirmationEmailAsync(ApplicationUser user, string returnUrl)
        {
            Uri callbackUri = await GetEmailConfirmationCallbackUri(user, returnUrl);
            await _emailService.SendAccountConfirmationEmailAsync(user.UserName!, callbackUri);
            return callbackUri;
        }

        private async Task<Uri> GetEmailConfirmationCallbackUri(ApplicationUser user, string returnUrl)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string email = WebUtility.UrlEncode(user.UserName);
            returnUrl = WebUtility.UrlEncode(returnUrl);
            string callbackUrl = new StringBuilder(HttpContext.Request.Scheme)
                .Append("://").Append(HttpContext.Request.Host)
                .Append("/account/confirm/?email=").Append(email)
                .Append("&code=").Append(code)
                .Append("&returnUrl=").Append(returnUrl)
                .ToString();
            return new Uri(callbackUrl);
        }
        [HttpGet("created"), AllowAnonymous]
        public IActionResult Created() => View();

        [HttpGet("confirm"), AllowAnonymous]
        public async Task<IActionResult> Confirm([FromQuery] string email, [FromQuery] string code, [FromQuery] string returnUrl)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(AccountNotConfirmed));

            ApplicationUser? user = await _userManager.FindByNameAsync(email) ?? throw new NullReferenceException("Utilisateur introuvable");
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return RedirectToAction(nameof(Confirmed), new { returnUrl });


            return RedirectToAction(nameof(AccountNotConfirmed));
        }

        [HttpGet("confirmed"), AllowAnonymous]
        public IActionResult Confirmed()
           => View();

        [HttpGet("password/forgot"), AllowAnonymous]
        public async Task<IActionResult> ForgotPassword() => View();

        [HttpPost("password/forgot"), AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordPost(ForgotPasswordViewModel viewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Account/ForgotPassword.cshtml");

            ApplicationUser? user = await _userManager.FindByNameAsync(viewModel.UserName);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordEmailSent));

            Uri callbackUri = await GetForgotPasswordCallbackUri(user, returnUrl);

            await _emailService.SendResetPasswordEmailAsync(viewModel.UserName, callbackUri);

            if (_env.IsDevelopment())
                return RedirectToAction(nameof(ForgotPasswordEmailSent), new { callbackUri });
            else
                return RedirectToAction(nameof(ForgotPasswordEmailSent));
        }

        [HttpGet("password/email-sent"), AllowAnonymous]
        public IActionResult ForgotPasswordEmailSent()
        => View();
        private async Task<Uri> GetForgotPasswordCallbackUri(ApplicationUser user, string returnUrl)
        {
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string email = WebUtility.UrlEncode(user.UserName);

            returnUrl = WebUtility.UrlEncode(returnUrl);
            string callbackUrl = new StringBuilder(HttpContext.Request.Scheme)
                .Append("://").Append(HttpContext.Request.Host).Append("/account/password/reset")
                .Append("?email=").Append(email)
                .Append("&code=").Append(code)
                .Append("&returnUrl=").Append(returnUrl).ToString();
            return new Uri(callbackUrl);
        }
        [HttpGet("password/reset"), AllowAnonymous]
        public IActionResult ResetPassword()
        => View();

        [HttpPost("password/reset"), AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordPost(ResetPasswordViewModel viewModel, [FromQuery] string email, [FromQuery] string code, [FromQuery] string returnUrl)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Account/ResetPassword.cshtml");

            ApplicationUser? user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return RedirectToAction(nameof(PasswordReseted));

            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, viewModel.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                    viewModel.Errors.Add(error.Description);

                return View("~/Views/Account/ResetPassword.cshtml", viewModel);
            }

            return RedirectToAction(nameof(PasswordReseted));
        }
        [HttpGet("password/reseted"), AllowAnonymous]
        public IActionResult PasswordReseted()
    => View();
        [HttpPost("password/reseted"), AllowAnonymous]
        public IActionResult PasswordResetedPost()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}

