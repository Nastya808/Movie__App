using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.Models;
using MusicPortal.ViewModels;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;


namespace MusicPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IPasswordValidator<ApplicationUser> _passwordValidator;
        private readonly IStringLocalizer<AccountController> _localizer;

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger,
                                  IPasswordValidator<ApplicationUser> passwordValidator,
                                  IStringLocalizer<AccountController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _passwordValidator = passwordValidator;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {

                returnUrl = Url.Action("Login", "Account"); // URL по умолчанию - Login

                if (Request.Path.Value.Contains("Register", StringComparison.OrdinalIgnoreCase))
                {
                    returnUrl = Url.Action("Register", "Account"); 
                }
            }

            return LocalRedirect(returnUrl);
        }



        [HttpGet]
        public IActionResult Profile()
        {
            var user = _userManager.GetUserAsync(User).Result;
            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = _localizer["Login"]; // Локализованное название
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    var registrationRequest = await _context.RegistrationRequests
                        .FirstOrDefaultAsync(r => r.Username == user.UserName);

                    if (registrationRequest != null && !registrationRequest.IsApproved)
                    {
                        ModelState.AddModelError(string.Empty, _localizer["PendingApproval"]); // Локализованное сообщение
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localizer["InvalidLoginAttempt"]); // Локализованное сообщение
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["UserNotFound"]); // Локализованное сообщение
                }
            }
            return View(model);
        }



        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = _localizer["Register"]; // Локализованное название
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var validationResults = await _passwordValidator.ValidateAsync(_userManager, user, model.Password);
                if (validationResults.Succeeded)
                {
                    var registrationRequest = new RegistrationRequest
                    {
                        Username = model.Username,
                        Email = model.Email,
                        Password = model.Password,
                        IsApproved = false
                    };

                    _context.RegistrationRequests.Add(registrationRequest);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("RegistrationPending");
                }
                else
                {
                    foreach (var error in validationResults.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }


        public IActionResult RegistrationPending()
        {
            return View();
        }

        public async Task<IActionResult> RegistrationRequests()
        {
            var requests = await _context.RegistrationRequests
                .Where(r => !r.IsApproved)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request == null)
            {
                return NotFound();
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password); 
            if (result.Succeeded)
            {
                request.IsApproved = true;
                _context.RegistrationRequests.Update(request);
                await _context.SaveChangesAsync();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogError($"Error creating user: {error.Description}");
                }
            }

            return RedirectToAction(nameof(RegistrationRequests));
        }

        public async Task<IActionResult> ApproveUser(int requestId)
        {
            var registrationRequest = await _context.RegistrationRequests.FindAsync(requestId);
            if (registrationRequest != null && !registrationRequest.IsApproved)
            {
                // Create a new user
                var user = new ApplicationUser
                {
                    UserName = registrationRequest.Username,
                    Email = registrationRequest.Email
                };

                var result = await _userManager.CreateAsync(user, registrationRequest.Password);

                if (result.Succeeded)
                {
                    registrationRequest.IsApproved = true; 
                    registrationRequest.IsProcessed = true; 
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    // Handle errors 
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Request not found or already approved.");
            }

            return View();
        }




        [HttpPost]
        public async Task<IActionResult> RejectRegistration(int requestId)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            _context.RegistrationRequests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("RegistrationRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RegisterRequest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterRequest(RegisterRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new RegistrationRequest
                {
                    Username = model.Username,
                    Email = model.Email,
                    RequestedBy = User.Identity.IsAuthenticated ? User.Identity.Name : "Anonymous"
                };

                _context.RegistrationRequests.Add(request);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }




}
