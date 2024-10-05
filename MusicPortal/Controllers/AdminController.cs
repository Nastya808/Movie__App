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
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        var rolesToAdd = selectedRoles.Except(currentRoles);
        var rolesToRemove = currentRoles.Except(selectedRoles);

        var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
        if (!addRolesResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Failed to add roles");
            return View(model);
        }

        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        if (!removeRolesResult.Succeeded)
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

        return RedirectToAction(nameof(AdminProfile));
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
        return RedirectToAction(nameof(Genres));
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
        return RedirectToAction(nameof(Genres));
    }

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
            var songFilePath = await SaveSongFileAsync(model.SongFile);
            if (songFilePath == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to save the song file.");
                ViewBag.Genres = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name", model.GenreId);
                return View(model);
            }

            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                GenreId = model.GenreId,
                FilePath = songFilePath,
                UserId = User.Identity.Name
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Songs));
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
        return RedirectToAction(nameof(Songs));
    }

    private async Task<string> SaveSongFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "songs", file.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (IOException)
        {
            return null;
        }

        return $"/songs/{file.FileName}";
    }

    public async Task<IActionResult> RegistrationRequests()
    {
        var requests = await _context.RegistrationRequests.ToListAsync();
        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveRequest([FromForm] int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request == null)
        {
            return Json(new { success = false, message = "Request not found" });
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
            request.IsProcessed = true;
            _context.RegistrationRequests.Update(request);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false, message = "Error creating user: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
    }


    [HttpPost]
    public async Task<IActionResult> RejectRequest([FromForm] int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request == null)
        {
            return Json(new { success = false, message = "Request not found" });
        }

        _context.RegistrationRequests.Remove(request);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }


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
