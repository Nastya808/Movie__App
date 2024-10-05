using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicPortal.Data;
using MusicPortal.Models;
using MusicPortal.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace MusicPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IPasswordValidator<ApplicationUser> _passwordValidator;

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger, IPasswordValidator<ApplicationUser> passwordValidator)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _passwordValidator = passwordValidator; 
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
                        ModelState.AddModelError(string.Empty, "Your registration request is still pending approval.");
                        return View(model);
                    }

                    // Proceed to sign in if user is found and approved
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
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
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                // Validate the password
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

            var result = await _userManager.CreateAsync(user, request.Password); // Pass the password here
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
                    registrationRequest.IsApproved = true; // Mark the request as approved
                    registrationRequest.IsProcessed = true; // Mark as processed
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
