using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using FinanceApp.Services.IEmailService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;


        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
        }

        // Other actions like Register, Login, etc. can be added here as needed

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            return RedirectToLocal(returnUrl);
                        }
                        else if (result.IsLockedOut)
                        {
                            ModelState.AddModelError(string.Empty, "User account is locked out.");
                        }
                        else if (result.RequiresTwoFactor)
                        {
                            ModelState.AddModelError(string.Empty, "User requires two-factor authentication.");
                        }
                        else
                        {
                            
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(DashboardController.Index), "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Manually update the EmailConfirmed field
                user.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    return View("ConfirmEmail");
                }
                else
                {
                    // Log the error or show a message to the user
                    return BadRequest("Error updating user after email confirmation.");
                }
            }

            return BadRequest("Error confirming your email.");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName,PreferredCurrency = model.PreferredCurrency };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

                    _logger.LogInformation($"Generated callback URL: {callbackUrl}");
                    await _emailService.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                    _logger.LogInformation($"Email sent to {model.Email} with confirmation link.");

                    return View("~/Views/Shared/EmailSent.cshtml");
                }
                AddErrors(result);
            }
            return View("~/Views/Shared/Register.cshtml", model);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // You may create a ProfileViewModel and pass it to the view if you want to display more user-specific data
            return View(user);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
