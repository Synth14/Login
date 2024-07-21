using Login.Models;
using Login.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;


namespace Login.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IdentityOptions _identityOptions;
        private readonly IStringLocalizer<AccountController> _localizer;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityOptions> identityOptions,
            IStringLocalizer<AccountController> localizer
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityOptions = identityOptions.Value;
            _localizer = localizer;

        }
        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
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
    }
}

