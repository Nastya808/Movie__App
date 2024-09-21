using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
                    var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
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
        public async Task<IActionResult> ApproveRegistration(int requestId)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            var user = new ApplicationUser { UserName = request.Username, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                request.IsApproved = true;
                await _context.SaveChangesAsync();
                return RedirectToAction("RegistrationRequests");
            }

            return RedirectToAction("RegistrationRequests");
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
