using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.Models;
using MusicPortal.ViewModels;
using System.Threading.Tasks;

namespace MusicPortal.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Identity.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                                     .Include(u => u.Songs)
                                     .FirstOrDefaultAsync(u => u.UserName == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                User = user,
                UserSongs = await _context.Songs.Where(s => s.UserId == user.Id).ToListAsync(),
                Genres = await _context.Genres.ToListAsync()
            };

            return View(model);
        }
    }
}
