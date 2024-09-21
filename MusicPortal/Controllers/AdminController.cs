using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.Models;
using MusicPortal.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task<IActionResult> AdminProfile()
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Или другой способ получения ID администратора
        var admin = await _userManager.FindByIdAsync(adminId);

        if (admin == null)
        {
            return NotFound();
        }

        var users = await _userManager.Users.ToListAsync();
        var roles = _roleManager.Roles.ToList();

        var model = new AdminProfileViewModel
        {
            UserName = admin.UserName,
            Email = admin.Email,
            Users = users,
            Roles = roles
            // Инициализируйте другие свойства модели, если необходимо
        };

        return View(model);
    }


    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> EditUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var roles = _roleManager.Roles.ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles.Select(r => new MusicPortal.ViewModels.SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = userRoles.Contains(r.Name)
            }).ToList(),
            SelectedRoles = userRoles
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound();
        }

        user.UserName = model.UserName;
        user.Email = model.Email;

        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.SelectedRoles.ToList();

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Failed to add roles");
            return View(model);
        }

        result = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Failed to remove roles");
            return View(model);
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Failed to update user");
            return View(model);
        }

        return RedirectToAction("AdminProfile", "Admin");
    }


    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete user");
            }
        }

        return RedirectToAction(nameof(AdminProfile));
    }


// Управление жанрами
public async Task<IActionResult> Genres()
    {
        var genres = await _context.Genres.ToListAsync();
        return View(genres);
    }

    [HttpPost]
    public async Task<IActionResult> AddGenre(string genreName)
    {
        if (!string.IsNullOrWhiteSpace(genreName))
        {
            var genre = new Genre { Name = genreName };
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Genres");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGenre(int genreId)
    {
        var genre = await _context.Genres.FindAsync(genreId);
        if (genre != null)
        {
            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Genres");
    }

    // Управление песнями
    public async Task<IActionResult> Songs()
    {
        var songs = await _context.Songs.Include(s => s.Genre).ToListAsync();
        return View(songs);
    }

    [HttpGet]
    public async Task<IActionResult> AddSong()
    {
        ViewBag.Genres = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddSong(AddSongViewModel model)
    {
        if (ModelState.IsValid)
        {
            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                GenreId = model.GenreId,
                FilePath = await SaveSongFileAsync(model.SongFile),
                UserId = User.Identity.Name
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
            return RedirectToAction("Songs");
        }

        ViewBag.Genres = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name", model.GenreId);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSong(int songId)
    {
        var song = await _context.Songs.FindAsync(songId);
        if (song != null)
        {
            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Songs");
    }

    private async Task<string> SaveSongFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "songs", file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/songs/{file.FileName}";
    }

    // Display registration requests
    public async Task<IActionResult> RegistrationRequests()
    {
        var requests = await _context.RegistrationRequests.ToListAsync();
        return View(requests);
    }

    // Approve a registration request
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

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
            request.IsApproved = true;
            _context.RegistrationRequests.Update(request);
            await _context.SaveChangesAsync();
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Failed to create user");
        }

        return RedirectToAction(nameof(RegistrationRequests));
    }

    // Reject a registration request
    [HttpPost]
    public async Task<IActionResult> RejectRequest(int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request != null)
        {
            _context.RegistrationRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(RegistrationRequests));
    }

    // View a registration request for approval
    public async Task<IActionResult> ViewRequest(int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request == null)
        {
            return NotFound();
        }
        return View(request);
    }
}
